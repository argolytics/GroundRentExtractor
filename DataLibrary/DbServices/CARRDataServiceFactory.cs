using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class CARRDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new CARRSqlDataService(uow);
}
