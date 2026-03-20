using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Models;
using Device_Monitor_App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceDao _deviceDao;
    private readonly IDeviceReadGroupDao _readGroupDao;
    private readonly IDevicePointDao _pointDao;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(
        IDeviceDao deviceDao,
        IDeviceReadGroupDao readGroupDao,
        IDevicePointDao pointDao,
        ILogger<DeviceService> logger)
    {
        _deviceDao = deviceDao;
        _readGroupDao = readGroupDao;
        _pointDao = pointDao;
        _logger = logger;
    }

    public IEnumerable<Device> GetAll() => _deviceDao.GetAll();

    public IEnumerable<Device> GetByIntegratorId(int integratorId) => _deviceDao.GetByIntegratorId(integratorId);

    public Device? GetById(int id) => _deviceDao.GetById(id);

    public IEnumerable<DeviceTemplateSummary> GetTemplates() => DeviceTemplateCatalog.GetSummaries();

    public int Add(Device device)
    {
        Validate(device);
        var newId = _deviceDao.Insert(device);
        device.Id = newId;
        EnsureTemplateConfiguration(device, overwriteExisting: false);
        return newId;
    }

    public bool Update(Device device)
    {
        if (device.Id <= 0)
        {
            throw new ArgumentException("无效的设备 ID");
        }

        Validate(device);
        var result = _deviceDao.Update(device) > 0;
        EnsureTemplateConfiguration(device, overwriteExisting: false);
        return result;
    }

    public bool Delete(int id)
    {
        _logger.LogInformation("级联删除设备 ID={Id}", id);
        _pointDao.DeleteByDeviceId(id);
        _readGroupDao.DeleteByDeviceId(id);
        return _deviceDao.Delete(id) > 0;
    }

    public bool RebuildTemplate(int id)
    {
        var device = _deviceDao.GetById(id) ?? throw new ArgumentException("设备不存在");
        _pointDao.DeleteByDeviceId(id);
        _readGroupDao.DeleteByDeviceId(id);
        EnsureTemplateConfiguration(device, overwriteExisting: true);
        return true;
    }

    private void Validate(Device device)
    {
        if (string.IsNullOrWhiteSpace(device.Name))
        {
            throw new ArgumentException("设备名称不能为空");
        }

        if (device.IntegratorId <= 0)
        {
            throw new ArgumentException("必须指定所属网关");
        }

        if (device.SlaveAddress is < 1 or > 247)
        {
            throw new ArgumentException("从站地址必须在 1 到 247 之间");
        }

        if (device.PollIntervalMs <= 0)
        {
            device.PollIntervalMs = 1000;
        }
    }

    private void EnsureTemplateConfiguration(Device device, bool overwriteExisting)
    {
        if (!overwriteExisting)
        {
            var hasReadGroups = _readGroupDao.GetByDeviceId(device.Id).Any();
            var hasPoints = _pointDao.GetByDeviceId(device.Id).Any();
            if (hasReadGroups || hasPoints)
            {
                return;
            }
        }

        var template = DeviceTemplateCatalog.Resolve(device.TemplateKey, device.DeviceType, device.DeviceModel);
        if (template is null)
        {
            return;
        }

        foreach (var groupTemplate in template.BuildReadGroups())
        {
            var group = new DeviceReadGroup
            {
                DeviceId = device.Id,
                Name = groupTemplate.Name,
                FunctionCode = groupTemplate.FunctionCode,
                StartRegister = groupTemplate.StartRegister,
                RegisterCount = groupTemplate.RegisterCount,
                SortOrder = groupTemplate.SortOrder,
                IsEnabled = true
            };

            group.Id = _readGroupDao.Insert(group);

            foreach (var pointTemplate in groupTemplate.Points)
            {
                _pointDao.Insert(new DevicePoint
                {
                    DeviceId = device.Id,
                    ReadGroupId = group.Id,
                    PointKey = pointTemplate.PointKey,
                    DisplayName = pointTemplate.DisplayName,
                    RegisterAddress = pointTemplate.RegisterAddress,
                    RegisterLength = pointTemplate.RegisterLength,
                    DataType = pointTemplate.DataType,
                    Scale = pointTemplate.Scale,
                    Unit = pointTemplate.Unit,
                    PlcAddress = string.Empty,
                    Notes = pointTemplate.Notes,
                    SortOrder = pointTemplate.SortOrder,
                    IsEnabled = true
                });
            }
        }

        _logger.LogInformation("已根据模板生成默认配置, DeviceId={DeviceId}, TemplateKey={TemplateKey}", device.Id, template.Key);
    }
}
