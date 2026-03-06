using Device_Monitor_App.Models;

namespace Device_Monitor_App.DAO.Interfaces;

/// <summary>
/// 设备标签映射 DAO 接口
/// </summary>
public interface IDeviceTagMappingDao
{
    IEnumerable<DeviceTagMapping> GetByDeviceId(int deviceId);
    DeviceTagMapping? GetById(int id);
    int Insert(DeviceTagMapping mapping);
    int Update(DeviceTagMapping mapping);
    int Delete(int id);
    int DeleteByDeviceId(int deviceId);
}
