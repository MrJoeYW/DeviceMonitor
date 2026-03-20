using Device_Monitor_App.Models;
using Device_Monitor_App.Services.Interfaces;

namespace Device_Monitor_App.Controllers;

public class DeviceReadGroupController
{
    private readonly IDeviceReadGroupService _service;

    public DeviceReadGroupController(IDeviceReadGroupService service)
    {
        _service = service;
    }

    public IEnumerable<DeviceReadGroup> GetByDeviceId(int deviceId) => _service.GetByDeviceId(deviceId);

    public int Add(DeviceReadGroup group) => _service.Add(group);

    public bool Update(DeviceReadGroup group) => _service.Update(group);

    public bool Delete(int id) => _service.Delete(id);
}
