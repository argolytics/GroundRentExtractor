using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class FREDDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new FREDSqlDataService(uow);
}
