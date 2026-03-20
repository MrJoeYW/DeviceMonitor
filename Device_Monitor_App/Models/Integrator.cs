using SQLite;
using System.Text.Json.Serialization;

namespace Device_Monitor_App.Models;

/// <summary>
/// TCP 网关/集成器实体。
/// </summary>
[Table("Integrator")]
public class Integrator
{
    /// <summary>
    /// 主键 ID。
    /// </summary>
    [PrimaryKey, AutoIncrement]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 网关名称，用于界面展示和现场区分。
    /// </summary>
    [NotNull, MaxLength(100)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 网关的 TCP IP 地址。
    /// </summary>
    [NotNull, MaxLength(50)]
    [JsonPropertyName("ipAddress")]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// 网关的 TCP 端口。
    /// </summary>
    [NotNull]
    [JsonPropertyName("port")]
    public int Port { get; set; } = 502;

    /// <summary>
    /// 是否启用该网关。
    /// </summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; } = true;
}
