using SQLite;
using System.Text.Json.Serialization;

namespace Device_Monitor_App.Models;

/// <summary>
/// 标签映射表 —— 将设备 Modbus 读回的字节偏移解析为实际值，并映射到网关 PLC 数组的偏移位置
/// </summary>
[Table("DeviceTagMapping")]
public class DeviceTagMapping
{
    [PrimaryKey, AutoIncrement]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>关联 Device.Id</summary>
    [Indexed, NotNull]
    [JsonPropertyName("deviceId")]
    public int DeviceId { get; set; }

    /// <summary>读取值名称（如 Temperature, FlowRate）</summary>
    [NotNull, MaxLength(50)]
    [JsonPropertyName("valueName")]
    public string ValueName { get; set; } = string.Empty;

    /// <summary>Modbus 读回数组中的字节偏移</summary>
    [JsonPropertyName("registerOffset")]
    public int RegisterOffset { get; set; } = 0;

    /// <summary>数据类型（Int16, UInt16, Int32, UInt32, Float32 等）</summary>
    [MaxLength(20)]
    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = "Float32";

    /// <summary>缩放系数（原始值 × Scale = 实际值）</summary>
    [JsonPropertyName("scale")]
    public double Scale { get; set; } = 1.0;

    /// <summary>【核心】在该网关 PLC 数组中的偏移量（0, 1, 2...）</summary>
    [JsonPropertyName("plcOffset")]
    public int PlcOffset { get; set; } = 0;

    /// <summary>是否启用</summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; } = true;
}
