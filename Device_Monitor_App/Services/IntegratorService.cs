using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Models;
using Device_Monitor_App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.Services;

public class IntegratorService : IIntegratorService
{
    private readonly IIntegratorDao _dao;
    private readonly IDeviceDao _deviceDao;
    private readonly IDeviceReadGroupDao _readGroupDao;
    private readonly IDevicePointDao _pointDao;
    private readonly ILogger<IntegratorService> _logger;

    public IntegratorService(
        IIntegratorDao dao,
        IDeviceDao deviceDao,
        IDeviceReadGroupDao readGroupDao,
        IDevicePointDao pointDao,
        ILogger<IntegratorService> logger)
    {
        _dao = dao;
        _deviceDao = deviceDao;
        _readGroupDao = readGroupDao;
        _pointDao = pointDao;
        _logger = logger;
    }

    public IEnumerable<Integrator> GetAll() => _dao.GetAll();

    public Integrator? GetById(int id) => _dao.GetById(id);

    public int Add(Integrator integrator)
    {
        if (string.IsNullOrWhiteSpace(integrator.Name))
        {
            throw new ArgumentException("网关名称不能为空");
        }

        if (string.IsNullOrWhiteSpace(integrator.IpAddress))
        {
            throw new ArgumentException("IP 地址不能为空");
        }

        if (integrator.Port <= 0)
        {
            throw new ArgumentException("端口必须大于 0");
        }

        return _dao.Insert(integrator);
    }

    public bool Update(Integrator integrator)
    {
        if (integrator.Id <= 0)
        {
            throw new ArgumentException("无效的网关 ID");
        }

        return _dao.Update(integrator) > 0;
    }

    public bool Delete(int id)
    {
        _logger.LogInformation("级联删除网关 ID={Id}", id);
        var devices = _deviceDao.GetByIntegratorId(id).ToList();
        foreach (var device in devices)
        {
            _pointDao.DeleteByDeviceId(device.Id);
            _readGroupDao.DeleteByDeviceId(device.Id);
        }

        _deviceDao.DeleteByIntegratorId(id);
        return _dao.Delete(id) > 0;
    }
}
