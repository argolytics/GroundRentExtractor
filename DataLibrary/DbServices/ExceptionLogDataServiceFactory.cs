using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class ExceptionLogDataServiceFactory
{
    public IExceptionLogDataService CreateExceptionLogDataService(IUnitOfWork uow) => new ExceptionLogSqlDataService(uow);
}
