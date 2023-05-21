using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class WICODataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new WICOSqlDataService(uow);
}
