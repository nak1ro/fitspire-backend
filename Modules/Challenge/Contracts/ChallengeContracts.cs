using backend.Modules.Challenge.Domain.Constants;
using backend.Modules.Media.Contracts;
using FluentValidation;

namespace backend.Modules.Challenge.Contracts;

public record CreateChallengeRequest(
    string Title, string? Description, string MetricCode, string? WorkoutType, string Mode, double? TargetValue,
    string Visibility, DateTime StartDate, DateTime EndDate, string JoinClosing, int ParticipantLimit = 100);

public record UpdateChallengeRequest(
    string Title, string? Description, string MetricCode, string? WorkoutType, string Mode, double? TargetValue,
    string Visibility, DateTime StartDate, DateTime EndDate, string JoinClosing, int ParticipantLimit);

public record UpdateActiveChallengeCopyRequest(string Title, string? Description);

public record InviteChallengeUserRequest(Guid UserId);
public record ChallengeListFilter(string? Role = null, string? Status = null, string? MetricCode = null, int Page = 1, int PageSize = 20);

public record ChallengeResponse(
    Guid Id, string Title, string? Description, string MetricCode, string? WorkoutType, string Mode, double? TargetValue,
    string Visibility, DateTime StartDate, DateTime EndDate, string JoinClosing, int ParticipantLimit, string Status,
    int ParticipantsCount, bool IsJoined);

public record ChallengeDetailResponse(
    Guid Id, string Title, string? Description, string MetricCode, string? WorkoutType, string Mode, double? TargetValue,
    string Visibility, DateTime StartDate, DateTime EndDate, string JoinClosing, int ParticipantLimit, string Status,
    ChallengeCreatorResponse Creator, int ParticipantsCount, ChallengeViewerState Viewer);

public record ChallengeCreatorResponse(Guid UserId, string UserName, string DisplayName, string? ProfilePictureUrl, MediaResponse? ProfilePicture);
public record ChallengeViewerState(bool IsCreator, string? MembershipStatus, double? Score, double? ProgressPercent, bool CanJoin, bool CanManage);
public record ChallengeLeaderboardEntry(Guid UserId, string DisplayName, string? ProfilePictureUrl, MediaResponse? ProfilePicture, double Score, int Rank, double? ProgressPercent);
public record ChallengeResultEntry(Guid UserId, string DisplayName, string? ProfilePictureUrl, MediaResponse? ProfilePicture,
    double Score, int Rank, double? ProgressPercent, bool IsFinisher, bool IsWinner);
public record ChallengePageResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
public record ChallengeInvitationResponse(Guid Id, Guid ChallengeId, string ChallengeTitle, Guid InvitedByUserId, string InvitedByDisplayName, DateTime StartDate, DateTime EndDate, string Status, DateTime CreatedAt);
public record SentChallengeInvitationResponse(Guid Id, Guid InvitedUserId, string InvitedUserDisplayName, string? InvitedUserAvatarUrl, MediaResponse? InvitedUserAvatar, string Status, DateTime CreatedAt);

public class CreateChallengeRequestValidator : AbstractValidator<CreateChallengeRequest>
{
    public CreateChallengeRequestValidator()
    {
        ChallengeRequestRules.Apply(this, item => item.Title, item => item.Description, item => item.MetricCode, item => item.Mode,
            item => item.TargetValue, item => item.Visibility, item => item.StartDate, item => item.EndDate, item => item.JoinClosing, item => item.ParticipantLimit);
        RuleFor(item => item.WorkoutType).Must(value => value is null or "gym" or "running" or "cycling" or "swimming" or "yoga")
            .WithMessage("Challenge workout type is not supported.");
    }
}

public class UpdateChallengeRequestValidator : AbstractValidator<UpdateChallengeRequest>
{
    public UpdateChallengeRequestValidator()
    {
        ChallengeRequestRules.Apply(this, item => item.Title, item => item.Description, item => item.MetricCode, item => item.Mode,
            item => item.TargetValue, item => item.Visibility, item => item.StartDate, item => item.EndDate, item => item.JoinClosing, item => item.ParticipantLimit);
        RuleFor(item => item.WorkoutType).Must(value => value is null or "gym" or "running" or "cycling" or "swimming" or "yoga")
            .WithMessage("Challenge workout type is not supported.");
    }
}

public class InviteChallengeUserRequestValidator : AbstractValidator<InviteChallengeUserRequest>
{
    public InviteChallengeUserRequestValidator() => RuleFor(item => item.UserId).NotEmpty();
}

public class UpdateActiveChallengeCopyRequestValidator : AbstractValidator<UpdateActiveChallengeCopyRequest>
{
    public UpdateActiveChallengeCopyRequestValidator()
    {
        RuleFor(item => item.Title).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Description).MaximumLength(1000);
    }
}

public class ChallengeListFilterValidator : AbstractValidator<ChallengeListFilter>
{
    public ChallengeListFilterValidator()
    {
        RuleFor(item => item.Role).Must(value => value is null or "Created" or "Joined").WithMessage("Role must be Created or Joined.");
        RuleFor(item => item.Status).Must(value => value is null or ChallengeStatuses.Upcoming or ChallengeStatuses.Active or ChallengeStatuses.Completed or ChallengeStatuses.Cancelled)
            .WithMessage("Challenge status is invalid.");
        RuleFor(item => item.Page).GreaterThan(0);
        RuleFor(item => item.PageSize).InclusiveBetween(1, 100);
    }
}

internal static class ChallengeRequestRules
{
    public static void Apply<T>(AbstractValidator<T> validator,
        System.Linq.Expressions.Expression<Func<T, string>> title,
        System.Linq.Expressions.Expression<Func<T, string?>> description,
        System.Linq.Expressions.Expression<Func<T, string>> metricCode,
        System.Linq.Expressions.Expression<Func<T, string>> mode,
        System.Linq.Expressions.Expression<Func<T, double?>> targetValue,
        System.Linq.Expressions.Expression<Func<T, string>> visibility,
        System.Linq.Expressions.Expression<Func<T, DateTime>> startDate,
        System.Linq.Expressions.Expression<Func<T, DateTime>> endDate,
        System.Linq.Expressions.Expression<Func<T, string>> joinClosing,
        System.Linq.Expressions.Expression<Func<T, int>> participantLimit)
    {
        validator.RuleFor(title).NotEmpty().MaximumLength(120);
        validator.RuleFor(description).MaximumLength(1000);
        validator.RuleFor(metricCode).NotEmpty().MaximumLength(80);
        validator.RuleFor(mode).Must(value => value is ChallengeModes.Target or ChallengeModes.Leaderboard);
        validator.RuleFor(targetValue).GreaterThan(0).When(item => mode.Compile()(item) == ChallengeModes.Target);
        validator.RuleFor(visibility).Must(value => value is ChallengeVisibilities.Public or ChallengeVisibilities.FollowersOnly or ChallengeVisibilities.InviteOnly);
        validator.RuleFor(joinClosing).Must(value => value is ChallengeJoinClosingModes.AtStart or ChallengeJoinClosingModes.AtEnd);
        validator.RuleFor(participantLimit).InclusiveBetween(2, 100);
        validator.RuleFor(endDate).GreaterThanOrEqualTo(item => startDate.Compile()(item).AddDays(1));
        validator.RuleFor(endDate).LessThanOrEqualTo(item => startDate.Compile()(item).AddYears(1));
    }
}
