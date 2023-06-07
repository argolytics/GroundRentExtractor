using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class CALVDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new CALVSqlDataService(uow);
}
