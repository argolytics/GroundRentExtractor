using DataLibrary.Models;
using Dapper;
using System.Data;
using DataLibrary.DbAccess;

namespace DataLibrary.DbServices;

public class BACISqlDataService : IExtractorDataService
{
    private readonly IUnitOfWork _unitOfWork;

    public BACISqlDataService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<bool> CreateAddress(AddressModel addressModel)
    {
        try
        {
            var parms = new
            {
                addressModel.AccountId,
                addressModel.County,
                addressModel.AccountNumber,
                addressModel.Ward,
                addressModel.Section,
                addressModel.Block,
                addressModel.Lot,
                addressModel.LandUseCode,
                addressModel.YearBuilt,
                addressModel.IsGroundRent,
                addressModel.IsRedeemed,
                addressModel.PdfCount,
                addressModel.AllDataDownloaded
            };
            var dynParms = new DynamicParameters(parms);
            dynParms.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
            await _unitOfWork.Connection.ExecuteAsync("spBaltimoreCity_CreateAddress", dynParms,
                commandType: CommandType.StoredProcedure, transaction: _unitOfWork.Transaction);
            addressModel.Id = dynParms.Get<int>("Id");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            return false;
        }
    }
    public async Task<bool> UpdateAddress(AddressModel addressModel)
    {
        try
        {
            var parms = new
            {
                addressModel.AccountId,
                addressModel.County,
                addressModel.AccountNumber,
                addressModel.Ward,
                addressModel.Section,
                addressModel.Block,
                addressModel.Lot,
                addressModel.LandUseCode,
                addressModel.YearBuilt,
                addressModel.IsGroundRent,
                addressModel.IsRedeemed,
                addressModel.PdfCount,
                addressModel.AllDataDownloaded
            };
            await _unitOfWork.Connection.ExecuteAsync("spBaltimoreCity_UpdateAddress", parms,
                commandType: CommandType.StoredProcedure, transaction: _unitOfWork.Transaction);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            return false;
        }
    }
    public async Task<bool> CreateGroundRentPdf(GroundRentPdfModel groundRentPdfModel)
    {
        try
        {
            var parms = new
            {
                groundRentPdfModel.AccountId,
                groundRentPdfModel.AddressId,
                groundRentPdfModel.AcknowledgementNumber,
                groundRentPdfModel.DocumentFiledType,
                groundRentPdfModel.DateTimeFiled,
                groundRentPdfModel.PdfPageCount,
                groundRentPdfModel.Book,
                groundRentPdfModel.Page,
                groundRentPdfModel.ClerkInitials,
                groundRentPdfModel.YearRecorded
            };
            var dynParms = new DynamicParameters(parms);
            dynParms.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
            await _unitOfWork.Connection.ExecuteAsync("spBaltimoreCity_CreateOrUpdateGroundRentPdf", dynParms,
                commandType: CommandType.StoredProcedure, transaction: _unitOfWork.Transaction);
            groundRentPdfModel.Id = dynParms.Get<int>("Id");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }
    public async Task<bool> UpdateGroundRentPdf(GroundRentPdfModel groundRentPdfModel)
    {
        try
        {
            var parms = new
            {
                groundRentPdfModel.AccountId,
                groundRentPdfModel.AddressId,
                groundRentPdfModel.AcknowledgementNumber,
                groundRentPdfModel.DocumentFiledType,
                groundRentPdfModel.DateTimeFiled,
                groundRentPdfModel.PdfPageCount,
                groundRentPdfModel.Book,
                groundRentPdfModel.Page,
                groundRentPdfModel.ClerkInitials,
                groundRentPdfModel.YearRecorded
            };
            await _unitOfWork.Connection.ExecuteAsync("spBaltimoreCity_UpdateGroundRentPdf", parms,
                commandType: CommandType.StoredProcedure, transaction: _unitOfWork.Transaction);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }
    public async Task<List<AddressModel>> ReadAddressTopAmountWhereIsGroundRentNull(int amount)
    {
        return (await _unitOfWork.Connection.QueryAsync<AddressModel>("spBaltimoreCity_ReadAddressTopAmountWhereIsGroundRentNull",
            new { Amount = amount },
            commandType: CommandType.StoredProcedure, transaction: _unitOfWork.Transaction)).ToList();
    }
    public async Task<List<AddressModel>> ReadAddressTopAmountWhereIsGroundRentTrue(int amount)
    {
        return (await _unitOfWork.Connection.QueryAsync<AddressModel>("spBaltimoreCity_ReadAddressTopAmountWhereIsGroundRentTrue",
            new { Amount = amount },
            commandType: CommandType.StoredProcedure, transaction: _unitOfWork.Transaction)).ToList();
    }
    public async Task<bool> DeleteAddress(string accountId)
    {
        try
        {
            await _unitOfWork.Connection.ExecuteAsync("spBaltimoreCity_DeleteAddress", new { AccountId = accountId },
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
