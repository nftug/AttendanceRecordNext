using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace UseCase;

public class GetMonthlyAll
{
    public class Query : IRequest<List<WorkTime>>
    {
        public required DateTime Date { get; init; }
    }

    public class Handler : IRequestHandler<Query, List<WorkTime>>
    {
        private readonly IWorkTimeRepository _repository;

        public Handler(IWorkTimeRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<WorkTime>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await _repository.FindAllByMonthAsync(request.Date);
        }
    }
}
