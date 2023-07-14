using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace UseCase;

public class ToggleWork
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
            var workToday = await _repository.FindByDateAsync(DateTime.Now);

            if (workToday != null)
            {
                if (workToday.IsTodayOngoing)
                    workToday.Finish();        // 退勤
                else
                    workToday.Restart();       // 退勤後の勤務再開

                await _repository.UpdateAsync(workToday);
            }
            else
            {
                workToday = WorkTime.Start();  // 勤務開始
                await _repository.CreateAsync(workToday);
            }

            return workToday.Recreate();
        }
    }
}
