using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class TALBDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new TALBSqlDataService(uow);
}
