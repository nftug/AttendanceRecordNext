using Domain.Entities;
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

        public Handler(WorkTimeService workTimeService)
        {
            _workTimeService = workTimeService;
        }

        public Task<WorkTime> Handle(Command request, CancellationToken cancellationToken)
            => _workTimeService.ToggleWorkAsync();
    }
}
