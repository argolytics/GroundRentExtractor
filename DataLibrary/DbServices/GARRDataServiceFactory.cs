using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class GARRDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new GARRSqlDataService(uow);
}
