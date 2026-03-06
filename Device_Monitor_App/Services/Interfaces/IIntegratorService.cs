using Device_Monitor_App.Models;

namespace Device_Monitor_App.Services.Interfaces;

/// <summary>
/// 集成设备业务服务接口
/// </summary>
public interface IIntegratorService
{
    IEnumerable<Integrator> GetAll();
    Integrator? GetById(int id);
    int Add(Integrator integrator);
    bool Update(Integrator integrator);
    bool Delete(int id);
}
