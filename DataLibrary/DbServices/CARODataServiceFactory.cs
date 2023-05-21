using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class CARODataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new CAROSqlDataService(uow);
}
