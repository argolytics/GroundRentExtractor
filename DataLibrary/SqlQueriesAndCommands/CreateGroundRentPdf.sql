INSERT INTO dbo.[GroundRentPdf] 
([AddressId], 
[AccountId], 
[AcknowledgementNumber], 
[DocumentFiledType], 
[DateTimeFiled], 
[PdfPageCount], 
[Book], 
[Page], 
[ClerkInitials], 
[YearRecorded],
[PdfDownloaded],
[AzureFormRecognizerCategory],
[GroundRentPaymentAmount],
[GroundLeaseCreatedPriorTo1884],
[GroundLeaseCreatedBetween1884And1888],
[GroundLeaseCreatedBetween1888And1982],
[GroundLeaseCreated1982OrLater],
[PaymentFrequencyAnnually],
[PaymentFrequencySemiAnnually],
[PaymentFrequencyQuarterly],
[PaymentFrequencyOther],
[PaymentFrequencyAnnuallyDate],
[PaymentFrequencySemiAnnuallyDate],
[PaymentFrequencyQuarterlyDate],
[PaymentFrequencyOtherDate],
[FormPreparedDate],
[TenantName],
[TenantStreet],
[TenantCity],
[TenantState],
[TenantZipCode],
[Holder1Name],
[Holder1Street],
[Holder1City],
[Holder1State],
[Holder1ZipCode],
[Holder2Name],
[Holder2Street],
[Holder2City],
[Holder2State],
[Holder2ZipCode],
[Payable1Name],
[Payable2Name],
[PayableStreet],
[PayableCity],
[PayableState],
[PayableZipCode])
VALUES
(@AddressId, 
@AccountId, 
@AcknowledgementNumber, 
@DocumentFiledType, 
@DateTimeFiled, 
@PdfPageCount, 
@Book, 
@Page, 
@ClerkInitials, 
@YearRecorded,
@PdfDownloaded,
@AzureFormRecognizerCategory,
@GroundRentPaymentAmount,
@GroundLeaseCreatedPriorTo1884,
@GroundLeaseCreatedBetween1884And1888,
@GroundLeaseCreatedBetween1888And1982,
@GroundLeaseCreated1982OrLater,
@PaymentFrequencyAnnually,
@PaymentFrequencySemiAnnually,
@PaymentFrequencyQuarterly,
@PaymentFrequencyOther,
@PaymentFrequencyAnnuallyDate,
@PaymentFrequencySemiAnnuallyDate,
@PaymentFrequencyQuarterlyDate,
@PaymentFrequencyOtherDate,
@FormPreparedDate,
@TenantName,
@TenantStreet,
@TenantCity,
@TenantState,
@TenantZipCode,
@Holder1Name,
@Holder1Street,
@Holder1City,
@Holder1State,
@Holder1ZipCode,
@Holder2Name,
@Holder2Street,
@Holder2City,
@Holder2State,
@Holder2ZipCode,
@Payable1Name,
@Payable2Name,
@PayableStreet,
@PayableCity,
@PayableState,
@PayableZipCode);
SELECT CAST(SCOPE_IDENTITY() AS INT);