using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class MONTDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new MONTSqlDataService(uow);
}
