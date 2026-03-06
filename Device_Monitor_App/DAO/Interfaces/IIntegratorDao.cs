using Device_Monitor_App.Models;

namespace Device_Monitor_App.DAO.Interfaces;

/// <summary>
/// 集成设备 DAO 接口
/// </summary>
public interface IIntegratorDao
{
    IEnumerable<Integrator> GetAll();
    Integrator? GetById(int id);
    int Insert(Integrator integrator);
    int Update(Integrator integrator);
    int Delete(int id);
}
