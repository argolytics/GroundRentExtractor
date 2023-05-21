using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class SOMEDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new SOMESqlDataService(uow);
}
