using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class WASHDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new WASHSqlDataService(uow);
}
