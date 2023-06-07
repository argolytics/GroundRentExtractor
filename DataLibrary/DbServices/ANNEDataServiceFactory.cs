using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class ANNEDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new ANNESqlDataService(uow);
}
