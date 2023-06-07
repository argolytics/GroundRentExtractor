using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class CECIDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new CECISqlDataService(uow);
}
