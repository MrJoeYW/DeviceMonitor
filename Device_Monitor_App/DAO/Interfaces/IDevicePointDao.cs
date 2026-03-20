using Device_Monitor_App.Models;

namespace Device_Monitor_App.DAO.Interfaces;

public interface IDevicePointDao
{
    IEnumerable<DevicePoint> GetByDeviceId(int deviceId);
    IEnumerable<DevicePoint> GetByReadGroupId(int readGroupId);
    DevicePoint? GetById(int id);
    int Insert(DevicePoint point);
    int Update(DevicePoint point);
    int Delete(int id);
    int DeleteByDeviceId(int deviceId);
    int DeleteByReadGroupId(int readGroupId);
}
