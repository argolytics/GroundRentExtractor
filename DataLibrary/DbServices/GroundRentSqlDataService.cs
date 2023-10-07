using DataLibrary.Models;
using DataLibrary.DbAccess;
using Dapper;

namespace DataLibrary.DbServices;

public class GroundRentSqlDataService : IGroundRentDataService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _UpdateGroundRentProperty;
    private readonly string _CreateOrUpdateGroundRentPdf;
    private readonly string _SelectGroundRentPropertyByCounty;
    private readonly string _SelectGroundRentPropertyByCountyAndOffset;
    private readonly string _SelectGroundRentPdf;
    private readonly string _SelectGroundRentPropertyByCountyAndOffsetWherePdfDownloadedIsTrue;
    private readonly string _SelectBoolGroundRentPdfIfExists;

    public GroundRentSqlDataService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _UpdateGroundRentProperty = File.ReadAllText(@"C:\Users\Jason\source\repos\GroundRentExtractor\DataLibrary\SqlQueriesAndCommands\UpdateGroundRentProperty.sql");
        _CreateOrUpdateGroundRentPdf = File.ReadAllText(@"C:\Users\Jason\source\repos\GroundRentExtractor\DataLibrary\SqlQueriesAndCommands\CreateOrUpdateGroundRentPdf.sql");
        _SelectGroundRentPropertyByCounty = File.ReadAllText(@"C:\Users\Jason\source\repos\GroundRentExtractor\DataLibrary\SqlQueriesAndCommands\SelectGroundRentPropertyByCounty.sql");
        _SelectGroundRentPropertyByCountyAndOffset = File.ReadAllText(@"C:\Users\Jason\source\repos\GroundRentExtractor\DataLibrary\SqlQueriesAndCommands\SelectGroundRentPropertyByCountyAndOffset.sql");
        _SelectGroundRentPropertyByCountyAndOffsetWherePdfDownloadedIsTrue = File.ReadAllText(@"C:\Users\Jason\source\repos\GroundRentExtractor\DataLibrary\SqlQueriesAndCommands\SelectGroundRentPropertyByCountyAndOffsetWherePdfDownloadedIsTrue.sql");
        _SelectGroundRentPdf = File.ReadAllText(@"C:\Users\Jason\source\repos\GroundRentExtractor\DataLibrary\SqlQueriesAndCommands\SelectGroundRentPdf.sql");
        _SelectBoolGroundRentPdfIfExists = File.ReadAllText(@"C:\Users\Jason\source\repos\GroundRentExtractor\DataLibrary\SqlQueriesAndCommands\SelectBoolGroundRentPdfIfExists.sql");
    }
    public async Task<bool> UpdateGroundRentProperty(GroundRentPropertyModel model)
    {
        try
        {
            var parms = new
            {
                model.AccountId,
                model.County,
                model.AccountNumber,
                model.Ward,
                model.Section,
                model.Block,
                model.Lot,
                model.LandUseCode,
                model.YearBuilt,
                model.PdfCount,
                model.IsRedeemed,
                model.PdfsDownloaded
            };
            await _unitOfWork.Connection.ExecuteAsync(_UpdateGroundRentProperty, parms, transaction: _unitOfWork.Transaction);
            return true;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error($"{ ex.Message}");
            return false;
        }
    }
    public async Task<bool> CreateOrUpdateGroundRentPdf(GroundRentPdfModel model)
    {
        try
        {
            var parms = new
            {
                model.AddressId,
                model.AccountId,
                model.AcknowledgementNumber,
                model.DocumentFiledType,
                model.DateTimeFiled,
                model.PdfPageCount,
                model.Book,
                model.Page,
                model.ClerkInitials,
                model.YearRecorded,
                model.PdfDownloaded,
                model.AzureFormRecognizerCategory,
                model.GroundRentPaymentAmount,
                model.GroundLeaseCreatedPriorTo1884,
                model.GroundLeaseCreatedBetween1884And1888,
                model.GroundLeaseCreatedBetween1888And1982,
                model.GroundLeaseCreated1982OrLater,
                model.PaymentFrequencyAnnually,
                model.PaymentFrequencySemiAnnually,
                model.PaymentFrequencyQuarterly,
                model.PaymentFrequencyOther,
                model.PaymentFrequencyAnnuallyDate,
                model.PaymentFrequencySemiAnnuallyDate,
                model.PaymentFrequencyQuarterlyDate,
                model.PaymentFrequencyOtherDate,
                model.FormPreparedDate,
                model.TenantName,
                model.TenantStreet,
                model.TenantCity,
                model.TenantState,
                model.TenantZipCode,
                model.Holder1Name,
                model.Holder1Street,
                model.Holder1City,
                model.Holder1State,
                model.Holder1ZipCode,
                model.Holder2Name,
                model.Holder2Street,
                model.Holder2City,
                model.Holder2State,
                model.Holder2ZipCode,
                model.Payable1Name,
                model.Payable2Name,
                model.PayableStreet,
                model.PayableCity,
                model.PayableState,
                model.PayableZipCode
            };
            model.Id = (int)await _unitOfWork.Connection.ExecuteScalarAsync(_CreateOrUpdateGroundRentPdf, parms, transaction: _unitOfWork.Transaction);
            return true;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error($"{ex.Message}");
            return false;
        }
    }
    public async Task<List<GroundRentPropertyModel>> SelectGroundRentPropertyByCounty(string county)
    {
        return (await _unitOfWork.Connection.QueryAsync<GroundRentPropertyModel>(_SelectGroundRentPropertyByCounty,
            new { County = county }, transaction: _unitOfWork.Transaction)).ToList();
    }
    public async Task<List<GroundRentPropertyModel>> SelectGroundRentPropertyByCountyAndOffset(string county, int amount, int offset)
    {
        return (await _unitOfWork.Connection.QueryAsync<GroundRentPropertyModel>(_SelectGroundRentPropertyByCountyAndOffset,
            new { County = county, Amount = amount, Offset = offset }, transaction: _unitOfWork.Transaction)).ToList();
    }
    public async Task<List<GroundRentPropertyModel>> SelectGroundRentPropertyByCountyAndOffsetWherePdfDownloadedIsTrue(string county, int amount, int offset)
    {
        return (await _unitOfWork.Connection.QueryAsync<GroundRentPropertyModel>(_SelectGroundRentPropertyByCountyAndOffsetWherePdfDownloadedIsTrue,
            new { County = county, Amount = amount, Offset = offset }, transaction: _unitOfWork.Transaction)).ToList();
    }
    public async Task<GroundRentPdfModel?> SelectGroundRentPdf(string acknowledgementNumber)
    {
        return (await _unitOfWork.Connection.QueryAsync<GroundRentPdfModel?>(_SelectGroundRentPdf,
            new { AcknowledgementNumber = acknowledgementNumber }, transaction: _unitOfWork.Transaction)).FirstOrDefault();
    }
    public async Task<bool> SelectBoolGroundRentPdfIfExists(string acknowledgementNumber, string accountId)
    {
        return (await _unitOfWork.Connection.QueryAsync<bool>(_SelectBoolGroundRentPdfIfExists,
            new { AcknowledgementNumber = acknowledgementNumber, AccountId = accountId }, transaction: _unitOfWork.Transaction)).FirstOrDefault();
    }
}
