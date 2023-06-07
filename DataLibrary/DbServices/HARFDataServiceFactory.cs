using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class HARFDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new HARFSqlDataService(uow);
}
