using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Models;
using Device_Monitor_App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.Services;

/// <summary>
/// 设备标签映射业务服务实现
/// </summary>
public class DeviceTagMappingService : IDeviceTagMappingService
{
    private readonly IDeviceTagMappingDao _dao;
    private readonly ILogger<DeviceTagMappingService> _logger;

    public DeviceTagMappingService(IDeviceTagMappingDao dao, ILogger<DeviceTagMappingService> logger)
    {
        _dao = dao;
        _logger = logger;
    }

    public IEnumerable<DeviceTagMapping> GetByDeviceId(int deviceId) => _dao.GetByDeviceId(deviceId);

    public int Add(DeviceTagMapping mapping)
    {
        if (string.IsNullOrWhiteSpace(mapping.ValueName))
            throw new ArgumentException("值名称不能为空");
        if (mapping.PlcOffset < 0)
            throw new ArgumentException("PLC 偏移量不能为负数");
        if (mapping.DeviceId <= 0)
            throw new ArgumentException("必须指定所属设备");

        return _dao.Insert(mapping);
    }

    public bool Update(DeviceTagMapping mapping)
    {
        if (mapping.Id <= 0)
            throw new ArgumentException("无效的映射 ID");
        return _dao.Update(mapping) > 0;
    }

    public bool Delete(int id) => _dao.Delete(id) > 0;
}
