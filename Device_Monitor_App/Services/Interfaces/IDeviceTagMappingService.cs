using Device_Monitor_App.Models;

namespace Device_Monitor_App.Services.Interfaces;

/// <summary>
/// 设备标签映射业务服务接口
/// </summary>
public interface IDeviceTagMappingService
{
    IEnumerable<DeviceTagMapping> GetByDeviceId(int deviceId);
    int Add(DeviceTagMapping mapping);
    bool Update(DeviceTagMapping mapping);
    bool Delete(int id);
}
