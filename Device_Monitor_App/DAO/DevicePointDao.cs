using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Infrastructure.Database;
using Device_Monitor_App.Models;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.DAO;

public class DevicePointDao : IDevicePointDao
{
    private readonly DatabaseContext _db;
    private readonly ILogger<DevicePointDao> _logger;

    public DevicePointDao(DatabaseContext db, ILogger<DevicePointDao> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IEnumerable<DevicePoint> GetByDeviceId(int deviceId)
    {
        return _db.Connection.Table<DevicePoint>()
            .Where(point => point.DeviceId == deviceId)
            .OrderBy(point => point.SortOrder)
            .ThenBy(point => point.Id)
            .ToList();
    }

    public IEnumerable<DevicePoint> GetByReadGroupId(int readGroupId)
    {
        return _db.Connection.Table<DevicePoint>()
            .Where(point => point.ReadGroupId == readGroupId)
            .OrderBy(point => point.SortOrder)
            .ThenBy(point => point.Id)
            .ToList();
    }

    public DevicePoint? GetById(int id)
    {
        return _db.Connection.Find<DevicePoint>(id);
    }

    public int Insert(DevicePoint point)
    {
        _db.Connection.Insert(point);
        _logger.LogInformation("新增测点: {DisplayName}, ID={Id}, DeviceId={DeviceId}", point.DisplayName, point.Id, point.DeviceId);
        return point.Id;
    }

    public int Update(DevicePoint point)
    {
        var result = _db.Connection.Update(point);
        _logger.LogInformation("更新测点: {DisplayName}, ID={Id}", point.DisplayName, point.Id);
        return result;
    }

    public int Delete(int id)
    {
        var result = _db.Connection.Delete<DevicePoint>(id);
        _logger.LogInformation("删除测点 ID={Id}", id);
        return result;
    }

    public int DeleteByDeviceId(int deviceId)
    {
        var points = GetByDeviceId(deviceId).ToList();
        foreach (var point in points)
        {
            _db.Connection.Delete<DevicePoint>(point.Id);
        }

        _logger.LogInformation("删除设备下全部测点, DeviceId={DeviceId}, Count={Count}", deviceId, points.Count);
        return points.Count;
    }

    public int DeleteByReadGroupId(int readGroupId)
    {
        var points = GetByReadGroupId(readGroupId).ToList();
        foreach (var point in points)
        {
            _db.Connection.Delete<DevicePoint>(point.Id);
        }

        _logger.LogInformation("删除采集块下全部测点, ReadGroupId={ReadGroupId}, Count={Count}", readGroupId, points.Count);
        return points.Count;
    }
}
