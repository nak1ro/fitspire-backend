using backend.Modules.BodyTracking.Contracts;
using backend.Modules.BodyTracking.Services;
using FluentValidation;
using MediatR;

namespace backend.Modules.BodyTracking.Features;

public record CreateBodyCheckInCommand(Guid UserId, CreateBodyCheckInRequest Request) : IRequest<Guid>;
public record UpdateBodyCheckInCommand(Guid UserId, Guid CheckInId, UpdateBodyCheckInRequest Request) : IRequest;
public record DeleteBodyCheckInCommand(Guid UserId, Guid CheckInId) : IRequest;

public class CreateBodyCheckInHandler : IRequestHandler<CreateBodyCheckInCommand, Guid>
{
    private readonly IBodyCheckInWriteService _service;
    private readonly IValidator<CreateBodyCheckInRequest> _validator;

    public CreateBodyCheckInHandler(IBodyCheckInWriteService service, IValidator<CreateBodyCheckInRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    public async Task<Guid> Handle(CreateBodyCheckInCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command.Request, cancellationToken);
        return await _service.CreateAsync(command.UserId, command.Request, cancellationToken);
    }
}

public class UpdateBodyCheckInHandler : IRequestHandler<UpdateBodyCheckInCommand>
{
    private readonly IBodyCheckInWriteService _service;
    private readonly IValidator<UpdateBodyCheckInRequest> _validator;

    public UpdateBodyCheckInHandler(IBodyCheckInWriteService service, IValidator<UpdateBodyCheckInRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    public async Task Handle(UpdateBodyCheckInCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command.Request, cancellationToken);
        await _service.UpdateAsync(command.UserId, command.CheckInId, command.Request, cancellationToken);
    }
}

public class DeleteBodyCheckInHandler : IRequestHandler<DeleteBodyCheckInCommand>
{
    private readonly IBodyCheckInWriteService _service;

    public DeleteBodyCheckInHandler(IBodyCheckInWriteService service)
    {
        _service = service;
    }

    public Task Handle(DeleteBodyCheckInCommand command, CancellationToken cancellationToken) =>
        _service.SoftDeleteAsync(command.UserId, command.CheckInId, cancellationToken);
}
