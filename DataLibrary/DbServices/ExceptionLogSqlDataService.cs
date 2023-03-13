using DataLibrary.Models;
using Dapper;
using System.Data;
using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class ExceptionLogSqlDataService : IExceptionLogDataService
{
    private readonly IUnitOfWork _unitOfWork;
    private const string _CreateExceptionLogSql = @"INSERT INTO dbo.[ExceptionLog](
                                                        [AccountId],
                                                        [Exception])
                                                    VALUES(
                                                        @AccountId,
                                                        @Exception);

                                                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

    private const string _DeleteExceptionLogSql = @"DELETE FROM dbo.[ExceptionLog] WHERE Id = @Id;";
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
        exceptionLogModel.Id = (int)(await _unitOfWork.Connection.ExecuteScalarAsync(_CreateExceptionLogSql, parms, transaction: _unitOfWork.Transaction));
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
            await _unitOfWork.Connection.ExecuteAsync(_DeleteExceptionLogSql, new { Id = id }, transaction: _unitOfWork.Transaction);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }
}
