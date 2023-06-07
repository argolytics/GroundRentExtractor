using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class ALLEDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new ALLESqlDataService(uow);
}
