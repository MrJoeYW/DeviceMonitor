using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Models;
using Device_Monitor_App.Services.Interfaces;

namespace Device_Monitor_App.Services;

public class DeviceReadGroupService : IDeviceReadGroupService
{
    private readonly IDeviceReadGroupDao _dao;
    private readonly IDevicePointDao _pointDao;

    public DeviceReadGroupService(IDeviceReadGroupDao dao, IDevicePointDao pointDao)
    {
        _dao = dao;
        _pointDao = pointDao;
    }

    public IEnumerable<DeviceReadGroup> GetByDeviceId(int deviceId) => _dao.GetByDeviceId(deviceId);

    public int Add(DeviceReadGroup group)
    {
        Validate(group);
        return _dao.Insert(group);
    }

    public bool Update(DeviceReadGroup group)
    {
        if (group.Id <= 0)
        {
            throw new ArgumentException("无效的采集块 ID");
        }

        Validate(group);
        return _dao.Update(group) > 0;
    }

    public bool Delete(int id)
    {
        _pointDao.DeleteByReadGroupId(id);
        return _dao.Delete(id) > 0;
    }

    private static void Validate(DeviceReadGroup group)
    {
        if (group.DeviceId <= 0)
        {
            throw new ArgumentException("必须指定所属设备");
        }

        if (string.IsNullOrWhiteSpace(group.Name))
        {
            throw new ArgumentException("采集块名称不能为空");
        }

        if (group.RegisterCount <= 0)
        {
            throw new ArgumentException("寄存器长度必须大于 0");
        }
    }
}
