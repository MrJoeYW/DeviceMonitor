using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Infrastructure.Database;
using Device_Monitor_App.Models;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.DAO;

/// <summary>
/// 设备数据访问对象实现
/// </summary>
public class DeviceDao : IDeviceDao
{
    private readonly DatabaseContext _dbContext;
    private readonly ILogger<DeviceDao> _logger;

    public DeviceDao(DatabaseContext dbContext, ILogger<DeviceDao> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public IEnumerable<Device> GetAll()
    {
        _logger.LogDebug("查询所有设备");
        return _dbContext.Connection.Table<Device>().ToList();
    }

    public Device? GetById(int id)
    {
        _logger.LogDebug("查询设备 ID={Id}", id);
        return _dbContext.Connection.Find<Device>(id);
    }

    public int Insert(Device device)
    {
        device.CreatedAt = DateTime.Now;
        var result = _dbContext.Connection.Insert(device);
        _logger.LogInformation("新增设备: {Name}, ID={Id}", device.Name, device.Id);
        return device.Id;
    }

    public int Update(Device device)
    {
        var result = _dbContext.Connection.Update(device);
        _logger.LogInformation("更新设备: {Name}, ID={Id}", device.Name, device.Id);
        return result;
    }

    public int Delete(int id)
    {
        var result = _dbContext.Connection.Delete<Device>(id);
        _logger.LogInformation("删除设备 ID={Id}", id);
        return result;
    }

    public void UpdateOnlineStatus(int id, bool isOnline, DateTime heartbeat)
    {
        var device = _dbContext.Connection.Find<Device>(id);
        if (device is null) return;
        device.IsOnline = isOnline;
        device.LastHeartbeat = heartbeat;
        _dbContext.Connection.Update(device);
        _logger.LogDebug("设备 ID={Id} 在线状态更新: {Status}", id, isOnline);
    }
}
