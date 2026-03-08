using SQLite;
using System.Text.Json.Serialization;

namespace Device_Monitor_App.Models;

/// <summary>
/// Modbus 子设备表（挂在集成设备下，通过 485 通讯）
/// </summary>
[Table("Device")]
public class Device
{
    [PrimaryKey, AutoIncrement]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>设备名称</summary>
    [NotNull, MaxLength(100)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>关联的集成设备 ID</summary>
    [Indexed, NotNull]
    [JsonPropertyName("integratorId")]
    public int IntegratorId { get; set; }

    /// <summary>Modbus 从站地址（1~247）</summary>
    [NotNull]
    [JsonPropertyName("slaveAddress")]
    public int SlaveAddress { get; set; } = 1;

    /// <summary>波特率</summary>
    [NotNull]
    [JsonPropertyName("baudRate")]
    public int BaudRate { get; set; } = 9600;

    /// <summary>数据位</summary>
    [JsonPropertyName("dataBits")]
    public int DataBits { get; set; } = 8;

    /// <summary>停止位（1 或 2）</summary>
    [JsonPropertyName("stopBits")]
    public int StopBits { get; set; } = 1;

    /// <summary>校验方式（None / Odd / Even）</summary>
    [MaxLength(10)]
    [JsonPropertyName("parity")]
    public string Parity { get; set; } = "None";

    /// <summary>读取功能码（HoldingRegisters / InputRegisters）</summary>
    [MaxLength(30)]
    [JsonPropertyName("readMode")]
    public string ReadMode { get; set; } = "HoldingRegisters";

    /// <summary>起始寄存器地址</summary>
    [JsonPropertyName("startRegister")]
    public int StartRegister { get; set; }

    /// <summary>读取寄存器数量</summary>
    [JsonPropertyName("registerCount")]
    public int RegisterCount { get; set; } = 2;

    /// <summary>设备类型（FlowMeter 等）</summary>
    [MaxLength(50)]
    [JsonPropertyName("deviceType")]
    public string DeviceType { get; set; } = "FlowMeter";

    /// <summary>是否启用</summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>备注</summary>
    [MaxLength(500)]
    [JsonPropertyName("remark")]
    public string Remark { get; set; } = string.Empty;

    /// <summary>创建时间</summary>
    [NotNull]
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
