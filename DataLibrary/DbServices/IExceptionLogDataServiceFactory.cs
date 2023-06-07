using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public interface IExceptionLogDataServiceFactory
{
    IExceptionLogDataService CreateExceptionLogDataService(IUnitOfWork uow);
}