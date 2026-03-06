using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Infrastructure.Database;
using Device_Monitor_App.Models;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.DAO;

/// <summary>
/// 设备标签映射 DAO 实现
/// </summary>
public class DeviceTagMappingDao : IDeviceTagMappingDao
{
    private readonly DatabaseContext _db;
    private readonly ILogger<DeviceTagMappingDao> _logger;

    public DeviceTagMappingDao(DatabaseContext db, ILogger<DeviceTagMappingDao> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IEnumerable<DeviceTagMapping> GetByDeviceId(int deviceId)
    {
        return _db.Connection.Table<DeviceTagMapping>()
            .Where(m => m.DeviceId == deviceId)
            .ToList();
    }

    public DeviceTagMapping? GetById(int id)
    {
        return _db.Connection.Find<DeviceTagMapping>(id);
    }

    public int Insert(DeviceTagMapping mapping)
    {
        _db.Connection.Insert(mapping);
        _logger.LogInformation("新增标签映射: {ValueName} → {PlcTag}, DeviceId={DevId}",
            mapping.ValueName, mapping.PlcTagAddress, mapping.DeviceId);
        return mapping.Id;
    }

    public int Update(DeviceTagMapping mapping)
    {
        var result = _db.Connection.Update(mapping);
        _logger.LogInformation("更新标签映射 ID={Id}", mapping.Id);
        return result;
    }

    public int Delete(int id)
    {
        var result = _db.Connection.Delete<DeviceTagMapping>(id);
        _logger.LogInformation("删除标签映射 ID={Id}", id);
        return result;
    }

    public int DeleteByDeviceId(int deviceId)
    {
        var mappings = GetByDeviceId(deviceId).ToList();
        foreach (var m in mappings)
            _db.Connection.Delete<DeviceTagMapping>(m.Id);
        _logger.LogInformation("删除设备 {DevId} 下所有标签映射, 共{Count}条", deviceId, mappings.Count);
        return mappings.Count;
    }
}
