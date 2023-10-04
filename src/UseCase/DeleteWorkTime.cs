using Domain.Events;
using Domain.Services;
using MediatR;

namespace UseCase;

public class DeleteWorkTime
{
    public class Command : IRequest<Unit>
    {
        public Guid ItemId { get; }

        public Command(Guid itemId)
        {
            ItemId = itemId;
        }
    }

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly WorkTimeService _workTimeService;
        private readonly EventPublisher _eventPublisher = new();

        public Handler(WorkTimeService workTimeService)
        {
            _workTimeService = workTimeService;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            await _workTimeService.DeleteAsync(request.ItemId, _eventPublisher);
            await _eventPublisher.CommitAsync();
            return Unit.Value;
        }
    }
}
