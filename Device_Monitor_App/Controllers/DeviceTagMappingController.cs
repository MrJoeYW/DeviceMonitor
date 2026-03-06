using Device_Monitor_App.Models;
using Device_Monitor_App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.Controllers;

/// <summary>
/// 设备标签映射控制器
/// </summary>
public class DeviceTagMappingController
{
    private readonly IDeviceTagMappingService _service;
    private readonly ILogger<DeviceTagMappingController> _logger;

    public DeviceTagMappingController(IDeviceTagMappingService service, ILogger<DeviceTagMappingController> logger)
    {
        _service = service;
        _logger = logger;
    }

    public IEnumerable<DeviceTagMapping> GetByDeviceId(int deviceId) => _service.GetByDeviceId(deviceId);

    public int Add(DeviceTagMapping mapping)
    {
        _logger.LogInformation("请求新增标签映射: {ValueName} → {PlcTag}", mapping.ValueName, mapping.PlcTagAddress);
        return _service.Add(mapping);
    }

    public bool Update(DeviceTagMapping mapping) => _service.Update(mapping);

    public bool Delete(int id) => _service.Delete(id);
}
