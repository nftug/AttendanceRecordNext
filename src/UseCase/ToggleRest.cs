using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Services;
using MediatR;

namespace UseCase;

public class ToggleRest
{
    public class Command : IRequest<WorkTime>
    {
    }

    public class Handler : IRequestHandler<Command, WorkTime>
    {
        private readonly WorkTimeService _workTimeService;
        private readonly IWorkTimeRepository _repository;

        public Handler(WorkTimeService workTimeService, IWorkTimeRepository repository)
        {
            _workTimeService = workTimeService;
            _repository = repository;
        }

        public async Task<WorkTime> Handle(Command request, CancellationToken cancellationToken)
        {
            var latest =
                await _repository.FindByDateAsync(DateTime.Today)
                ?? throw new DomainException("There is no available work item.");

            return await _workTimeService.ToggleRestAsync(latest);
        }
    }
}
