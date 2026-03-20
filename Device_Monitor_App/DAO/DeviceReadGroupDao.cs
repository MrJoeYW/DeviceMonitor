using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Infrastructure.Database;
using Device_Monitor_App.Models;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.DAO;

public class DeviceReadGroupDao : IDeviceReadGroupDao
{
    private readonly DatabaseContext _db;
    private readonly ILogger<DeviceReadGroupDao> _logger;

    public DeviceReadGroupDao(DatabaseContext db, ILogger<DeviceReadGroupDao> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IEnumerable<DeviceReadGroup> GetByDeviceId(int deviceId)
    {
        return _db.Connection.Table<DeviceReadGroup>()
            .Where(group => group.DeviceId == deviceId)
            .OrderBy(group => group.SortOrder)
            .ThenBy(group => group.Id)
            .ToList();
    }

    public DeviceReadGroup? GetById(int id)
    {
        return _db.Connection.Find<DeviceReadGroup>(id);
    }

    public int Insert(DeviceReadGroup group)
    {
        _db.Connection.Insert(group);
        _logger.LogInformation("新增采集块: {Name}, ID={Id}, DeviceId={DeviceId}", group.Name, group.Id, group.DeviceId);
        return group.Id;
    }

    public int Update(DeviceReadGroup group)
    {
        var result = _db.Connection.Update(group);
        _logger.LogInformation("更新采集块: {Name}, ID={Id}", group.Name, group.Id);
        return result;
    }

    public int Delete(int id)
    {
        var result = _db.Connection.Delete<DeviceReadGroup>(id);
        _logger.LogInformation("删除采集块 ID={Id}", id);
        return result;
    }

    public int DeleteByDeviceId(int deviceId)
    {
        var groups = GetByDeviceId(deviceId).ToList();
        foreach (var group in groups)
        {
            _db.Connection.Delete<DeviceReadGroup>(group.Id);
        }

        _logger.LogInformation("删除设备下全部采集块, DeviceId={DeviceId}, Count={Count}", deviceId, groups.Count);
        return groups.Count;
    }
}
