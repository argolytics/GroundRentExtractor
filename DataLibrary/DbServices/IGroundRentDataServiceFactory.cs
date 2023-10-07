using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public interface IGroundRentDataServiceFactory
{
    IGroundRentDataService CreateGroundRentDataService(IUnitOfWork unitOfWork);
}