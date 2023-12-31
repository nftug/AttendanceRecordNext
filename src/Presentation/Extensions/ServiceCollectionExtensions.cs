using Domain.Entities;
using Domain.Events;
using Domain.Interfaces;
using Domain.Services;
using Infrastructure.Repositories;
using Infrastructure.Shared;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// ユースケース層のサービスを注入する
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAttendanceRecordUseCase(this IServiceCollection services)
    {
        // データ保存用のパスが存在しないとサービス注入時に例外が発生するので、事前に作っておく
        return services
            .AddMediatR(opt => opt.RegisterServicesFromAssembly(typeof(UseCase.GetWorkToday).Assembly))
            .AddSingleton<IAppInfo, AppInfo>()
            .AddSingleton<IAppConfigRepository, AppConfigRepository>()
            .AddTransient<IWorkTimeRepository, WorkTimeRepository>()
            .AddTransient<IRepository<WorkTime>, WorkTimeRepository>()
            .AddSingleton<WorkTimeService>()
            .AddSingleton<WorkTimeFactory>()
            .AddSingleton<EntityEventSubscriber<WorkTime>>();
    }
}
