using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class CECIDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new CECISqlDataService(uow);
}
