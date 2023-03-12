using DataLibrary.Models;

namespace DataLibrary.DbServices
{
    public interface IExtractorDataService
    {
        Task<bool> CreateAddress(AddressModel addressModel);
        Task<bool> CreateGroundRentPdf(GroundRentPdfModel groundRentPdfModel);
        Task<bool> DeleteAddress(string accountId);
        Task<List<AddressModel>> ReadAddressTopAmountWhereIsGroundRentNull(int amount);
        Task<List<AddressModel>> ReadAddressTopAmountWhereIsGroundRentTrue(int amount);
        Task<bool> UpdateAddress(AddressModel addressModel);
        Task<bool> UpdateGroundRentPdf(GroundRentPdfModel groundRentPdfModel);
    }
}