using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Models;
using Device_Monitor_App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.Services;

/// <summary>
/// 集成设备业务服务实现
/// </summary>
public class IntegratorService : IIntegratorService
{
    private readonly IIntegratorDao _dao;
    private readonly IDeviceDao _deviceDao;
    private readonly IDeviceTagMappingDao _tagDao;
    private readonly ILogger<IntegratorService> _logger;

    public IntegratorService(IIntegratorDao dao, IDeviceDao deviceDao,
        IDeviceTagMappingDao tagDao, ILogger<IntegratorService> logger)
    {
        _dao = dao;
        _deviceDao = deviceDao;
        _tagDao = tagDao;
        _logger = logger;
    }

    public IEnumerable<Integrator> GetAll() => _dao.GetAll();

    public Integrator? GetById(int id) => _dao.GetById(id);

    public int Add(Integrator integrator)
    {
        if (string.IsNullOrWhiteSpace(integrator.Name))
            throw new ArgumentException("集成设备名称不能为空");
        if (string.IsNullOrWhiteSpace(integrator.IpAddress))
            throw new ArgumentException("IP 地址不能为空");

        return _dao.Insert(integrator);
    }

    public bool Update(Integrator integrator)
    {
        if (integrator.Id <= 0)
            throw new ArgumentException("无效的集成设备 ID");
        return _dao.Update(integrator) > 0;
    }

    /// <summary>级联删除：集成设备 → 子设备 → 标签映射</summary>
    public bool Delete(int id)
    {
        _logger.LogInformation("级联删除集成设备 ID={Id}", id);
        // 先删所有子设备的标签映射
        var devices = _deviceDao.GetByIntegratorId(id);
        foreach (var d in devices)
            _tagDao.DeleteByDeviceId(d.Id);
        // 再删子设备
        _deviceDao.DeleteByIntegratorId(id);
        // 最后删集成设备
        return _dao.Delete(id) > 0;
    }
}
