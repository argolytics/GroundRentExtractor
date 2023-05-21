using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class QUEEDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new QUEESqlDataService(uow);
}
