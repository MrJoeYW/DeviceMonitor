using Device_Monitor_App.Models;

namespace Device_Monitor_App.DAO.Interfaces;

/// <summary>
/// 设备 DAO 接口
/// </summary>
public interface IDeviceDao
{
    /// <summary>获取所有设备</summary>
    IEnumerable<Device> GetAll();

    /// <summary>根据 ID 获取设备</summary>
    Device? GetById(int id);

    /// <summary>插入设备，返回新 ID</summary>
    int Insert(Device device);

    /// <summary>更新设备信息</summary>
    int Update(Device device);

    /// <summary>删除设备</summary>
    int Delete(int id);

    /// <summary>更新设备在线状态</summary>
    void UpdateOnlineStatus(int id, bool isOnline, DateTime heartbeat);
}
