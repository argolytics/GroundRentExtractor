using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public interface IDataServiceFactory
{
    IExtractorDataService CreateExtractorDataService(IUnitOfWork uow);
}