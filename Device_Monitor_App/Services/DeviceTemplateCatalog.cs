using Device_Monitor_App.Models;

namespace Device_Monitor_App.Services;

public static class DeviceTemplateCatalog
{
    private static readonly IReadOnlyList<DeviceTemplateDefinition> Templates =
    [
        new AirSpeedMeterTemplate(),
        new PowerMeterTemplate(),
        new AirConditionerTemplate(),
        new FlowMeterTemplate()
    ];

    public static IEnumerable<DeviceTemplateSummary> GetSummaries()
    {
        return Templates.Select(template => template.ToSummary()).ToList();
    }

    public static DeviceTemplateDefinition? Resolve(string? templateKey, string? deviceType, string? deviceModel)
    {
        var normalizedKey = NormalizeKey(templateKey);
        var byKey = Templates.FirstOrDefault(template => NormalizeKey(template.Key) == normalizedKey);
        if (byKey is not null)
        {
            return byKey;
        }

        var normalizedType = NormalizeKey(deviceType);
        var normalizedModel = NormalizeKey(deviceModel);

        return Templates.FirstOrDefault(template =>
            NormalizeKey(template.DeviceType) == normalizedType &&
            (string.IsNullOrWhiteSpace(normalizedModel) || NormalizeKey(template.DeviceModel) == normalizedModel))
            ?? Templates.FirstOrDefault(template => NormalizeKey(template.DeviceType) == normalizedType);
    }

    public static string NormalizeKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var chars = value.Trim().ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '_')
            .ToArray();

        return new string(chars).Trim('_');
    }
}

public abstract class DeviceTemplateDefinition
{
    public abstract string Key { get; }
    public abstract string DeviceType { get; }
    public abstract string DeviceModel { get; }
    public abstract string Description { get; }

    public abstract IReadOnlyList<ReadGroupTemplateDefinition> BuildReadGroups();

    public DeviceTemplateSummary ToSummary()
    {
        var groups = BuildReadGroups();
        return new DeviceTemplateSummary
        {
            Key = Key,
            DeviceType = DeviceType,
            DeviceModel = DeviceModel,
            Description = Description,
            ReadGroupCount = groups.Count,
            PointCount = groups.Sum(group => group.Points.Count)
        };
    }

    protected static PointTemplateDefinition Point(
        string pointKey,
        string displayName,
        int registerAddress,
        string dataType,
        double scale,
        string unit,
        int sortOrder,
        int registerLength = 1,
        string notes = "")
    {
        return new PointTemplateDefinition(pointKey, displayName, registerAddress, registerLength, dataType, scale, unit, sortOrder, notes);
    }
}

public sealed record ReadGroupTemplateDefinition(
    string Name,
    int FunctionCode,
    int StartRegister,
    int RegisterCount,
    int SortOrder,
    IReadOnlyList<PointTemplateDefinition> Points);

public sealed record PointTemplateDefinition(
    string PointKey,
    string DisplayName,
    int RegisterAddress,
    int RegisterLength,
    string DataType,
    double Scale,
    string Unit,
    int SortOrder,
    string Notes);

internal sealed class AirSpeedMeterTemplate : DeviceTemplateDefinition
{
    public override string Key => "air_speed_meter_5_channel";
    public override string DeviceType => "AirSpeedMeter";
    public override string DeviceModel => "5路皮托管";
    public override string Description => "依据文档预置 1~5 路压力、风速、温度点位。";

    public override IReadOnlyList<ReadGroupTemplateDefinition> BuildReadGroups()
    {
        return
        [
            new ReadGroupTemplateDefinition(
                "默认采集块",
                3,
                0x0000,
                15,
                1,
                [
                    Point("pressure_1", "1号传感器压力", 0x0000, "Int16", 1.0, "Pa", 1),
                    Point("pressure_2", "2号传感器压力", 0x0001, "Int16", 1.0, "Pa", 2),
                    Point("pressure_3", "3号传感器压力", 0x0002, "Int16", 1.0, "Pa", 3),
                    Point("pressure_4", "4号传感器压力", 0x0003, "Int16", 1.0, "Pa", 4),
                    Point("pressure_5", "5号传感器压力", 0x0004, "Int16", 1.0, "Pa", 5),
                    Point("speed", "1号传感器风速", 0x0005, "UInt16", 0.1, "m/s", 6),
                    Point("speed_2", "2号传感器风速", 0x0006, "UInt16", 0.1, "m/s", 7),
                    Point("speed_3", "3号传感器风速", 0x0007, "UInt16", 0.1, "m/s", 8),
                    Point("speed_4", "4号传感器风速", 0x0008, "UInt16", 0.1, "m/s", 9),
                    Point("speed_5", "5号传感器风速", 0x0009, "UInt16", 0.1, "m/s", 10),
                    Point("temperature", "1号传感器温度", 0x000A, "UInt16", 1.0, "C", 11),
                    Point("temperature_2", "2号传感器温度", 0x000B, "UInt16", 1.0, "C", 12),
                    Point("temperature_3", "3号传感器温度", 0x000C, "UInt16", 1.0, "C", 13),
                    Point("temperature_4", "4号传感器温度", 0x000D, "UInt16", 1.0, "C", 14),
                    Point("temperature_5", "5号传感器温度", 0x000E, "UInt16", 1.0, "C", 15)
                ])
        ];
    }
}

