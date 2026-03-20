using SQLite;

namespace Device_Monitor_App.Infrastructure.Database;

/// <summary>
/// SQLite 数据库上下文，负责管理连接生命周期。
/// </summary>
public class DatabaseContext : IDisposable
{
    private readonly SQLiteConnection _connection;
    private bool _disposed;

    public DatabaseContext(string dbPath)
    {
        _connection = new SQLiteConnection(dbPath);
    }

    /// <summary>
    /// 当前数据库连接实例。
    /// </summary>
    public SQLiteConnection Connection => _connection;

    /// <summary>
    /// 确保指定实体对应的数据表已创建。
    /// </summary>
    public void EnsureTable<T>() where T : new()
    {
        _connection.CreateTable<T>();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _connection.Close();
            _connection.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
