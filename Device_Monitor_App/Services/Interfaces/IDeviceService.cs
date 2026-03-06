using Device_Monitor_App.Models;

namespace Device_Monitor_App.Services.Interfaces;

/// <summary>
/// 设备业务服务接口
/// </summary>
public interface IDeviceService
{
    IEnumerable<Device> GetAll();
    IEnumerable<Device> GetByIntegratorId(int integratorId);
    Device? GetById(int id);
    int Add(Device device);
    bool Update(Device device);
    bool Delete(int id);
}
