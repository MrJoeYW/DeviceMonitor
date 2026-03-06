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
    private readonly DatabaseContext _db;
    private readonly ILogger<DeviceDao> _logger;

    public DeviceDao(DatabaseContext db, ILogger<DeviceDao> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IEnumerable<Device> GetAll()
    {
        return _db.Connection.Table<Device>().ToList();
    }

    public IEnumerable<Device> GetByIntegratorId(int integratorId)
    {
        return _db.Connection.Table<Device>()
            .Where(d => d.IntegratorId == integratorId)
            .ToList();
    }

    public Device? GetById(int id)
    {
        return _db.Connection.Find<Device>(id);
    }

    public int Insert(Device device)
    {
        device.CreatedAt = DateTime.Now;
        _db.Connection.Insert(device);
        _logger.LogInformation("新增设备: {Name}, ID={Id}, IntegratorId={IntId}", device.Name, device.Id, device.IntegratorId);
        return device.Id;
    }

    public int Update(Device device)
    {
        var result = _db.Connection.Update(device);
        _logger.LogInformation("更新设备: {Name}, ID={Id}", device.Name, device.Id);
        return result;
    }

    public int Delete(int id)
    {
        var result = _db.Connection.Delete<Device>(id);
        _logger.LogInformation("删除设备 ID={Id}", id);
        return result;
    }

    public int DeleteByIntegratorId(int integratorId)
    {
        var devices = GetByIntegratorId(integratorId).ToList();
        foreach (var d in devices)
            _db.Connection.Delete<Device>(d.Id);
        _logger.LogInformation("删除集成设备 {IntId} 下所有子设备, 共{Count}台", integratorId, devices.Count);
        return devices.Count;
    }
}
