using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class PRINDataServiceFactory
{
    public IExtractorDataService CreateExtractorDataService(IUnitOfWork uow) => new PRINSqlDataService(uow);
}
