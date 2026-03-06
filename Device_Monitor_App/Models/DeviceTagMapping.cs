using SQLite;

namespace Device_Monitor_App.Models;

/// <summary>
/// 设备读取值 → PLC 标签地址映射表
/// </summary>
[Table("DeviceTagMapping")]
public class DeviceTagMapping
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>关联 Device.Id</summary>
    [Indexed, NotNull]
    public int DeviceId { get; set; }

    /// <summary>读取值名称（如 Temperature, FlowRate）</summary>
    [NotNull, MaxLength(50)]
    public string ValueName { get; set; } = string.Empty;

    /// <summary>相对于 StartRegister 的寄存器偏移</summary>
    public int RegisterOffset { get; set; }

    /// <summary>数据类型（Float / Int16 / UInt16 / Int32 等）</summary>
    [MaxLength(20)]
    public string DataType { get; set; } = "Float";

    /// <summary>缩放系数（读取的原始值 × Scale = 实际值）</summary>
    public double Scale { get; set; } = 1.0;

    /// <summary>PLC 标签地址（如 DB100.DBD10）</summary>
    [NotNull, MaxLength(100)]
    public string PlcTagAddress { get; set; } = string.Empty;

    /// <summary>单位（℃, L/min 等）</summary>
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; } = true;
}
