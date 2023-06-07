using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class QUEEDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new QUEESqlDataService(uow);
}
