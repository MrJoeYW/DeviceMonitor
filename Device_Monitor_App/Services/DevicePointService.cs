using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Models;
using Device_Monitor_App.Services.Interfaces;

namespace Device_Monitor_App.Services;

public class DevicePointService : IDevicePointService
{
    private readonly IDevicePointDao _dao;

    public DevicePointService(IDevicePointDao dao)
    {
        _dao = dao;
    }

    public IEnumerable<DevicePoint> GetByDeviceId(int deviceId) => _dao.GetByDeviceId(deviceId);

    public int Add(DevicePoint point)
    {
        Normalize(point);
        Validate(point);
        return _dao.Insert(point);
    }

    public bool Update(DevicePoint point)
    {
        if (point.Id <= 0)
        {
            throw new ArgumentException("无效的测点 ID");
        }

        Normalize(point);
        Validate(point);
        return _dao.Update(point) > 0;
    }

    public bool Delete(int id) => _dao.Delete(id) > 0;

    private static void Normalize(DevicePoint point)
    {
        point.DisplayName = point.DisplayName.Trim();
        if (string.IsNullOrWhiteSpace(point.PointKey))
        {
            point.PointKey = point.DisplayName;
        }

        point.PointKey = DeviceTemplateCatalog.NormalizeKey(point.PointKey);
        if (point.RegisterLength <= 0)
        {
            point.RegisterLength = 1;
        }
    }

    private static void Validate(DevicePoint point)
    {
        if (point.DeviceId <= 0)
        {
            throw new ArgumentException("必须指定所属设备");
        }

        if (point.ReadGroupId <= 0)
        {
            throw new ArgumentException("必须指定所属采集块");
        }

        if (string.IsNullOrWhiteSpace(point.DisplayName))
        {
            throw new ArgumentException("测点名称不能为空");
        }
    }
}
