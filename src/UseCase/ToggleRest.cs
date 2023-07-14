using Domain.Entities;
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

        public Handler(IWorkTimeRepository repository)
        {
            _repository = repository;
        }

        public async Task<WorkTime> Handle(Command request, CancellationToken cancellationToken)
        {
            var latest =
                await _repository.FindByDateAsync(DateTime.UtcNow)
                ?? throw new DomainException("There is no available work item.");

            var toggled = latest.ToggleRest();
            await _repository.UpdateAsync(toggled);

            return toggled;
        }
    }
}
