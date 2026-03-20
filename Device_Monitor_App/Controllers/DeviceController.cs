using Device_Monitor_App.Models;
using Device_Monitor_App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.Controllers;

public class DeviceController
{
    private readonly IDeviceService _deviceService;
    private readonly ILogger<DeviceController> _logger;

    public DeviceController(IDeviceService deviceService, ILogger<DeviceController> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    public IEnumerable<Device> GetAll() => _deviceService.GetAll();

    public IEnumerable<Device> GetByIntegratorId(int integratorId) => _deviceService.GetByIntegratorId(integratorId);

    public Device? GetById(int id) => _deviceService.GetById(id);

    public IEnumerable<DeviceTemplateSummary> GetTemplates() => _deviceService.GetTemplates();

    public int Add(Device device)
    {
        _logger.LogInformation("请求新增设备: {Name}", device.Name);
        return _deviceService.Add(device);
    }

    public bool Update(Device device) => _deviceService.Update(device);

    public bool Delete(int id) => _deviceService.Delete(id);

    public bool RebuildTemplate(int id) => _deviceService.RebuildTemplate(id);
}