internal sealed class PowerMeterTemplate : DeviceTemplateDefinition
{
    public override string Key => "power_meter_amc";
    public override string DeviceType => "PowerMeter";
    public override string DeviceModel => "AMC系列";
    public override string Description => "依据文档预置电压、电流、功率和功率因数点位。";

    public override IReadOnlyList<ReadGroupTemplateDefinition> BuildReadGroups()
    {
        return
        [
            new ReadGroupTemplateDefinition(
                "默认采集块",
                3,
                0x0100,
                21,
                1,
                [
                    Point("voltage", "相电压 UA", 0x0100, "UInt16", 0.1, "V", 1),
                    Point("voltage_ub", "相电压 UB", 0x0101, "UInt16", 0.1, "V", 2),
                    Point("voltage_uc", "相电压 UC", 0x0102, "UInt16", 0.1, "V", 3),
                    Point("line_voltage_uab", "线电压 UAB", 0x0103, "UInt16", 0.1, "V", 4),
                    Point("line_voltage_ubc", "线电压 UBC", 0x0104, "UInt16", 0.1, "V", 5),
                    Point("line_voltage_uca", "线电压 UCA", 0x0105, "UInt16", 0.1, "V", 6),
                    Point("current", "相电流 IA", 0x0106, "UInt16", 0.001, "A", 7),
                    Point("current_ib", "相电流 IB", 0x0107, "UInt16", 0.001, "A", 8),
                    Point("current_ic", "相电流 IC", 0x0108, "UInt16", 0.001, "A", 9),
                    Point("power_a", "A相有功功率", 0x0109, "UInt16", 0.001, "kW", 10),
                    Point("power_b", "B相有功功率", 0x010A, "UInt16", 0.001, "kW", 11),
                    Point("power_c", "C相有功功率", 0x010B, "UInt16", 0.001, "kW", 12),
                    Point("power", "总有功功率 Pt", 0x010C, "UInt16", 0.001, "kW", 13),
                    Point("reactive_power", "总无功功率 Qt", 0x0110, "UInt16", 0.001, "kvar", 14),
                    Point("power_factor", "总功率因数", 0x0114, "UInt16", 0.001, string.Empty, 15)
                ])
        ];
    }
}

internal sealed class AirConditionerTemplate : DeviceTemplateDefinition
{
    public override string Key => "air_conditioner_btw_289";
    public override string DeviceType => "AirConditioner";
    public override string DeviceModel => "BTW-289+";
    public override string Description => "依据文档预置温度和继电器状态点位。";

    public override IReadOnlyList<ReadGroupTemplateDefinition> BuildReadGroups()
    {
        return
        [
            new ReadGroupTemplateDefinition(
                "默认采集块",
                3,
                0x0000,
                5,
                1,
                [
                    Point("temperature", "柜温探头温度", 0x0000, "UInt16", 0.1, "C", 1),
                    Point("settemperature", "蒸发器探头温度", 0x0001, "UInt16", 0.1, "C", 2),
                    Point("cooling_relay", "制冷继电器状态", 0x0002, "UInt16", 1.0, string.Empty, 3),
                    Point("fan_relay", "风机继电器状态", 0x0003, "UInt16", 1.0, string.Empty, 4),
                    Point("alarm_relay", "报警继电器状态", 0x0004, "UInt16", 1.0, string.Empty, 5)
                ])
        ];
    }
}

internal sealed class FlowMeterTemplate : DeviceTemplateDefinition
{
    public override string Key => "flow_meter_fl3w7";
    public override string DeviceType => "FlowMeter";
    public override string DeviceModel => "FL3W7系列";
    public override string Description => "依据文档预置瞬时流量、累计量和温度相关点位。";

    public override IReadOnlyList<ReadGroupTemplateDefinition> BuildReadGroups()
    {
        return
        [
            new ReadGroupTemplateDefinition(
                "默认采集块",
                4,
                0x0000,
                9,
                1,
                [
                    Point("flow", "实时流量值", 0x0000, "UInt16", 0.01, string.Empty, 1),
                    Point("out1_setting", "显示 OUT1 的设定值", 0x0001, "UInt16", 0.01, string.Empty, 2),
                    Point("out1_total_high", "OUT1 累计流量高16位", 0x0002, "UInt16", 0.01, string.Empty, 3),
                    Point("out1_total_low", "OUT1 累计流量低16位", 0x0003, "UInt16", 0.01, string.Empty, 4),
                    Point("flow_valley", "流量谷值显示", 0x0004, "UInt16", 0.01, string.Empty, 5),
                    Point("flow_peak", "流量峰值显示", 0x0005, "UInt16", 0.01, string.Empty, 6),
                    Point("temperature_peak", "温度峰值", 0x0006, "UInt16", 0.01, "C", 7),
                    Point("temperature_valley", "温度谷值", 0x0007, "UInt16", 0.01, "C", 8),
                    Point("temperature", "流体温度值", 0x0008, "UInt16", 0.01, "C", 9)
                ])
        ];
    }
}
