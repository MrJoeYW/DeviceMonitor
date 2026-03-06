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
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(IDeviceDao deviceDao, ILogger<DeviceService> logger)
    {
        _deviceDao = deviceDao;
        _logger = logger;
    }

    public IEnumerable<Device> GetAllDevices()
    {
        return _deviceDao.GetAll();
    }

    public Device? GetDevice(int id)
    {
        return _deviceDao.GetById(id);
    }

    public int AddDevice(Device device)
    {
        if (string.IsNullOrWhiteSpace(device.Name))
            throw new ArgumentException("设备名称不能为空", nameof(device));

        _logger.LogInformation("添加设备: {Name}", device.Name);
        return _deviceDao.Insert(device);
    }

    public bool UpdateDevice(Device device)
    {
        if (device.Id <= 0)
            throw new ArgumentException("无效的设备 ID", nameof(device));

        _logger.LogInformation("更新设备: ID={Id}, Name={Name}", device.Id, device.Name);
        return _deviceDao.Update(device) > 0;
    }

    public bool DeleteDevice(int id)
    {
        _logger.LogInformation("删除设备: ID={Id}", id);
        return _deviceDao.Delete(id) > 0;
    }

    public void SetDeviceOnlineStatus(int id, bool isOnline)
    {
        _deviceDao.UpdateOnlineStatus(id, isOnline, DateTime.Now);
    }
}
