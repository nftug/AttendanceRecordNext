using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Helpers;
using Presentation.Models;
using Presentation.Services;
using Presentation.ViewModels;
using Presentation.Views;
using Prism.DryIoc;
using Prism.Ioc;
using System.Reflection;
using System.Windows;

namespace Presentation;

public partial class App : PrismApplication
{
    private Mutex _mutex = new(false, Assembly.GetEntryAssembly()!.GetName().Name);

    protected override async void Initialize()
    {
        if (!_mutex.WaitOne(0, false))
        {
            // Named pipeで既に開いているアプリにウィンドウの再表示を指示
            await NamedPipeClient.SendMessageAsync(new(Environment.ProcessId.ToString(), "Reopen"));

            _mutex.Close();
            _mutex = null!;

            Shutdown();
        }

        base.Initialize();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mutex?.ReleaseMutex();
    }

    protected override Window CreateShell() => Container.Resolve<MainWindow>();

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.Register<IDialogHelper, DialogHelper>();
        containerRegistry.Register<IFileDialogHelper, FileDialogHelper>();
        containerRegistry.Register<IContentDialogHelper<DatePickerDialogViewModel>, DatePickerDialogHelper>();

        containerRegistry.RegisterSingleton<MainWindowModel>();
        containerRegistry.RegisterSingleton<ToastMessagePublisher>();
        containerRegistry.RegisterSingleton<SettingsModel>();
        containerRegistry.RegisterSingleton<WorkTimeAlarmModel>();
        containerRegistry.RegisterSingleton<RestTimeAlarmModel>();
        containerRegistry.RegisterSingleton<WorkTimeModel>();
        containerRegistry.RegisterSingleton<IToastHelper, ToastHelper>();
    }

    // Reference: https://zenn.dev/nin_neko/articles/44045180e35861
    protected override IContainerExtension CreateContainerExtension()
    {
        var services = new ServiceCollection();
        services.AddAttendanceRecordUseCase();

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
