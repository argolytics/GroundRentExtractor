namespace DataLibrary.DbAccess;

public interface IDataContext
{
    IUnitOfWork CreateUnitOfWork();
}