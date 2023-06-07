using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class BACIDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new BACISqlDataService(uow);
}
