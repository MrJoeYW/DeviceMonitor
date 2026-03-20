using SQLite;
using System.Text.Json.Serialization;

namespace Device_Monitor_App.Models;

/// <summary>
/// 设备测点实体。
/// 同时描述 Modbus 解析规则和 PLC 映射规则。
/// </summary>
[Table("DevicePoint")]
public class DevicePoint
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
    /// 所属采集块 ID。
    /// </summary>
    [Indexed, NotNull]
    [JsonPropertyName("readGroupId")]
    public int ReadGroupId { get; set; }

    /// <summary>
    /// 测点键，供程序内部和前端展示逻辑使用。
    /// </summary>
    [NotNull, MaxLength(100)]
    [JsonPropertyName("pointKey")]
    public string PointKey { get; set; } = string.Empty;

    /// <summary>
    /// 测点显示名称。
    /// </summary>
    [NotNull, MaxLength(100)]
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 测点所在寄存器地址。
    /// </summary>
    [JsonPropertyName("registerAddress")]
    public int RegisterAddress { get; set; }

    /// <summary>
    /// 该测点占用的寄存器个数。
    /// </summary>
    [JsonPropertyName("registerLength")]
    public int RegisterLength { get; set; } = 1;

    /// <summary>
    /// 数据类型，例如 UInt16、Float32。
    /// </summary>
    [MaxLength(20)]
    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = "UInt16";

    /// <summary>
    /// 缩放系数。原始值乘以该系数后得到最终值。
    /// </summary>
    [JsonPropertyName("scale")]
    public double Scale { get; set; } = 1.0;

    /// <summary>
    /// 工程单位，例如 V、A、kW。
    /// </summary>
    [MaxLength(20)]
    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// PLC 目标地址，例如 D1000、M200。
    /// </summary>
    [MaxLength(100)]
    [JsonPropertyName("plcAddress")]
    public string PlcAddress { get; set; } = string.Empty;

    /// <summary>
    /// 备注信息，通常记录协议说明或来源。
    /// </summary>
    [MaxLength(200)]
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// 调度/展示排序值。
    /// </summary>
    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }

    /// <summary>
    /// 是否启用该测点。
    /// </summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; } = true;
}
