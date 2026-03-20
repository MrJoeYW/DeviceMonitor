using Device_Monitor_App.Controllers;
using Device_Monitor_App.DAO;
using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Forms;
using Device_Monitor_App.Infrastructure.Database;
using Device_Monitor_App.Services;
using Device_Monitor_App.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace Device_Monitor_App;

static class Program
{
    [STAThread]
    static void Main()
    {
        var startupLogger = LogManager.Setup()
            .LoadConfigurationFromFile("nlog.config")
            .GetCurrentClassLogger();

        startupLogger.Info("============ 设备监控系统启动 ============");

        try
        {
            ApplicationConfiguration.Initialize();

            var services = new ServiceCollection();
            ConfigureServices(services);

            using var serviceProvider = services.BuildServiceProvider();

            var dbContext = serviceProvider.GetRequiredService<DatabaseContext>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var appLogger = loggerFactory.CreateLogger("Program");

            DatabaseBootstrapper.Initialize(dbContext, appLogger);
            startupLogger.Info("数据库初始化完成（Integrator / Device / DeviceReadGroup / DevicePoint）");

            var mainForm = serviceProvider.GetRequiredService<FormMain>();
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            startupLogger.Error(ex, "应用程序发生未处理异常，即将退出");
            MessageBox.Show($"程序启动失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            startupLogger.Info("============ 设备监控系统退出 ============");
            LogManager.Shutdown();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var dbFolder = Path.Combine(AppContext.BaseDirectory, "data");
        Directory.CreateDirectory(dbFolder);
        var dbPath = Path.Combine(dbFolder, "device_monitor.db");
        services.AddSingleton(new DatabaseContext(dbPath));

        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
            logging.AddNLog();
        });

        services.AddSingleton<IIntegratorDao, IntegratorDao>();
        services.AddSingleton<IDeviceDao, DeviceDao>();
        services.AddSingleton<IDeviceReadGroupDao, DeviceReadGroupDao>();
        services.AddSingleton<IDevicePointDao, DevicePointDao>();

        services.AddSingleton<IIntegratorService, IntegratorService>();
        services.AddSingleton<IDeviceService, DeviceService>();
        services.AddSingleton<IDeviceReadGroupService, DeviceReadGroupService>();
        services.AddSingleton<IDevicePointService, DevicePointService>();
        services.AddSingleton<ISnapshotService, SnapshotService>();

        services.AddSingleton<IntegratorController>();
        services.AddSingleton<DeviceController>();
        services.AddSingleton<DeviceReadGroupController>();
        services.AddSingleton<DevicePointController>();

        services.AddTransient<FormMain>();
    }
}
