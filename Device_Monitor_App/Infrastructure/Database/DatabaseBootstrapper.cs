using Device_Monitor_App.Models;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.Infrastructure.Database;

public static class DatabaseBootstrapper
{
    public static void Initialize(DatabaseContext db, ILogger logger)
    {
        db.EnsureTable<Integrator>();
        db.EnsureTable<Device>();
        db.EnsureTable<DeviceReadGroup>();
        db.EnsureTable<DevicePoint>();

        logger.LogInformation("数据库表初始化完成");
    }
}
