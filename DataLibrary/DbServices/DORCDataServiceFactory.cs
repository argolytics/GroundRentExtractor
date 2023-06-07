using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class DORCDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new DORCSqlDataService(uow);
}
