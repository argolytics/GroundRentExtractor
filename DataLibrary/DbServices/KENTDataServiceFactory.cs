using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class KENTDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new KENTSqlDataService(uow);
}
