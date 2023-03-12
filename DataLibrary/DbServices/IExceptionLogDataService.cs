using DataLibrary.Models;

namespace DataLibrary.DbServices
{
    public interface IExceptionLogDataService
    {
        Task Create(ExceptionLogModel exceptionLogModel);
        Task<bool> Delete(int id);
        Task<List<ExceptionLogModel>> ReadByAccountId(string accountId);
        Task<List<ExceptionLogModel>> ReadByCounty(string county);
        Task<List<ExceptionLogModel>> ReadByGroundRentPdfId(string groundRentPdfId);
        Task<List<ExceptionLogModel>> ReadById(int id);
        Task<List<ExceptionLogModel>> ReadTopAmount(int amount);
    }
}