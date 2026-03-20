using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Infrastructure.Database;
using Device_Monitor_App.Models;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.DAO;

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
        return _db.Connection.Table<Device>().OrderBy(device => device.Id).ToList();
    }

    public IEnumerable<Device> GetByIntegratorId(int integratorId)
    {
        return _db.Connection.Table<Device>()
            .Where(device => device.IntegratorId == integratorId)
            .OrderBy(device => device.Id)
            .ToList();
    }

    public Device? GetById(int id)
    {
        return _db.Connection.Find<Device>(id);
    }

    public int Insert(Device device)
    {
        _db.Connection.Insert(device);
        _logger.LogInformation("新增设备: {Name}, ID={Id}, IntegratorId={IntegratorId}", device.Name, device.Id, device.IntegratorId);
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
        foreach (var device in devices)
        {
            _db.Connection.Delete<Device>(device.Id);
        }

        _logger.LogInformation("删除网关下全部设备, IntegratorId={IntegratorId}, Count={Count}", integratorId, devices.Count);
        return devices.Count;
    }
}
