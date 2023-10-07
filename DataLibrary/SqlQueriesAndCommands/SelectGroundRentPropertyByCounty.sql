SELECT 
[Id], 
[AccountId], 
[County], 
[AccountNumber], 
[Ward], 
[Section], 
[Block], 
[Lot], 
[LandUseCode], 
[YearBuilt], 
[PdfCount], 
[IsRedeemed], 
[PdfsDownloaded] 
FROM dbo.[GroundRentProperty] WHERE [County] = @County AND [PdfsDownloaded] IS NULL;