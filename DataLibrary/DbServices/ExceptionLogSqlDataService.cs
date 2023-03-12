using DataLibrary.Models;
using Dapper;
using System.Data;
using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class ExceptionLogSqlDataService : IExceptionLogDataService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExceptionLogSqlDataService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task Create(ExceptionLogModel exceptionLogModel)
    {
        var parms = new
        {
            exceptionLogModel.AccountId,
            exceptionLogModel.Exception
        };
        var dynParms = new DynamicParameters(parms);
        dynParms.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _unitOfWork.Connection.ExecuteAsync("spExceptionLog_Create", dynParms,
            commandType: CommandType.StoredProcedure, transaction: _unitOfWork.Transaction);
        exceptionLogModel.Id = dynParms.Get<int>("Id");
    }
    public async Task<List<ExceptionLogModel>> ReadTopAmount(int amount)
    {
        return (await _unitOfWork.Connection.QueryAsync<ExceptionLogModel>("spExceptionLog_ReadTopAmount",
            new { Amount = amount },
            commandType: CommandType.StoredProcedure, transaction: _unitOfWork.Transaction)).ToList();
    }
    public async Task<List<ExceptionLogModel>> ReadByCounty(string county)
    {
        return (await _unitOfWork.Connection.QueryAsync<ExceptionLogModel>("spExceptionLog_ReadByCounty",
            new { County = county },
            commandType: CommandType.StoredProcedure, transaction: _unitOfWork.Transaction)).ToList();
    }
    public async Task<List<ExceptionLogModel>> ReadById(int id)
    {
        return (await _unitOfWork.Connection.QueryAsync<ExceptionLogModel>("spExceptionLog_ReadById",
            new { Id = id },
            commandType: CommandType.StoredProcedure, transaction: _unitOfWork.Transaction)).ToList();
    }
    public async Task<List<ExceptionLogModel>> ReadByAccountId(string accountId)
    {
        return (await _unitOfWork.Connection.QueryAsync<ExceptionLogModel>("spExceptionLog_ReadByAccountId",
            new { AccountId = accountId },
            commandType: CommandType.StoredProcedure, transaction: _unitOfWork.Transaction)).ToList();
    }
    public async Task<List<ExceptionLogModel>> ReadByGroundRentPdfId(string groundRentPdfId)
    {
        return (await _unitOfWork.Connection.QueryAsync<ExceptionLogModel>("spExceptionLog_ReadByGroundRentPdfId",
            new { GroundRentPdfId = groundRentPdfId },
            commandType: CommandType.StoredProcedure, transaction: _unitOfWork.Transaction)).ToList();
    }
    public async Task<bool> Delete(int id)
    {
        try
        {
            await _unitOfWork.Connection.ExecuteAsync("spExceptionLog_Delete", new { Id = id },
            commandType: CommandType.StoredProcedure, transaction: _unitOfWork.Transaction);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }
}
