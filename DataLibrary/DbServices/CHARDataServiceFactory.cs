using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class CHARDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new CHARSqlDataService(uow);
}
