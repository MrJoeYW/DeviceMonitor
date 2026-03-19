using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Models;
using Device_Monitor_App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.Services;

/// <summary>
/// 设备数据快照服务 - 提供模拟的实时数据
/// </summary>
public interface ISnapshotService
{
    DeviceSnapshot GetSnapshot(Device device, List<DeviceTagMapping> tags);
    List<DeviceSnapshot> GetAllSnapshots();
}

public class SnapshotService : ISnapshotService
{
    private readonly ILogger<SnapshotService> _logger;
    private readonly IDeviceDao _deviceDao;
    private readonly IDeviceTagMappingDao _tagDao;
    private readonly Random _random = new();

    // 模拟数据状态缓存
    private readonly Dictionary<int, DeviceDataState> _deviceStates = new();

    public SnapshotService(
        ILogger<SnapshotService> logger,
        IDeviceDao deviceDao,
        IDeviceTagMappingDao tagDao)
    {
        _logger = logger;
        _deviceDao = deviceDao;
        _tagDao = tagDao;
    }

    public DeviceSnapshot GetSnapshot(Device device, List<DeviceTagMapping> tags)
    {
        if (!_deviceStates.TryGetValue(device.Id, out var state))
        {
            state = new DeviceDataState { DeviceId = device.Id };
            _deviceStates[device.Id] = state;
        }

        var snapshot = new DeviceSnapshot
        {
            DeviceId = device.Id,
            DeviceName = device.Name,
            DeviceType = device.DeviceType,
            IntegratorId = device.IntegratorId,
            IsEnabled = device.IsEnabled,
            SlaveAddress = device.SlaveAddress,
            Values = new Dictionary<string, double>()
        };

        if (!device.IsEnabled)
        {
            snapshot.Status = "disabled";
            return snapshot;
        }

        // 模拟在线状态
        var randVal = _random.Next(100);
        snapshot.Status = randVal < 85 ? "online" : (randVal < 95 ? "warning" : "offline");

        if (snapshot.Status == "offline")
        {
            return snapshot;
        }

        // 根据设备类型生成模拟数据
        foreach (var tag in tags.Where(t => t.IsEnabled))
        {
            var value = GenerateSimulatedValue(device.DeviceType, tag.ValueName, state);
            snapshot.Values[tag.ValueName] = Math.Round(value * tag.Scale, 2);
        }

        return snapshot;
    }

    public List<DeviceSnapshot> GetAllSnapshots()
    {
        var snapshots = new List<DeviceSnapshot>();
        var devices = _deviceDao.GetAll().ToList();

        foreach (var device in devices)
        {
            var tags = _tagDao.GetByDeviceId(device.Id).ToList();
            snapshots.Add(GetSnapshot(device, tags));
        }

        return snapshots;
    }

    private double GenerateSimulatedValue(string deviceType, string valueName, DeviceDataState state)
    {
        return deviceType.ToLower() switch
        {
            "flowmeter" or "流量计" => GenerateFlowMeterValue(valueName, state),
            "powermeter" or "电能表" => GeneratePowerMeterValue(valueName, state),
            "airspeedmeter" or "风速仪" => GenerateAirSpeedValue(valueName, state),
            "airconditioner" or "空调" => GenerateAirConditionerValue(valueName, state),
            _ => _random.NextDouble() * 100
        };
    }

    private double GenerateFlowMeterValue(string valueName, DeviceDataState state)
    {
        return valueName.ToLower() switch
        {
            "temperature" or "温度" => UpdateValue(ref state._temperature, 25, 60, 0.3),
            "flow" or "流量" => UpdateValue(ref state._flow, 40, 120, 2.0),
            "pressure" or "压力" => UpdateValue(ref state._pressure, 0.2, 0.8, 0.02),
            _ => _random.NextDouble() * 100
        };
    }

    private double GeneratePowerMeterValue(string valueName, DeviceDataState state)
    {
        return valueName.ToLower() switch
        {
            "voltage" or "电压" => UpdateValue(ref state._voltage, 215, 235, 0.5),
            "current" or "电流" => UpdateValue(ref state._current, 5, 50, 1.0),
            "power" or "功率" => UpdateValue(ref state._power, 1, 15, 0.3),
            "energy" or "电能" => state._energy += _random.NextDouble() * 0.1,
            "powerfactor" or "功率因数" => UpdateValue(ref state._powerFactor, 0.85, 0.98, 0.01),
            _ => _random.NextDouble() * 100
        };
    }

    private double GenerateAirSpeedValue(string valueName, DeviceDataState state)
    {
        return valueName.ToLower() switch
        {
            "speed" or "风速" => UpdateValue(ref state._airSpeed, 0.5, 15, 0.3),
            "direction" or "风向" => UpdateValue(ref state._airDirection, 0, 360, 10),
            _ => _random.NextDouble() * 100
        };
    }

    private double GenerateAirConditionerValue(string valueName, DeviceDataState state)
    {
        return valueName.ToLower() switch
        {
            "temperature" or "温度" or "settemperature" or "设定温度" => UpdateValue(ref state._temperature, 18, 28, 0.2),
            "humidity" or "湿度" => UpdateValue(ref state._humidity, 40, 70, 1.0),
            "mode" or "模式" => state._mode,
            _ => _random.NextDouble() * 100
        };
    }

    private double UpdateValue(ref double value, double min, double max, double maxDrift)
    {
        if (value == 0)
        {
            value = min + _random.NextDouble() * (max - min);
        }
        else
        {
            value += (_random.NextDouble() - 0.5) * 2 * maxDrift;
            value = Math.Clamp(value, min, max);
        }
        return value;
    }
}

/// <summary>
/// 设备数据快照
/// </summary>
public class DeviceSnapshot
{
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public int IntegratorId { get; set; }
    public bool IsEnabled { get; set; }
    public int SlaveAddress { get; set; }
    public string Status { get; set; } = "unknown";
    public Dictionary<string, double> Values { get; set; } = new();
}

/// <summary>
/// 设备模拟数据状态
/// </summary>
internal class DeviceDataState
{
    public int DeviceId { get; set; }

    // 流量计
    public double _temperature;
    public double _flow;
    public double _pressure;

    // 电能表
    public double _voltage;
    public double _current;
    public double _power;
    public double _energy = 12345.67;
    public double _powerFactor;

    // 风速仪
    public double _airSpeed;
    public double _airDirection;

    // 空调
    public double _humidity;
    public double _mode;
}
