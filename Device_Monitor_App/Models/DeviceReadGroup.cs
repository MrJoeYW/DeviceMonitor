using SQLite;
using System.Text.Json.Serialization;

namespace Device_Monitor_App.Models;

/// <summary>
/// 设备采集块实体。
/// 用于描述一次批量读取的寄存器范围。
/// </summary>
[Table("DeviceReadGroup")]
public class DeviceReadGroup
{
    /// <summary>
    /// 主键 ID。
    /// </summary>
    [PrimaryKey, AutoIncrement]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 所属设备 ID。
    /// </summary>
    [Indexed, NotNull]
    [JsonPropertyName("deviceId")]
    public int DeviceId { get; set; }

    /// <summary>
    /// 采集块名称。
    /// </summary>
    [NotNull, MaxLength(100)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Modbus 功能码，例如 3 或 4。
    /// </summary>
    [NotNull]
    [JsonPropertyName("functionCode")]
    public int FunctionCode { get; set; } = 3;

    /// <summary>
    /// 批量读取的起始寄存器地址。
    /// </summary>
    [NotNull]
    [JsonPropertyName("startRegister")]
    public int StartRegister { get; set; }

    /// <summary>
    /// 批量读取的寄存器长度。
    /// </summary>
    [NotNull]
    [JsonPropertyName("registerCount")]
    public int RegisterCount { get; set; }

    /// <summary>
    /// 调度/展示排序值。
    /// </summary>
    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }

    /// <summary>
    /// 是否启用该采集块。
    /// </summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; } = true;
}
