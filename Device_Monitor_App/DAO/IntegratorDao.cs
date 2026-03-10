using Device_Monitor_App.DAO.Interfaces;
using Device_Monitor_App.Infrastructure.Database;
using Device_Monitor_App.Models;
using Microsoft.Extensions.Logging;

namespace Device_Monitor_App.DAO;

/// <summary>
/// 网关 DAO 实现
/// </summary>
public class IntegratorDao : IIntegratorDao
{
    private readonly DatabaseContext _db;
    private readonly ILogger<IntegratorDao> _logger;

    public IntegratorDao(DatabaseContext db, ILogger<IntegratorDao> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IEnumerable<Integrator> GetAll()
    {
        return _db.Connection.Table<Integrator>().ToList();
    }

    public Integrator? GetById(int id)
    {
        return _db.Connection.Find<Integrator>(id);
    }

    public int Insert(Integrator integrator)
    {
        _db.Connection.Insert(integrator);
        _logger.LogInformation("新增网关: {Name}, ID={Id}", integrator.Name, integrator.Id);
        return integrator.Id;
    }

    public int Update(Integrator integrator)
    {
        var result = _db.Connection.Update(integrator);
        _logger.LogInformation("更新网关: {Name}, ID={Id}", integrator.Name, integrator.Id);
        return result;
    }

    public int Delete(int id)
    {
        var result = _db.Connection.Delete<Integrator>(id);
        _logger.LogInformation("删除网关 ID={Id}", id);
        return result;
    }
}
