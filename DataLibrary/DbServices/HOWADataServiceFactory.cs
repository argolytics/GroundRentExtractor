using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class HOWADataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new HOWASqlDataService(uow);
}
