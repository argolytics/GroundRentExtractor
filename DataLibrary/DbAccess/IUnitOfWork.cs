using System.Data.SqlClient;

namespace DataLibrary.DbAccess;

public interface IUnitOfWork : IDisposable
{
    SqlConnection Connection { get; }
    bool IsDisposed { get; }
    SqlTransaction Transaction { get; }
    void BeginTransaction();
    Task CommitAsync();
    Task RollbackAsync();
}