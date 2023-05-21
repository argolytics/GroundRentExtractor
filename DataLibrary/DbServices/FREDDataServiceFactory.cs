using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class FREDDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new FREDSqlDataService(uow);
}
