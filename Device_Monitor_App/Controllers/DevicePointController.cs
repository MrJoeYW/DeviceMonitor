using Device_Monitor_App.Models;
using Device_Monitor_App.Services.Interfaces;

namespace Device_Monitor_App.Controllers;

public class DevicePointController
{
    private readonly IDevicePointService _service;

    public DevicePointController(IDevicePointService service)
    {
        _service = service;
    }

    public IEnumerable<DevicePoint> GetByDeviceId(int deviceId) => _service.GetByDeviceId(deviceId);

    public int Add(DevicePoint point) => _service.Add(point);

    public bool Update(DevicePoint point) => _service.Update(point);

    public bool Delete(int id) => _service.Delete(id);
}
