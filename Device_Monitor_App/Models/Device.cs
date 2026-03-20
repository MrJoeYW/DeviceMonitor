using SQLite;
using System.Text.Json.Serialization;

namespace Device_Monitor_App.Models;

/// <summary>
/// Modbus 设备实例实体。
/// </summary>
[Table("Device")]
public class Device
{
    /// <summary>
    /// 主键 ID。
    /// </summary>
    [PrimaryKey, AutoIncrement]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 所属网关 ID。
    /// </summary>
    [Indexed, NotNull]
    [JsonPropertyName("integratorId")]
    public int IntegratorId { get; set; }

    /// <summary>
    /// 设备名称。
    /// </summary>
    [NotNull, MaxLength(100)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 设备类型，例如 FlowMeter、PowerMeter。
    /// </summary>
    [MaxLength(50)]
    [JsonPropertyName("deviceType")]
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// 设备型号，例如 AMC 系列、BTW-289+。
    /// </summary>
    [MaxLength(100)]
    [JsonPropertyName("deviceModel")]
    public string DeviceModel { get; set; } = string.Empty;

    /// <summary>
    /// 模板键，用于自动生成默认采集块和测点。
    /// </summary>
    [MaxLength(100)]
    [JsonPropertyName("templateKey")]
    public string TemplateKey { get; set; } = string.Empty;

    /// <summary>
    /// Modbus 从站地址。
    /// </summary>
    [NotNull]
    [JsonPropertyName("slaveAddress")]
    public int SlaveAddress { get; set; } = 1;

    /// <summary>
    /// 轮询周期，单位毫秒。
    /// </summary>
    [NotNull]
    [JsonPropertyName("pollIntervalMs")]
    public int PollIntervalMs { get; set; } = 1000;

    /// <summary>
    /// 运行态在线状态。
    /// </summary>
    [JsonPropertyName("isOnline")]
    public bool IsOnline { get; set; }

    /// <summary>
    /// 是否启用该设备。
    /// </summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; } = true;
}
