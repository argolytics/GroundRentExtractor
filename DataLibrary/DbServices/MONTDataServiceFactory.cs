using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class MONTDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new MONTSqlDataService(uow);
}
