using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class PRINDataServiceFactory : IDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new PRINSqlDataService(uow);
}
