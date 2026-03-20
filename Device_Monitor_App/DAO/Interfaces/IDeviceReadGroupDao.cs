using Device_Monitor_App.Models;

namespace Device_Monitor_App.DAO.Interfaces;

public interface IDeviceReadGroupDao
{
    IEnumerable<DeviceReadGroup> GetByDeviceId(int deviceId);
    DeviceReadGroup? GetById(int id);
    int Insert(DeviceReadGroup group);
    int Update(DeviceReadGroup group);
    int Delete(int id);
    int DeleteByDeviceId(int deviceId);
}
