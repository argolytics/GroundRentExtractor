using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class BACODataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new BACOSqlDataService(uow);
}
