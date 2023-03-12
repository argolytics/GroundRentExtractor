using System.Data.SqlClient;

namespace DataLibrary.DbAccess;

public class UnitOfWork : IUnitOfWork
{
    private SqlTransaction _transaction;
    private SqlConnection _connection;
    private bool _commitCalled;

    public UnitOfWork(SqlConnection connection)
    {
        this._connection = connection;
        this._commitCalled = false;
    }

    public bool IsDisposed { get; internal set; }

    public SqlConnection Connection => this._connection;

    public SqlTransaction Transaction => this._transaction;

    public void BeginTransaction()
    {
        this._transaction = this._connection.BeginTransaction();
    }

    public Task CommitAsync()
    {
        this._commitCalled = true;
        if (this._transaction != null)
            return _transaction.CommitAsync();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        this.IsDisposed = true;
        if (this._transaction != null)
        {
            if (!this._commitCalled)
            {
                try
                {
                    this._transaction.Rollback();
                }
                catch (Exception e)
                {

                }
            }
            this._transaction.Dispose();
        }
        if (this._connection != null)
        {
            this._connection.Dispose();
        }
    }

    public Task RollbackAsync()
    {
        if (this._transaction != null)
            return _transaction.RollbackAsync();

        return Task.CompletedTask;
    }
}

