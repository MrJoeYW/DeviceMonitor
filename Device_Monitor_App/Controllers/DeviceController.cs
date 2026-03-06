using Device_Monitor_App.Models;
using Device_Monitor_App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.Controllers;

/// <summary>
/// 设备控制器 —— 作为 WebView2 JS Bridge 与前端的通信入口
/// </summary>
public class DeviceController
{
    private readonly IDeviceService _deviceService;
    private readonly ILogger<DeviceController> _logger;

    public DeviceController(IDeviceService deviceService, ILogger<DeviceController> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    /// <summary>获取所有设备（供 JS 调用）</summary>
    public IEnumerable<Device> GetDevices()
    {
        _logger.LogDebug("前端请求: 获取设备列表");
        return _deviceService.GetAllDevices();
    }

    /// <summary>添加设备（供 JS 调用）</summary>
    public int AddDevice(string name, string ipAddress, string deviceType, string remark = "")
    {
        _logger.LogInformation("前端请求: 添加设备 {Name}", name);
        var device = new Device
        {
            Name = name,
            IpAddress = ipAddress,
            DeviceType = deviceType,
            Remark = remark
        };
        return _deviceService.AddDevice(device);
    }

    /// <summary>删除设备（供 JS 调用）</summary>
    public bool DeleteDevice(int id)
    {
        _logger.LogInformation("前端请求: 删除设备 ID={Id}", id);
        return _deviceService.DeleteDevice(id);
    }

    /// <summary>更新设备在线状态</summary>
    public void UpdateOnlineStatus(int id, bool isOnline)
    {
        _deviceService.SetDeviceOnlineStatus(id, isOnline);
    }
}
