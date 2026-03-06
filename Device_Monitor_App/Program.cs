using Device_Monitor_App.Controllers;
using Device_Monitor_App.DAO;
using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Forms;
using Device_Monitor_App.Infrastructure.Database;
using Device_Monitor_App.Models;
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
        // 初始化 NLog（读取 nlog.config）
        var logger = LogManager.Setup()
            .LoadConfigurationFromFile("nlog.config")
            .GetCurrentClassLogger();

        logger.Info("============ 设备监控系统启动 ============");

        try
        {
            ApplicationConfiguration.Initialize();

            // 构建 DI 容器
            var services = new ServiceCollection();
            ConfigureServices(services);

            using var serviceProvider = services.BuildServiceProvider();

            // 初始化数据库表
            var dbContext = serviceProvider.GetRequiredService<DatabaseContext>();
            dbContext.EnsureTable<Device>();
            logger.Info("数据库初始化完成");

            // 启动主窗体
            var mainForm = serviceProvider.GetRequiredService<FormMain>();
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "应用程序发生未处理异常，即将退出");
            MessageBox.Show($"程序启动失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            logger.Info("============ 设备监控系统退出 ============");
            LogManager.Shutdown();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // 数据库路径（放在程序目录下的 data 文件夹）
        var dbFolder = Path.Combine(AppContext.BaseDirectory, "data");
        Directory.CreateDirectory(dbFolder);
        var dbPath = Path.Combine(dbFolder, "device_monitor.db");

        // 基础设施
        services.AddSingleton(new DatabaseContext(dbPath));

        // 日志（NLog 接管 ILogger<T>）
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
            logging.AddNLog();
        });

        // DAO 层
        services.AddScoped<IDeviceDao, DeviceDao>();

        // Service 层
        services.AddScoped<IDeviceService, DeviceService>();

        // Controller 层
        services.AddTransient<DeviceController>();

        // 窗体
        services.AddTransient<FormMain>();
    }
}