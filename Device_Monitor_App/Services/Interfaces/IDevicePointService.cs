using Device_Monitor_App.Models;

namespace Device_Monitor_App.Services.Interfaces;

public interface IDevicePointService
{
    IEnumerable<DevicePoint> GetByDeviceId(int deviceId);
    int Add(DevicePoint point);
    bool Update(DevicePoint point);
    bool Delete(int id);
}
