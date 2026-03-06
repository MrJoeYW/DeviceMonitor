using Device_Monitor_App.Models;
using Device_Monitor_App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.Controllers;

/// <summary>
/// 集成设备控制器
/// </summary>
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

    public int Add(string name, string ipAddress, int port, string remark = "")
    {
        _logger.LogInformation("请求新增集成设备: {Name} ({Ip}:{Port})", name, ipAddress, port);
        var integrator = new Integrator
        {
            Name = name,
            IpAddress = ipAddress,
            Port = port,
            Remark = remark
        };
        return _service.Add(integrator);
    }

    public bool Update(Integrator integrator) => _service.Update(integrator);

    public bool Delete(int id) => _service.Delete(id);
}
