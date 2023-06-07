using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class ExceptionLogDataServiceFactory : IExceptionLogDataServiceFactory
{
    public IExceptionLogDataService CreateExceptionLogDataService(IUnitOfWork uow) => new ExceptionLogSqlDataService(uow);
}
