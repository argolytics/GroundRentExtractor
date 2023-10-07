namespace DataLibrary.Models;

public class GroundRentPdfModel
{
    public int Id { get; set; }
    public int AddressId { get; set; }
    public string? AccountId { get; set; }
    public string? AcknowledgementNumber { get; set; }
    public string? DocumentFiledType { get; set; }
    public DateTime? DateTimeFiled { get; set; }
    public string? PdfPageCount { get; set; }
    public string? Book { get; set; }
    public string? Page { get; set; }
    public string? ClerkInitials { get; set; }
    public int? YearRecorded { get; set; }
    public bool? PdfDownloaded { get; set; }
    public string? AzureFormRecognizerCategory { get; set; }
    public string? GroundRentPaymentAmount { get; set; }
    public bool? GroundLeaseCreatedPriorTo1884 { get; set; }
    public bool? GroundLeaseCreatedBetween1884And1888 { get; set; }
    public bool? GroundLeaseCreatedBetween1888And1982 { get; set; }
    public bool? GroundLeaseCreated1982OrLater { get; set; }
    public bool? PaymentFrequencyAnnually { get; set; }
    public bool? PaymentFrequencySemiAnnually { get; set; }
    public bool? PaymentFrequencyQuarterly { get; set; }
    public bool? PaymentFrequencyOther { get; set; }
    public string? PaymentFrequencyAnnuallyDate { get; set; }
    public string? PaymentFrequencySemiAnnuallyDate { get; set; }
    public string? PaymentFrequencyQuarterlyDate { get; set; }
    public string? PaymentFrequencyOtherDate { get; set; }
    public string? FormPreparedDate { get; set; }
    public string? TenantName { get; set; }
    public string? TenantStreet { get; set; }
    public string? TenantCity { get; set; }
    public string? TenantState { get; set; }
    public string? TenantZipCode { get; set; }
    public string? Holder1Name { get; set; }
    public string? Holder1Street { get; set; }
    public string? Holder1City { get; set; }
    public string? Holder1State { get; set; }
    public string? Holder1ZipCode { get; set; }
    public string? Holder2Name { get; set; }
    public string? Holder2Street { get; set; }
    public string? Holder2City { get; set; }
    public string? Holder2State { get; set; }
    public string? Holder2ZipCode { get; set; }
    public string? Payable1Name { get; set; }
    public string? Payable2Name { get; set; }
    public string? PayableStreet { get; set; }
    public string? PayableCity { get; set; }
    public string? PayableState { get; set; }
    public string? PayableZipCode { get; set; }
}
