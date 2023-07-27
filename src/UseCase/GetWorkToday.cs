﻿using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace UseCase;

public class GetWorkToday
{
    public class Query : IRequest<WorkTime>
    {
    }

    public class Handler : IRequestHandler<Query, WorkTime>
    {
        private readonly IWorkTimeRepository _repository;

        public Handler(IWorkTimeRepository repository)
        {
            _repository = repository;
        }

        public async Task<WorkTime> Handle(Query request, CancellationToken cancellationToken)
            => await _repository.FindByDateAsync(DateTime.Today) ?? WorkTime.CreateEmpty();
    }
}
