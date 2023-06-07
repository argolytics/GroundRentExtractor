using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class BACODataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new BACOSqlDataService(uow);
}
