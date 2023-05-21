using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class GARRDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new GARRSqlDataService(uow);
}
