using SQLite;
using System.Text.Json.Serialization;

namespace Device_Monitor_App.Models;

/// <summary>
/// 网关表 —— 一个 TCP 网关管理一段连续 PLC 地址块
/// </summary>
[Table("Integrator")]
public class Integrator
{
    [PrimaryKey, AutoIncrement]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>网关名称</summary>
    [NotNull, MaxLength(100)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>IP 地址</summary>
    [NotNull, MaxLength(50)]
    [JsonPropertyName("ipAddress")]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>端口号</summary>
    [NotNull]
    [JsonPropertyName("port")]
    public int Port { get; set; } = 502;

    /// <summary>该网关对应的 PLC 起始地址，例如 "D1000"</summary>
    [NotNull, MaxLength(50)]
    [JsonPropertyName("plcBaseAddress")]
    public string PlcBaseAddress { get; set; } = string.Empty;

    /// <summary>该网关占用的 PLC 地址总长度，例如 100</summary>
    [NotNull]
    [JsonPropertyName("plcBlockSize")]
    public int PlcBlockSize { get; set; } = 100;

    /// <summary>是否启用</summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; } = true;
}
