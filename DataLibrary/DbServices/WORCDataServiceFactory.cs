using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class WORCDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new WORCSqlDataService(uow);
}
