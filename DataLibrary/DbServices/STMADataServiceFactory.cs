using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class STMADataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new STMASqlDataService(uow);
}
