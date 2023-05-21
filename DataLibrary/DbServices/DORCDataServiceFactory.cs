using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class DORCDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new DORCSqlDataService(uow);
}
