using Domain.Interfaces;
using Domain.Services;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Infrastructure.Repositories;
using Infrastructure.Shared;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Helpers;
using Presentation.Views;
using Prism.DryIoc;
using Prism.Ioc;
using System.Windows;

namespace Presentation;

public partial class App : PrismApplication
{
    protected override Window CreateShell() => Container.Resolve<MainWindow>();

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.Register<IDialogHelper, DialogHelper>();
        containerRegistry.Register<IFileDialogHelper, FileDialogHelper>();
        containerRegistry.Register<IWorkTimeRepository, WorkTimeRepository>();
        containerRegistry.Register<WorkTimeService>();
        // containerRegistry.Register<WorkTimeModel>();
    }

    // Reference: https://zenn.dev/nin_neko/articles/44045180e35861
    protected override IContainerExtension CreateContainerExtension()
    {
        // データ保存用のパスが存在しないとサービス注入時に例外が発生するので、事前に作っておく
        AppConfig.InitAppDataPath();

        var services = new ServiceCollection();
        services.AddMediatR(opt => opt.RegisterServicesFromAssembly(typeof(UseCase.GetWorkToday).Assembly));

        var container = ConvertToDryIocContainer(services);
        return new DryIocContainerExtension(container);
    }

    private Container ConvertToDryIocContainer(
        IEnumerable<ServiceDescriptor> services,
        Func<IRegistrator, ServiceDescriptor, bool>? registerService = null
    )
    {
        var rules = CreateContainerRules();
        var container = new Container(rules);

        container.Use<IServiceScopeFactory>(r => new DryIocServiceScopeFactory(r));
        container.Populate(services, registerService);

        return container;
    }
}
