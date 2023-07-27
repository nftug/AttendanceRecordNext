using Domain.Entities;
using Domain.Events;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace UseCase;

public class ToggleRest
{
    public class Command : IRequest<WorkTime>
    {
    }

    public class Handler : IRequestHandler<Command, WorkTime>
    {
        private readonly IWorkTimeRepository _repository;
        private readonly DomainEventPublisher _eventPublisher = new();

        public Handler(
            IWorkTimeRepository repository,
            EntityEventSubscriber<RestTime> restTimeSubscriber,
            EntityEventSubscriber<WorkTime> workTimeSubscriber
        )
        {
            _repository = repository;
            _eventPublisher
                .Subscribe(restTimeSubscriber)
                .Subscribe(workTimeSubscriber);
        }

        public async Task<WorkTime> Handle(Command request, CancellationToken cancellationToken)
        {
            var latest =
                await _repository.FindByDateAsync(DateTime.Today)
                ?? throw new DomainException("There is no available work item.");

            latest = latest.ToggleRest(_eventPublisher);
            await _eventPublisher.DispatchAsync();

            return latest;
        }
    }
}
