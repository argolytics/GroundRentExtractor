using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class WORCDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new WORCSqlDataService(uow);
}
