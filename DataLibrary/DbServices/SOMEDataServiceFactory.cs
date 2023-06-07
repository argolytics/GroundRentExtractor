using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class SOMEDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new SOMESqlDataService(uow);
}
