UPDATE dbo.[GroundRentProperty] SET
[AccountId] = @AccountId, 
[County] = @County, 
[AccountNumber] = @AccountNumber, 
[Ward] = @Ward, 
[Section] = @Section, 
[Block] = @Block, 
[Lot] = @Lot, 
[LandUseCode] = @LandUseCode, 
[YearBuilt] = @YearBuilt, 
[PdfCount] = @PdfCount, 
[IsRedeemed] = @IsRedeemed, 
[PdfsDownloaded] = @PdfsDownloaded
WHERE [AccountId] = @AccountId;