using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class WASHDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new WASHSqlDataService(uow);
}
