using SQLite;

namespace Device_Monitor_App.Infrastructure.Database;

/// <summary>
/// SQLite 数据库上下文，管理连接生命周期（Singleton）
/// </summary>
public class DatabaseContext : IDisposable
{
    private readonly SQLiteConnection _connection;
    private bool _disposed;

    public DatabaseContext(string dbPath)
    {
        _connection = new SQLiteConnection(dbPath);
    }

    public SQLiteConnection Connection => _connection;

    /// <summary>
    /// 注册并自动建表
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
