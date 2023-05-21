using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class HARFDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new HARFSqlDataService(uow);
}
