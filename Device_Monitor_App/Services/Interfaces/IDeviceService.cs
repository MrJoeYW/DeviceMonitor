using Device_Monitor_App.Models;

namespace Device_Monitor_App.Services.Interfaces;

/// <summary>
/// 设备业务服务接口
/// </summary>
public interface IDeviceService
{
    /// <summary>获取所有设备列表</summary>
    IEnumerable<Device> GetAllDevices();

    /// <summary>根据 ID 获取设备</summary>
    Device? GetDevice(int id);

    /// <summary>添加设备</summary>
    int AddDevice(Device device);

    /// <summary>更新设备信息</summary>
    bool UpdateDevice(Device device);

    /// <summary>删除设备</summary>
    bool DeleteDevice(int id);

    /// <summary>标记设备在线/离线</summary>
    void SetDeviceOnlineStatus(int id, bool isOnline);
}
