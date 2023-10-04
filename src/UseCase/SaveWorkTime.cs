using Domain.Commands;
using Domain.Entities;
using Domain.Events;
using Domain.Services;
using MediatR;

namespace UseCase;

public class SaveWorkTime
{
    public class Command : IRequest<WorkTime>
    {
        public WorkTimeEditCommandDto Request { get; }

        public Command(WorkTimeEditCommandDto request)
        {
            Request = request;
        }
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
            var result = await _workTimeService.SaveAsync(request.Request, _eventPublisher);
            await _eventPublisher.CommitAsync();
            return result;
        }
    }
}
