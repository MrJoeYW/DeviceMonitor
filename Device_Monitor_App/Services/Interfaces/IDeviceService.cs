using Device_Monitor_App.Models;

namespace Device_Monitor_App.Services.Interfaces;

public interface IDeviceService
{
    IEnumerable<Device> GetAll();
    IEnumerable<Device> GetByIntegratorId(int integratorId);
    Device? GetById(int id);
    IEnumerable<DeviceTemplateSummary> GetTemplates();
    int Add(Device device);
    bool Update(Device device);
    bool Delete(int id);
    bool RebuildTemplate(int id);
}
