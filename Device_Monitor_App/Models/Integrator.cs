using SQLite;
using System.Text.Json.Serialization;

namespace Device_Monitor_App.Models;

/// <summary>
/// 集成设备表（TCP 网关，485 接口集成多个子设备）
/// </summary>
[Table("Integrator")]
public class Integrator
{
    [PrimaryKey, AutoIncrement]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>集成设备名称</summary>
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
