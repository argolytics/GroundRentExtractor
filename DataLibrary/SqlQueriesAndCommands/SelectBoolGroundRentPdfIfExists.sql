SELECT CAST (CASE WHEN EXISTS 
(SELECT * FROM dbo.[GroundRentPdf] 
WHERE [AcknowledgementNumber] = @AcknowledgementNumber AND [AccountId] = @AccountId)
THEN 1 ELSE 0 END AS BIT);