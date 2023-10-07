SELECT 
[Id],
[AddressId], 
[AccountId], 
[AcknowledgementNumber], 
[DocumentFiledType], 
[DateTimeFiled], 
[PdfPageCount], 
[Book], 
[Page], 
[ClerkInitials], 
[YearRecorded],
[PdfDownloaded]
FROM dbo.[GroundRentPdf] 
WHERE [AcknowledgementNumber] = @AcknowledgementNumber;