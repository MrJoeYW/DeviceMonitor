using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Models;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.Services;

public interface ISnapshotService
{
    DeviceSnapshot GetSnapshot(Device device, List<DevicePoint> points);
    List<DeviceSnapshot> GetAllSnapshots();
}

public class SnapshotService : ISnapshotService
{
    private readonly ILogger<SnapshotService> _logger;
    private readonly IDeviceDao _deviceDao;
    private readonly IDevicePointDao _pointDao;
    private readonly Random _random = new();
    private readonly Dictionary<int, DeviceDataState> _deviceStates = new();

    public SnapshotService(
        ILogger<SnapshotService> logger,
        IDeviceDao deviceDao,
        IDevicePointDao pointDao)
    {
        _logger = logger;
        _deviceDao = deviceDao;
        _pointDao = pointDao;
    }

    public DeviceSnapshot GetSnapshot(Device device, List<DevicePoint> points)
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
            Status = "unknown",
            Values = new Dictionary<string, double>()
        };

        if (!device.IsEnabled)
        {
            snapshot.Status = "disabled";
            return snapshot;
        }

        var randVal = _random.Next(100);
        snapshot.Status = randVal < 85 ? "online" : randVal < 95 ? "warning" : "offline";

        if (snapshot.Status == "offline")
        {
            return snapshot;
        }

        foreach (var point in points.Where(item => item.IsEnabled))
        {
            var value = GenerateSimulatedValue(device.DeviceType, point.PointKey, state);
            snapshot.Values[point.PointKey] = Math.Round(value * point.Scale, 2);
        }

        return snapshot;
    }

    public List<DeviceSnapshot> GetAllSnapshots()
    {
        var snapshots = new List<DeviceSnapshot>();
        var devices = _deviceDao.GetAll().ToList();

        foreach (var device in devices)
        {
            var points = _pointDao.GetByDeviceId(device.Id).ToList();
            snapshots.Add(GetSnapshot(device, points));
        }

        _logger.LogDebug("已生成实时快照，Count={Count}", snapshots.Count);
        return snapshots;
    }

    private double GenerateSimulatedValue(string deviceType, string pointKey, DeviceDataState state)
    {
        return deviceType.ToLowerInvariant() switch
        {
            "flowmeter" => GenerateFlowMeterValue(pointKey, state),
            "powermeter" => GeneratePowerMeterValue(pointKey, state),
            "airspeedmeter" => GenerateAirSpeedValue(pointKey, state),
            "airconditioner" => GenerateAirConditionerValue(pointKey, state),
            _ => _random.NextDouble() * 100
        };
    }

    private double GenerateFlowMeterValue(string pointKey, DeviceDataState state)
    {
        return pointKey.ToLowerInvariant() switch
        {
            "temperature" or "temperature_peak" or "temperature_valley" => UpdateValue(ref state.Temperature, 25, 60, 0.3),
            "flow" or "flow_peak" or "flow_valley" => UpdateValue(ref state.Flow, 40, 120, 2.0),
            "out1_total_high" or "out1_total_low" => state.Energy += _random.NextDouble() * 0.1,
            _ => _random.NextDouble() * 100
        };
    }

    private double GeneratePowerMeterValue(string pointKey, DeviceDataState state)
    {
        return pointKey.ToLowerInvariant() switch
        {
            "voltage" or "voltage_ub" or "voltage_uc" or "line_voltage_uab" or "line_voltage_ubc" or "line_voltage_uca"
                => UpdateValue(ref state.Voltage, 215, 235, 0.5),
            "current" or "current_ib" or "current_ic"
                => UpdateValue(ref state.Current, 5, 50, 1.0),
            "power" or "power_a" or "power_b" or "power_c" or "reactive_power"
                => UpdateValue(ref state.Power, 1, 15, 0.3),
            "power_factor"
                => UpdateValue(ref state.PowerFactor, 0.85, 0.98, 0.01),
            _ => _random.NextDouble() * 100
        };
    }

    private double GenerateAirSpeedValue(string pointKey, DeviceDataState state)
    {
        return pointKey.ToLowerInvariant() switch
        {
            var key when key.StartsWith("speed") => UpdateValue(ref state.AirSpeed, 0.5, 15, 0.3),
            var key when key.StartsWith("pressure") => UpdateValue(ref state.Pressure, -120, 120, 5),
            var key when key.StartsWith("temperature") => UpdateValue(ref state.Temperature, 18, 35, 0.4),
            _ => _random.NextDouble() * 100
        };
    }

    private double GenerateAirConditionerValue(string pointKey, DeviceDataState state)
    {
        return pointKey.ToLowerInvariant() switch
        {
            "temperature" or "settemperature" => UpdateValue(ref state.Temperature, 18, 28, 0.2),
            "cooling_relay" or "fan_relay" or "alarm_relay" => _random.Next(0, 2),
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

internal class DeviceDataState
{
    public int DeviceId { get; set; }
    public double Temperature;
    public double Flow;
    public double Pressure;
    public double Voltage;
    public double Current;
    public double Power;
    public double Energy = 12345.67;
    public double PowerFactor;
    public double AirSpeed;
}
