using Domain.Entities;
using Domain.Services;
using MediatR;

namespace UseCase;

public class GetWorkToday
{
    public class Query : IRequest<WorkTime>
    {
    }

    public class Handler : IRequestHandler<Query, WorkTime>
    {
        private readonly WorkTimeFactory _workTimeFactory;

        public Handler(WorkTimeFactory workTimeFactory)
        {
            _workTimeFactory = workTimeFactory;
        }

        public async Task<WorkTime> Handle(Query request, CancellationToken cancellationToken)
            => await _workTimeFactory.FindByDateAsync(DateTime.Today) ?? _workTimeFactory.Create();
    }
}
