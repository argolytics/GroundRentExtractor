using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class ALLEDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new ALLESqlDataService(uow);
}
