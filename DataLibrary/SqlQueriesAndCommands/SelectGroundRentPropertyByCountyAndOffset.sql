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
FROM dbo.[GroundRentProperty] WHERE [County] = @County AND ([PdfsDownloaded] IS NULL OR [PdfsDownloaded] = 0)
ORDER BY [Id] OFFSET @Offset ROWS FETCH NEXT @Amount ROWS ONLY;