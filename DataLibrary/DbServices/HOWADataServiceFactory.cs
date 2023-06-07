using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class HOWADataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new HOWASqlDataService(uow);
}
