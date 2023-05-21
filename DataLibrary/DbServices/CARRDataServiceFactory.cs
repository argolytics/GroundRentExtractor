using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class CARRDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new CARRSqlDataService(uow);
}
