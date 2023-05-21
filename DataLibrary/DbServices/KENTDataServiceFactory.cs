using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class KENTDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new KENTSqlDataService(uow);
}
