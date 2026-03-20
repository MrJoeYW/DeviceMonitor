using Device_Monitor_App.Models;

namespace Device_Monitor_App.DAO.Interfaces;

public interface IDeviceDao
{
    IEnumerable<Device> GetAll();
    IEnumerable<Device> GetByIntegratorId(int integratorId);
    Device? GetById(int id);
    int Insert(Device device);
    int Update(Device device);
    int Delete(int id);
    int DeleteByIntegratorId(int integratorId);
}
