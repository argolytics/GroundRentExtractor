using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class STMADataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new STMASqlDataService(uow);
}
