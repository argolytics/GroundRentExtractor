using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class CHARDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new CHARSqlDataService(uow);
}
