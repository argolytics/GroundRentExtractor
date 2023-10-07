using DataLibrary.Models;

namespace DataLibrary.DbServices;

public interface IGroundRentDataService
{
    Task<bool> UpdateGroundRentProperty(GroundRentPropertyModel model);
    Task<bool> CreateOrUpdateGroundRentPdf(GroundRentPdfModel model);
    Task<List<GroundRentPropertyModel>> SelectGroundRentPropertyByCounty(string county);
    Task<List<GroundRentPropertyModel>> SelectGroundRentPropertyByCountyAndOffset(string county, int amount, int offset);
    Task<List<GroundRentPropertyModel>> SelectGroundRentPropertyByCountyAndOffsetWherePdfDownloadedIsTrue(string county, int amount, int offset);
    Task<GroundRentPdfModel?> SelectGroundRentPdf(string acknowledgementNumber);
    Task<bool> SelectBoolGroundRentPdfIfExists(string acknowledgementNumber, string accountId);
}