using backend.Modules.Nutrition.Contracts;
using backend.Modules.Nutrition.Services;
using FluentValidation;
using MediatR;

namespace backend.Modules.Nutrition.Features;

public record CreateMealCommand(Guid UserId, CreateMealRequest Request) : IRequest<Guid>;
public record UpdateMealCommand(Guid UserId, Guid MealId, UpdateMealRequest Request) : IRequest;
public record DeleteMealCommand(Guid UserId, Guid MealId) : IRequest;
public record AddMealItemCommand(Guid UserId, Guid MealId, AddMealItemRequest Request) : IRequest<Guid>;
public record UpdateMealItemCommand(Guid UserId, Guid MealId, Guid ItemId, MealItemRequest Request) : IRequest;
public record DeleteMealItemCommand(Guid UserId, Guid MealId, Guid ItemId) : IRequest;
public record ReorderMealItemsCommand(Guid UserId, Guid MealId, ReorderMealItemsRequest Request) : IRequest;

public class CreateMealHandler : IRequestHandler<CreateMealCommand, Guid>
{
    private readonly IMealWriteService _service;
    private readonly IValidator<CreateMealRequest> _validator;

    public CreateMealHandler(IMealWriteService service, IValidator<CreateMealRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    public async Task<Guid> Handle(CreateMealCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command.Request, cancellationToken);
        return await _service.CreateAsync(command.UserId, command.Request, cancellationToken);
    }
}

public class UpdateMealHandler : IRequestHandler<UpdateMealCommand>
{
    private readonly IMealWriteService _service;
    private readonly IValidator<UpdateMealRequest> _validator;

    public UpdateMealHandler(IMealWriteService service, IValidator<UpdateMealRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    public async Task Handle(UpdateMealCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command.Request, cancellationToken);
        await _service.UpdateAsync(command.UserId, command.MealId, command.Request, cancellationToken);
    }
}

public class DeleteMealHandler : IRequestHandler<DeleteMealCommand>
{
    private readonly IMealWriteService _service;
    public DeleteMealHandler(IMealWriteService service) => _service = service;
    public Task Handle(DeleteMealCommand command, CancellationToken cancellationToken) =>
        _service.SoftDeleteAsync(command.UserId, command.MealId, cancellationToken);
}

public class AddMealItemHandler : IRequestHandler<AddMealItemCommand, Guid>
{
    private readonly IMealWriteService _service;
    private readonly IValidator<AddMealItemRequest> _validator;

    public AddMealItemHandler(IMealWriteService service, IValidator<AddMealItemRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    public async Task<Guid> Handle(AddMealItemCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command.Request, cancellationToken);
        return await _service.AddItemAsync(command.UserId, command.MealId, command.Request, cancellationToken);
    }
}

public class UpdateMealItemHandler : IRequestHandler<UpdateMealItemCommand>
{
    private readonly IMealWriteService _service;
    private readonly IValidator<MealItemRequest> _validator;

    public UpdateMealItemHandler(IMealWriteService service, IValidator<MealItemRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    public async Task Handle(UpdateMealItemCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command.Request, cancellationToken);
        await _service.UpdateItemAsync(command.UserId, command.MealId, command.ItemId, command.Request, cancellationToken);
    }
}

public class DeleteMealItemHandler : IRequestHandler<DeleteMealItemCommand>
{
    private readonly IMealWriteService _service;
    public DeleteMealItemHandler(IMealWriteService service) => _service = service;
    public Task Handle(DeleteMealItemCommand command, CancellationToken cancellationToken) =>
        _service.RemoveItemAsync(command.UserId, command.MealId, command.ItemId, cancellationToken);
}

public class ReorderMealItemsHandler : IRequestHandler<ReorderMealItemsCommand>
{
    private readonly IMealWriteService _service;
    private readonly IValidator<ReorderMealItemsRequest> _validator;

    public ReorderMealItemsHandler(IMealWriteService service, IValidator<ReorderMealItemsRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    public async Task Handle(ReorderMealItemsCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command.Request, cancellationToken);
        await _service.ReorderItemsAsync(command.UserId, command.MealId, command.Request, cancellationToken);
    }
}
