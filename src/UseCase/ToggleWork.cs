using Domain.Entities;
using Domain.Events;
using Domain.Services;
using MediatR;

namespace UseCase;

public class ToggleWork
{
    public class Command : IRequest<WorkTime>
    {
    }

    public class Handler : IRequestHandler<Command, WorkTime>
    {
        private readonly WorkTimeService _workTimeService;
        private readonly EventPublisher _eventPublisher = new();

        public Handler(WorkTimeService workTimeService)
        {
            _workTimeService = workTimeService;
        }

        public async Task<WorkTime> Handle(Command request, CancellationToken cancellationToken)
        {
            var result = await _workTimeService.ToggleWorkAsync(_eventPublisher);
            await _eventPublisher.CommitAsync();
            return result.Recreate();
        }
    }
}
