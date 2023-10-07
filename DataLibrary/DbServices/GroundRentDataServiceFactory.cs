using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class GroundRentDataServiceFactory : IGroundRentDataServiceFactory
{
    public IGroundRentDataService CreateGroundRentDataService(IUnitOfWork unitOfWork) => new GroundRentSqlDataService(unitOfWork);
}
