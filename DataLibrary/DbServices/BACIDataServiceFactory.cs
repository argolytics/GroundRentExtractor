using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class BACIDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new BACISqlDataService(uow);
}
