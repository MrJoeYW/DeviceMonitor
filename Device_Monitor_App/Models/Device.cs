using SQLite;

namespace Device_Monitor_App.Models;

/// <summary>
/// Modbus 子设备表（挂在集成设备下，通过 485 通讯）
/// </summary>
[Table("Device")]
public class Device
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>设备名称</summary>
    [NotNull, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>关联的集成设备 ID</summary>
    [Indexed, NotNull]
    public int IntegratorId { get; set; }

    /// <summary>Modbus 从站地址（1~247）</summary>
    [NotNull]
    public int SlaveAddress { get; set; } = 1;

    /// <summary>波特率</summary>
    [NotNull]
    public int BaudRate { get; set; } = 9600;

    /// <summary>数据位</summary>
    public int DataBits { get; set; } = 8;

    /// <summary>停止位（1 或 2）</summary>
    public int StopBits { get; set; } = 1;

    /// <summary>校验方式（None / Odd / Even）</summary>
    [MaxLength(10)]
    public string Parity { get; set; } = "None";

    /// <summary>读取功能码（HoldingRegisters / InputRegisters）</summary>
    [MaxLength(30)]
    public string ReadMode { get; set; } = "HoldingRegisters";

    /// <summary>起始寄存器地址</summary>
    public int StartRegister { get; set; }

    /// <summary>读取寄存器数量</summary>
    public int RegisterCount { get; set; } = 2;

    /// <summary>设备类型（FlowMeter 等）</summary>
    [MaxLength(50)]
    public string DeviceType { get; set; } = "FlowMeter";

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>备注</summary>
    [MaxLength(500)]
    public string Remark { get; set; } = string.Empty;

    /// <summary>创建时间</summary>
    [NotNull]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
