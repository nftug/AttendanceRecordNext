using Domain.Responses;
using Domain.Services;
using MediatR;

namespace UseCase;

public class GetMonthlyAll
{
    public class Query : IRequest<WorkTimeMonthlyTally>
    {
        public DateTime Date { get; }

        public Query(DateTime date) => Date = date;
    }

    public class Handler : IRequestHandler<Query, WorkTimeMonthlyTally>
    {
        private readonly WorkTimeFactory _workTimeFactory;

        public Handler(WorkTimeFactory workTimeFactory)
        {
            _workTimeFactory = workTimeFactory;
        }

        public async Task<WorkTimeMonthlyTally> Handle(Query request, CancellationToken cancellationToken)
            => await _workTimeFactory.FindAllByMonthAsync(request.Date);
    }
}
