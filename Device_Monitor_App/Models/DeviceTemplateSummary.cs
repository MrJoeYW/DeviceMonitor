using System.Text.Json.Serialization;

namespace Device_Monitor_App.Models;

/// <summary>
/// 设备模板摘要信息。
/// 用于前端选择模板时快速展示模板能力。
/// </summary>
public class DeviceTemplateSummary
{
    /// <summary>
    /// 模板唯一键。
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 模板对应的设备类型。
    /// </summary>
    [JsonPropertyName("deviceType")]
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// 模板对应的设备型号。
    /// </summary>
    [JsonPropertyName("deviceModel")]
    public string DeviceModel { get; set; } = string.Empty;

    /// <summary>
    /// 模板说明文字。
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 模板默认生成的采集块数量。
    /// </summary>
    [JsonPropertyName("readGroupCount")]
    public int ReadGroupCount { get; set; }

    /// <summary>
    /// 模板默认生成的测点数量。
    /// </summary>
    [JsonPropertyName("pointCount")]
    public int PointCount { get; set; }
}
