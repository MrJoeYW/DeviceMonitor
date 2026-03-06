using SQLite;

namespace Device_Monitor_App.Models;

/// <summary>
/// 设备表模型（对应 SQLite 表 Device）
/// </summary>
[Table("Device")]
public class Device
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>设备名称</summary>
    [NotNull, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>设备IP地址</summary>
    [MaxLength(50)]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>设备类型（如: PLC, Scanner, Camera）</summary>
    [MaxLength(50)]
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>是否在线</summary>
    public bool IsOnline { get; set; }

    /// <summary>最后心跳时间</summary>
    public DateTime LastHeartbeat { get; set; }

    /// <summary>备注</summary>
    [MaxLength(500)]
    public string Remark { get; set; } = string.Empty;

    /// <summary>创建时间</summary>
    [NotNull]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
