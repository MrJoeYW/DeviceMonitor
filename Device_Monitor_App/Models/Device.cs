using SQLite;
using System.Text.Json.Serialization;

namespace Device_Monitor_App.Models;

/// <summary>
/// 设备表 —— 挂在网关下的 Modbus 子设备，定义一次性读取的寄存器范围
/// </summary>
[Table("Device")]
public class Device
{
    [PrimaryKey, AutoIncrement]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>关联的网关 ID</summary>
    [Indexed, NotNull]
    [JsonPropertyName("integratorId")]
    public int IntegratorId { get; set; }

    /// <summary>设备名称</summary>
    [NotNull, MaxLength(100)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>设备类型（流量计、电能表等）</summary>
    [MaxLength(50)]
    [JsonPropertyName("deviceType")]
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>Modbus 站号（1~247）</summary>
    [NotNull]
    [JsonPropertyName("slaveAddress")]
    public int SlaveAddress { get; set; } = 1;

    /// <summary>读取功能码：3 = HoldingRegisters，4 = InputRegisters</summary>
    [NotNull]
    [JsonPropertyName("readFunctionCode")]
    public int ReadFunctionCode { get; set; } = 3;

    /// <summary>起始寄存器地址</summary>
    [JsonPropertyName("readStartRegister")]
    public int ReadStartRegister { get; set; } = 0;

    /// <summary>一次读取的寄存器数量</summary>
    [JsonPropertyName("readRegisterCount")]
    public int ReadRegisterCount { get; set; } = 10;

    /// <summary>是否在线（运行时更新，不持久化到 DB 但保留字段供查询）</summary>
    [JsonPropertyName("isOnline")]
    public bool IsOnline { get; set; } = false;

    /// <summary>是否启用</summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; } = true;
}
