using DataLibrary.Models;
using DataLibrary.DbAccess;
using Dapper;

namespace DataLibrary.DbServices;

public class DORCSqlDataService : IExtractorDataService
{
    private readonly IUnitOfWork _unitOfWork;
    private const string _CreateAddressSql = @"INSERT INTO dbo.[DORC] ([AccountId], [County], [AccountNumber], [Ward], [Section], [Block], [Lot], [LandUseCode], [YearBuilt], [IsGroundRent], [IsRedeemed], [PdfCount], [AllDataDownloaded]) VALUES (@AccountId, @County, @AccountNumber, @Ward, @Section, @Block, @Lot, @LandUseCode, @YearBuilt, @IsGroundRent, @IsRedeemed, @PdfCount, @AllDataDownloaded); SELECT CAST(SCOPE_IDENTITY() AS INT);";
    private const string _UpdateAddressSql = @"UPDATE dbo.[DORC] SET [AccountId] = @AccountId, [County] = @County, [AccountNumber] = @AccountNumber, [Ward] = @Ward, [Section] = @Section, [Block] = @Block, [Lot] = @Lot, [LandUseCode] = @LandUseCode, [YearBuilt] = @YearBuilt, [IsGroundRent] = @IsGroundRent, [IsRedeemed] = @IsRedeemed, [PdfCount] = @PdfCount, [AllDataDownloaded] = @AllDataDownloaded WHERE [AccountId] = @AccountId";
    private const string _DeleteAddressSql = @"DELETE FROM dbo.[DORC] WHERE AccountId = @AccountId;";
    private const string _SelectAddressGroundRentNull = @"SELECT TOP (@Amount) [Id], [AccountId], [County], [AccountNumber], [Ward], [Section], [Block], [Lot], [LandUseCode], [YearBuilt], [IsGroundRent], [IsRedeemed], [PdfCount], [AllDataDownloaded] FROM dbo.[DORC] where [IsGroundRent] is null";
    private const string _SelectAddressGroundRentTrue = @"SELECT TOP (@Amount) [Id], [AccountId], [County], [AccountNumber], [Ward], [Section], [Block], [Lot], [LandUseCode], [YearBuilt], [IsGroundRent], [IsRedeemed], [PdfCount], [AllDataDownloaded] FROM dbo.[DORC] where [IsGroundRent] = 1";
    private const string _CreateGroundRentPdfSql = @"INSERT INTO dbo.[DORCGroundRentPdf] ([AddressId], [AccountId], [DocumentFiledType], [AcknowledgementNumber], [DateTimeFiled], [PdfPageCount], [Book], [Page], [ClerkInitials], [YearRecorded]) VALUES (@AddressId, @AccountId, @DocumentFiledType, @AcknowledgementNumber, @DateTimeFiled, @PdfPageCount, @Book, @Page, @ClerkInitials, @YearRecorded); SELECT CAST(SCOPE_IDENTITY() AS INT);";
    private const string _UpdateGroundRentPdfSql = @"UPDATE dbo.[DORCGroundRentPdf] SET [AccountId] = @AccountId, [AddressId] = @AddressId, [DocumentFiledType] = @DocumentFiledType, [AcknowledgementNumber] = @AcknowledgementNumber, [DateTimeFiled] = @DateTimeFiled, [PdfPageCount] = @PdfPageCount, [Book] = @Book, [Page] = @Page, [ClerkInitials] = @ClerkInitials, [YearRecorded] = @YearRecorded WHERE [AccountId] = @AccountId AND [DateTimeFiled] = @DateTimeFiled";
    public DORCSqlDataService(IUnitOfWork unitOfWork)
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

            addressModel.Id = (int)(await _unitOfWork.Connection.ExecuteScalarAsync(_CreateAddressSql, parms, transaction: _unitOfWork.Transaction));

            return true;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error($"{ex.Message}");
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
            await _unitOfWork.Connection.ExecuteAsync(_UpdateAddressSql, parms, transaction: _unitOfWork.Transaction);
            return true;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error($"{ex.Message}");
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
            groundRentPdfModel.Id = (int)(await _unitOfWork.Connection.ExecuteScalarAsync(_CreateGroundRentPdfSql, parms, transaction: _unitOfWork.Transaction));
            return true;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error($"{ex.Message}");
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
            await _unitOfWork.Connection.ExecuteAsync(_UpdateGroundRentPdfSql, parms, transaction: _unitOfWork.Transaction);
            return true;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error($"{ex.Message}");
            return false;
        }
    }
    public async Task<List<AddressModel>> ReadAddressTopAmountWhereIsGroundRentNull(int amount)
    {
        return (await _unitOfWork.Connection.QueryAsync<AddressModel>(_SelectAddressGroundRentNull,
            new { Amount = amount }, transaction: _unitOfWork.Transaction)).ToList();
    }
    public async Task<List<AddressModel>> ReadAddressTopAmountWhereIsGroundRentTrue(int amount)
    {
        return (await _unitOfWork.Connection.QueryAsync<AddressModel>(_SelectAddressGroundRentTrue,
            new { Amount = amount }, transaction: _unitOfWork.Transaction)).ToList();
    }
    public async Task<bool> DeleteAddress(string accountId)
    {
        try
        {
            await _unitOfWork.Connection.ExecuteAsync(_DeleteAddressSql, new { AccountId = accountId }, transaction: _unitOfWork.Transaction);
            return true;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error($"{ex.Message}");
            return false;
        }
    }
}
