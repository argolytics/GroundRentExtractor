using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class CALVDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new CALVSqlDataService(uow);
}
