using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class TALBDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new TALBSqlDataService(uow);
}
