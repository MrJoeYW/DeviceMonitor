using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Models;
using Device_Monitor_App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.Services;

/// <summary>
/// 设备业务服务实现
/// </summary>
public class DeviceService : IDeviceService
{
    private readonly IDeviceDao _deviceDao;
    private readonly IDeviceTagMappingDao _tagDao;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(IDeviceDao deviceDao, IDeviceTagMappingDao tagDao, ILogger<DeviceService> logger)
    {
        _deviceDao = deviceDao;
        _tagDao = tagDao;
        _logger = logger;
    }

    public IEnumerable<Device> GetAll() => _deviceDao.GetAll();

    public IEnumerable<Device> GetByIntegratorId(int integratorId) => _deviceDao.GetByIntegratorId(integratorId);

    public Device? GetById(int id) => _deviceDao.GetById(id);

    public int Add(Device device)
    {
        if (string.IsNullOrWhiteSpace(device.Name))
            throw new ArgumentException("设备名称不能为空");
        if (device.IntegratorId <= 0)
            throw new ArgumentException("必须指定所属集成设备");

        return _deviceDao.Insert(device);
    }

    public bool Update(Device device)
    {
        if (device.Id <= 0)
            throw new ArgumentException("无效的设备 ID");
        return _deviceDao.Update(device) > 0;
    }

    /// <summary>级联删除：设备 → 标签映射</summary>
    public bool Delete(int id)
    {
        _logger.LogInformation("级联删除设备 ID={Id} 及其标签映射", id);
        _tagDao.DeleteByDeviceId(id);
        return _deviceDao.Delete(id) > 0;
    }
}
