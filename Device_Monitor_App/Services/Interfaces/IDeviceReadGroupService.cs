using Device_Monitor_App.Models;

namespace Device_Monitor_App.Services.Interfaces;

public interface IDeviceReadGroupService
{
    IEnumerable<DeviceReadGroup> GetByDeviceId(int deviceId);
    int Add(DeviceReadGroup group);
    bool Update(DeviceReadGroup group);
    bool Delete(int id);
}
