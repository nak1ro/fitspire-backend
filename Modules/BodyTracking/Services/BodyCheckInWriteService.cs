using backend.Data;
using backend.Modules.BodyTracking.Contracts;
using backend.Modules.BodyTracking.Domain;
using backend.Modules.Media.Domain;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace backend.Modules.BodyTracking.Services;

public interface IBodyCheckInWriteService
{
    Task<Guid> CreateAsync(Guid userId, CreateBodyCheckInRequest request, CancellationToken cancellationToken);
    Task UpdateAsync(Guid userId, Guid checkInId, UpdateBodyCheckInRequest request, CancellationToken cancellationToken);
    Task SoftDeleteAsync(Guid userId, Guid checkInId, CancellationToken cancellationToken);
}

public class BodyCheckInWriteService : IBodyCheckInWriteService
{
    private readonly FitspireDbContext _context;
    private readonly IBodyCheckInTimeZoneService _timeZoneService;

    public BodyCheckInWriteService(FitspireDbContext context, IBodyCheckInTimeZoneService timeZoneService)
    {
        _context = context;
        _timeZoneService = timeZoneService;
    }

    public async Task<Guid> CreateAsync(Guid userId, CreateBodyCheckInRequest request, CancellationToken cancellationToken)
    {
        await EnsureDateIsNotFutureAsync(userId, request.CheckInDate, cancellationToken);
        await using var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        await EnsureNoActiveCheckInForDateAsync(userId, request.CheckInDate, null, cancellationToken);

        var photo = await LoadPhotoForAttachmentAsync(userId, request.PhotoMediaId, null, cancellationToken);
        photo?.Attach(DateTime.UtcNow);
        var checkIn = BodyCheckIn.Create(Guid.NewGuid(), userId, request.CheckInDate,
            request.WeightKg, request.BodyFatPercent, request.WaistCm, request.ChestCm, request.HipsCm,
            request.ArmCm, request.ThighCm, request.WellbeingScore, request.Note, photo?.Id);

        _context.BodyCheckIns.Add(checkIn);
        await _context.SaveChangesAsync(cancellationToken);
        await CommitAsync(transaction, cancellationToken);
        return checkIn.Id;
    }

    public async Task UpdateAsync(Guid userId, Guid checkInId, UpdateBodyCheckInRequest request, CancellationToken cancellationToken)
    {
        await EnsureDateIsNotFutureAsync(userId, request.CheckInDate, cancellationToken);
        await using var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        var checkIn = await LoadOwnedActiveCheckInAsync(userId, checkInId, cancellationToken);
        await EnsureNoActiveCheckInForDateAsync(userId, request.CheckInDate, checkIn.Id, cancellationToken);

        var finalPhotoId = await ResolveFinalPhotoIdAsync(userId, checkIn, request, cancellationToken);
        var previousPhotoId = checkIn.PhotoMediaId;
        checkIn.ChangeDate(request.CheckInDate);
        checkIn.Update(request.WeightKg, request.BodyFatPercent, request.WaistCm, request.ChestCm, request.HipsCm,
            request.ArmCm, request.ThighCm, request.WellbeingScore, request.Note, finalPhotoId);

        await RetireReplacedPhotoAsync(previousPhotoId, finalPhotoId, checkIn.Id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await CommitAsync(transaction, cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid userId, Guid checkInId, CancellationToken cancellationToken)
    {
        var checkIn = await LoadOwnedActiveCheckInAsync(userId, checkInId, cancellationToken);
        checkIn.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid?> ResolveFinalPhotoIdAsync(Guid userId, BodyCheckIn checkIn,
        UpdateBodyCheckInRequest request, CancellationToken cancellationToken)
    {
        if (request.PhotoOperation == BodyCheckInPhotoOperation.Keep)
            return checkIn.PhotoMediaId;
        if (request.PhotoOperation == BodyCheckInPhotoOperation.Remove)
            return null;

        var photo = await LoadPhotoForAttachmentAsync(userId, request.PhotoMediaId, checkIn.Id, cancellationToken);
        photo?.Attach(DateTime.UtcNow);
        return photo?.Id;
    }

    private async Task<BodyCheckIn> LoadOwnedActiveCheckInAsync(Guid userId, Guid checkInId, CancellationToken cancellationToken)
    {
        return await _context.BodyCheckIns.FirstOrDefaultAsync(checkIn =>
                checkIn.Id == checkInId && checkIn.UserId == userId && checkIn.DeletedAt == null,
                cancellationToken)
            ?? throw new NotFoundException("Body check-in was not found.");
    }

    private async Task EnsureNoActiveCheckInForDateAsync(Guid userId, DateOnly date, Guid? excludedId, CancellationToken cancellationToken)
    {
        var query = _context.BodyCheckIns.Where(checkIn =>
            checkIn.UserId == userId && checkIn.CheckInDate == date && checkIn.DeletedAt == null);
        if (excludedId.HasValue)
            query = query.Where(checkIn => checkIn.Id != excludedId.Value);

        var exists = await query.AnyAsync(cancellationToken);
        if (exists)
            throw new ConflictException("An active body check-in already exists for this local date.");
    }

    private async Task<MediaAsset?> LoadPhotoForAttachmentAsync(Guid userId, Guid? photoMediaId, Guid? currentCheckInId,
        CancellationToken cancellationToken)
    {
        if (!photoMediaId.HasValue)
            return null;

        var photo = await _context.MediaAssets.FirstOrDefaultAsync(asset =>
                asset.Id == photoMediaId && asset.OwnerUserId == userId,
                cancellationToken)
            ?? throw new NotFoundException("Progress photo upload was not found.");

        var alreadyAttachedHere = currentCheckInId.HasValue && await _context.BodyCheckIns.AnyAsync(checkIn =>
            checkIn.Id == currentCheckInId && checkIn.PhotoMediaId == photo.Id, cancellationToken);
        if (photo.Purpose != MediaPurpose.BodyProgressPhoto || (photo.Status != MediaStatus.Ready && !alreadyAttachedHere))
            throw new DomainException("Progress photo must be an owned, ready body-progress upload.");

        var usedElsewhere = await _context.BodyCheckIns.AnyAsync(checkIn =>
            checkIn.Id != currentCheckInId && checkIn.PhotoMediaId == photo.Id, cancellationToken);
        if (usedElsewhere)
            throw new ConflictException("Progress photo is already attached to another body check-in.");

        return photo;
    }

    private async Task RetireReplacedPhotoAsync(Guid? previousPhotoId, Guid? finalPhotoId, Guid checkInId,
        CancellationToken cancellationToken)
    {
        if (!previousPhotoId.HasValue || previousPhotoId == finalPhotoId)
            return;

        var stillReferenced = await _context.BodyCheckIns.AnyAsync(checkIn =>
            checkIn.Id != checkInId && checkIn.PhotoMediaId == previousPhotoId, cancellationToken);
        if (stillReferenced)
            return;

        var previous = await _context.MediaAssets.FirstOrDefaultAsync(asset => asset.Id == previousPhotoId, cancellationToken);
        previous?.Retire(DateTime.UtcNow);
    }

    private async Task EnsureDateIsNotFutureAsync(Guid userId, DateOnly date, CancellationToken cancellationToken)
    {
        var today = await _timeZoneService.GetTodayAsync(userId, cancellationToken);
        if (date > today)
            throw new DomainException("Body check-in date cannot be in the future in the user's timezone.");
    }

    private async Task<IDbContextTransaction?> BeginTransactionIfNeededAsync(CancellationToken cancellationToken) =>
        _context.Database.CurrentTransaction is null
            ? await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            : null;

    private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);
}
