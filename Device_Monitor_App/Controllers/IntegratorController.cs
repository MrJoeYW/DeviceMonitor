using Device_Monitor_App.Models;
using Device_Monitor_App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.Controllers;

public class IntegratorController
{
    private readonly IIntegratorService _service;
    private readonly ILogger<IntegratorController> _logger;

    public IntegratorController(IIntegratorService service, ILogger<IntegratorController> logger)
    {
        _service = service;
        _logger = logger;
    }

    public IEnumerable<Integrator> GetAll() => _service.GetAll();

    public Integrator? GetById(int id) => _service.GetById(id);

    public int Add(Integrator integrator)
    {
        _logger.LogInformation("请求新增网关: {Name} ({IpAddress}:{Port})", integrator.Name, integrator.IpAddress, integrator.Port);
        return _service.Add(integrator);
    }

    public bool Update(Integrator integrator) => _service.Update(integrator);

    public bool Delete(int id) => _service.Delete(id);
}
