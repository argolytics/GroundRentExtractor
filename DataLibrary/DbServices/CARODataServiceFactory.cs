using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class CARODataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new CAROSqlDataService(uow);
}
