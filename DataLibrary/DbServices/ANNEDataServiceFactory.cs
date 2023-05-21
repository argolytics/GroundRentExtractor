using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class ANNEDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new ANNESqlDataService(uow);
}
