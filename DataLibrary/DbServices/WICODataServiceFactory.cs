using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class WICODataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new WICOSqlDataService(uow);
}
