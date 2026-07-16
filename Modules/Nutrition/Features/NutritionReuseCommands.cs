using backend.Modules.Nutrition.Contracts;
using backend.Modules.Nutrition.Services;
using FluentValidation;
using MediatR;

namespace backend.Modules.Nutrition.Features;

public record UpsertNutritionTargetCommand(Guid UserId, NutritionTargetRequest Request) : IRequest;
public record DeleteNutritionTargetCommand(Guid UserId) : IRequest;
public record CreateFavouriteFoodCommand(Guid UserId, FavouriteFoodRequest Request) : IRequest<Guid>;
public record UpdateFavouriteFoodCommand(Guid UserId, Guid FavouriteFoodId, FavouriteFoodRequest Request) : IRequest;
public record DeleteFavouriteFoodCommand(Guid UserId, Guid FavouriteFoodId) : IRequest;

public class UpsertNutritionTargetHandler : IRequestHandler<UpsertNutritionTargetCommand>
{
    private readonly INutritionTargetService _service;
    private readonly IValidator<NutritionTargetRequest> _validator;

    public UpsertNutritionTargetHandler(INutritionTargetService service, IValidator<NutritionTargetRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    public async Task Handle(UpsertNutritionTargetCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command.Request, cancellationToken);
        await _service.UpsertAsync(command.UserId, command.Request, cancellationToken);
    }
}

public class DeleteNutritionTargetHandler : IRequestHandler<DeleteNutritionTargetCommand>
{
    private readonly INutritionTargetService _service;
    public DeleteNutritionTargetHandler(INutritionTargetService service) => _service = service;
    public Task Handle(DeleteNutritionTargetCommand command, CancellationToken cancellationToken) =>
        _service.DeleteAsync(command.UserId, cancellationToken);
}

public class CreateFavouriteFoodHandler : IRequestHandler<CreateFavouriteFoodCommand, Guid>
{
    private readonly IFavouriteFoodService _service;
    private readonly IValidator<FavouriteFoodRequest> _validator;

    public CreateFavouriteFoodHandler(IFavouriteFoodService service, IValidator<FavouriteFoodRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    public async Task<Guid> Handle(CreateFavouriteFoodCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command.Request, cancellationToken);
        return await _service.CreateAsync(command.UserId, command.Request, cancellationToken);
    }
}

public class UpdateFavouriteFoodHandler : IRequestHandler<UpdateFavouriteFoodCommand>
{
    private readonly IFavouriteFoodService _service;
    private readonly IValidator<FavouriteFoodRequest> _validator;

    public UpdateFavouriteFoodHandler(IFavouriteFoodService service, IValidator<FavouriteFoodRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    public async Task Handle(UpdateFavouriteFoodCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command.Request, cancellationToken);
        await _service.UpdateAsync(command.UserId, command.FavouriteFoodId, command.Request, cancellationToken);
    }
}

public class DeleteFavouriteFoodHandler : IRequestHandler<DeleteFavouriteFoodCommand>
{
    private readonly IFavouriteFoodService _service;
    public DeleteFavouriteFoodHandler(IFavouriteFoodService service) => _service = service;
    public Task Handle(DeleteFavouriteFoodCommand command, CancellationToken cancellationToken) =>
        _service.SoftDeleteAsync(command.UserId, command.FavouriteFoodId, cancellationToken);
}
