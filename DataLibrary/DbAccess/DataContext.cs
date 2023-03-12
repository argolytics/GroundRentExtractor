using System.Data.SqlClient;

namespace DataLibrary.DbAccess;

public class DataContext : IDataContext
{
    private string _connectionString;

    public DataContext(string connectionString)
    {
        this._connectionString = connectionString;
    }

    public IUnitOfWork CreateUnitOfWork()
    {
        var connection = new SqlConnection(this._connectionString);

        connection.Open();
        return new UnitOfWork(connection);
    }
}