using Microsoft.Extensions.Options;
using DataLibrary.DbAccess;
using DataLibrary.DbServices;
using DataLibrary.Models;
using DataLibrary.Settings;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using DataLibrary.Services;

namespace DataLibrary.Extractors;

public class CHARExtractor
{
    private readonly IDataContext _dataContext;
    private readonly CHARDataServiceFactory _charDataServiceFactory;
    private readonly ExceptionLogDataServiceFactory _exceptionLogDataServiceFactory;
    private readonly BlobService _blobService;
    private readonly IOptionsMonitor<BlobSettings> _blobSettings;
    private readonly IOptionsMonitor<DriverPathSettings> _pathSettings;
    private readonly IOptionsMonitor<WebPageStringSettings> _webPageStringSettings;
    FirefoxDriver FirefoxDriver;
    WebDriverWait WebDriverWait;
    private IWebElement Input { get; set; }
    private enum ExceptionLog
    {
        TransactionFailAddressNotGroundRent,
        TransactionFailCouldNotStoreAddress,
        TransactionFailCouldNotStoreMetadata,
        TransactionFailCouldNotStorePdf,
        TransactionFailCouldNotDeleteAddress,
        DateTimeFiledIsNull
    }
    private readonly string County = "CHAR";
    private List<AddressModel> AddressList = new();
    private bool? dbTransactionResultBool = null;
    private bool? allDataDownloadedBool = true;
    private bool? objectDisposedExceptionBool = null;
    private bool? noSuchWindowExceptionBool = null;
    private int exceptionCount = 0;
    private int currentCount = 0;
    private int accumulatedCount = 0;
    private int totalCount = 0;
    private decimal elapsedTime = 0;
    private decimal accumulatedElapsedTime = 0;

    public CHARExtractor(
        IDataContext dataContext,
        CHARDataServiceFactory charDataServiceFactory,
        ExceptionLogDataServiceFactory exceptionLogDataServiceFactory,
        BlobService blobService,
        IOptionsMonitor<BlobSettings> blobSettings,
        IOptionsMonitor<DriverPathSettings> pathSettings,
        IOptionsMonitor<WebPageStringSettings> webPageStringSettings)
    {
        _dataContext = dataContext;
        _charDataServiceFactory = charDataServiceFactory;
        _exceptionLogDataServiceFactory = exceptionLogDataServiceFactory;
        _blobService = blobService;
        _blobSettings = blobSettings;
        _pathSettings = pathSettings;
        _webPageStringSettings = webPageStringSettings;
    }

    private void SetupDriver(IOptionsMonitor<DriverPathSettings> pathSettings)
    {
        try
        {
            if (!string.IsNullOrEmpty(pathSettings.CurrentValue.FirefoxCHARProfilePath) && !string.IsNullOrEmpty(pathSettings.CurrentValue.GeckoDriverPath))
            {
                FirefoxProfile firefoxProfile = new(pathSettings.CurrentValue.FirefoxCHARProfilePath);
                FirefoxOptions FirefoxOptions = new()
                {
                    Profile = firefoxProfile,
                };
                //firefoxOptions.AddArguments("--headless");
                FirefoxDriver = new FirefoxDriver(pathSettings.CurrentValue.GeckoDriverPath, FirefoxOptions, TimeSpan.FromSeconds(60));
                WebDriverWait = new(FirefoxDriver, TimeSpan.FromSeconds(10));
                WebDriverWait.IgnoreExceptionTypes(
                    typeof(NoSuchElementException),
                    typeof(StaleElementReferenceException),
                    typeof(ElementNotSelectableException),
                    typeof(ElementNotVisibleException));

            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Error($"{County}: " + ex.ToString());
        }
    }

    public Task Extract(int amountToExtract)
    {
        return Extract(amountToExtract, new CancellationTokenSource().Token);
    }
    public async Task Extract(int remainingCount, CancellationToken cancellationToken)
    {
        if (FirefoxDriver == null && WebDriverWait == null)
        {
            SetupDriver(_pathSettings);
        }
        // Define baseUrlWindow
        var baseUrlWindow = FirefoxDriver.CurrentWindowHandle;
        // Read and populate address list
        using (var uow = _dataContext.CreateUnitOfWork())
        {
            var charDataService = _charDataServiceFactory.CreateExtractorDataService(uow);
            AddressList = await charDataService.ReadAddressTopAmountWhereIsGroundRentNull(remainingCount);
        }
        if (AddressList.Count == 0)
        {
            foreach (string window in FirefoxDriver.WindowHandles) FirefoxDriver.Close();
            FirefoxDriver.Quit();
            Serilog.Log.Information($"{County} complete.");
        }
        currentCount = 0;
        totalCount = remainingCount + accumulatedCount;
        var stopWatch = ValueStopwatch.StartNew();
        try
        {
            var iterList = AddressList.ToList();
            foreach (var iterAddress in iterList)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                // Selecting appropriate county
                FirefoxDriver.Navigate().GoToUrl(_webPageStringSettings.CurrentValue.BaseUrl);
                Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(_webPageStringSettings.CurrentValue.CHARDropDownSelect)));
                Input.Click();
                // Selecting "PROPERTY ACCOUNT IDENTIFIER"
                Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(_webPageStringSettings.CurrentValue.PropertyAccountIdentifierSelect)));
                Input.Click();
                // Click Continue button
                Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(_webPageStringSettings.CurrentValue.ContinueClick)));
                Input.Click();
                // Input District and AccountNumber
                Input = WebDriverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(_webPageStringSettings.CurrentValue.DistrictInput)));
                Input.Clear();
                Input.SendKeys(iterAddress.Ward);
                Input = WebDriverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(_webPageStringSettings.CurrentValue.AccountNumberInput)));
                Input.Clear();
                Input.SendKeys(iterAddress.AccountNumber);
                // Click Next button
                Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(_webPageStringSettings.CurrentValue.NextClick)));
                Input.Click();
                // Check for any error labels
                if (FirefoxDriver.FindElements(By.CssSelector(_webPageStringSettings.CurrentValue.GenericErrorTag)).Count != 0)
                {
                    // If address does not exist in SDAT
                    if (FirefoxDriver.FindElement(By.CssSelector(_webPageStringSettings.CurrentValue.GenericErrorTag))
                        .Text.Contains(_webPageStringSettings.CurrentValue.NoRecordsMessage))
                    {
                        using (var uow = _dataContext.CreateUnitOfWork())
                        {
                            var charDataService = _charDataServiceFactory.CreateExtractorDataService(uow);
                            dbTransactionResultBool = await charDataService.DeleteAddress(iterAddress.AccountId);
                        }
                        currentCount++;
                        AddressList.Remove(iterAddress);
                    }
                    // There must be an error tag that is different from the normal "no records exist" message
                    else
                    {
                        await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailCouldNotDeleteAddress.ToString());
                        Serilog.Log.Error($"{County} accountId {iterAddress.AccountId}: " + ExceptionLog.TransactionFailCouldNotDeleteAddress.ToString());
                    }
                    currentCount++;
                    AddressList.Remove(iterAddress);
                }
                // There are no error tags so we are free to check if address is fee simple or ground rent
                else
                {
                    // Click Ground Rent Registration link
                    WebDriverWait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(_webPageStringSettings.CurrentValue.InvisibilityCatch)));
                    Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(_webPageStringSettings.CurrentValue.GroundRentRegistrationLinkClick)));
                    Input.Click();
                    // Check if html has ground rent error tag (meaning property has no ground rent registered)
                    if (FirefoxDriver.FindElements(By.CssSelector(_webPageStringSettings.CurrentValue.GroundRentErrorTag)).Count != 0)
                    {
                        if (FirefoxDriver.FindElement(By.CssSelector(_webPageStringSettings.CurrentValue.GroundRentErrorTag))
                            .Text.Contains(_webPageStringSettings.CurrentValue.NoGroundRentMessage))
                        {
                            // Property is fee simple (not ground rent)
                            using (var uow = _dataContext.CreateUnitOfWork())
                            {
                                var charDataService = _charDataServiceFactory.CreateExtractorDataService(uow);
                                dbTransactionResultBool = await charDataService.UpdateAddress(new AddressModel()
                                {
                                    AccountId = iterAddress.AccountId,
                                    County = iterAddress.County,
                                    AccountNumber = iterAddress.AccountNumber,
                                    Ward = iterAddress.Ward,
                                    Section = iterAddress.Section,
                                    Block = iterAddress.Block,
                                    Lot = iterAddress.Lot,
                                    LandUseCode = iterAddress.LandUseCode,
                                    YearBuilt = iterAddress.YearBuilt,
                                    IsGroundRent = false,
                                    IsRedeemed = null,
                                    PdfCount = null,
                                    AllDataDownloaded = null
                                });
                            }
                            if (dbTransactionResultBool is false)
                            {
                                await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailAddressNotGroundRent.ToString());
                                Serilog.Log.Error($"{County} accountId {iterAddress.AccountId}: " + ExceptionLog.TransactionFailAddressNotGroundRent.ToString());
                            }
                        }
                        currentCount++;
                        AddressList.Remove(iterAddress);
                    }
                    // No ground rent error tag, therefore must be ground rent
                    else
                    {
                        iterAddress.IsGroundRent = true;
                        // Load ground rent metadata table
                        WebDriverWait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.XPath(@_webPageStringSettings.CurrentValue.GroundRentMetadataTable)));
                        // Initialize variables keeping track of the total and current metadata collection count
                        var metadataCollectionCurrentCount = 0;
                        var pdfDownloadCount = 0;
                        var metadataCollectionTotalCount = FirefoxDriver.FindElements(By.XPath($@"{_webPageStringSettings.CurrentValue.GroundRentMetadataTable}/tbody/tr")).Count;
                        List<GroundRentPdfModel> groundRentPdfModelList;
                        for (metadataCollectionCurrentCount = 0; metadataCollectionCurrentCount < metadataCollectionTotalCount;)
                        {
                            if (metadataCollectionCurrentCount == 0 ||
                                metadataCollectionCurrentCount == metadataCollectionTotalCount - 1)
                            {
                                metadataCollectionCurrentCount++;
                                continue;
                            }
                            {
                                // Per iteration, reinitialize the metadata collection to avoid stale element exception
                                WebDriverWait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.XPath(@_webPageStringSettings.CurrentValue.GroundRentMetadataTable)));
                                IReadOnlyCollection<IWebElement> metadataCollection = FirefoxDriver.FindElements(By.XPath($@"{_webPageStringSettings.CurrentValue.GroundRentMetadataTable}/tbody/tr"));
                                // In case the previous groundRentModel (and corresponding PDF) didn't process correctly, we empty the list again
                                groundRentPdfModelList = new();
                                // Then add the model and initialize its various properties
                                GroundRentPdfModel groundRentPdfModel = new();
                                groundRentPdfModelList.Add(groundRentPdfModel);
                                groundRentPdfModel.AddressId = iterAddress.Id;
                                groundRentPdfModel.AccountId = iterAddress.AccountId;
                                string? dateTimeFiledString = metadataCollection.ElementAt(metadataCollectionCurrentCount).FindElement(By.CssSelector($"{_webPageStringSettings.CurrentValue.DateTimeFiledTag}_{metadataCollectionCurrentCount - 1}")).Text ?? null;
                                // DateTimeFiled cannot be null since it is used as the sql table's Primary Key
                                // so we check for null here. If null, we skip metadata and pdf download but make note in log
                                if (dateTimeFiledString is null)
                                {
                                    await LogException(iterAddress.AccountId, ExceptionLog.DateTimeFiledIsNull.ToString());
                                    Serilog.Log.Error($"{County} accountId {iterAddress.AccountId}: " + ExceptionLog.DateTimeFiledIsNull.ToString());
                                    allDataDownloadedBool = false;
                                    metadataCollectionCurrentCount++;
                                    continue;
                                }
                                // Ensure there are always two digits for the month for a given date
                                // so that DateTime.TryParse can parse dateTimeFiledString
                                if (dateTimeFiledString[0] != '1')
                                {
                                    if (!dateTimeFiledString.StartsWith("1/"))
                                    {
                                        dateTimeFiledString = '0' + dateTimeFiledString;
                                    }
                                }
                                DateTime? DateTimeFiled = DateTime.TryParse(dateTimeFiledString, out DateTime tempDate) ? tempDate : null;
                                groundRentPdfModel.DateTimeFiled = DateTimeFiled;
                                groundRentPdfModel.DocumentFiledType = metadataCollection.ElementAt(metadataCollectionCurrentCount).FindElement(By.CssSelector($"{_webPageStringSettings.CurrentValue.DocumentFiledTypeTag}_{metadataCollectionCurrentCount - 1}")).Text ?? null;
                                groundRentPdfModel.AcknowledgementNumber = metadataCollection.ElementAt(metadataCollectionCurrentCount).FindElement(By.CssSelector($"{_webPageStringSettings.CurrentValue.AcknowledgementNumberTag}_{metadataCollectionCurrentCount - 1}")).Text ?? null;
                                groundRentPdfModel.PdfPageCount = metadataCollection.ElementAt(metadataCollectionCurrentCount).FindElement(By.CssSelector($"{_webPageStringSettings.CurrentValue.PdfPageCountTag}_{metadataCollectionCurrentCount - 1}")).Text ?? null;
                                string? deedReferenceData = metadataCollection.ElementAt(metadataCollectionCurrentCount).FindElement(By.CssSelector($"{_webPageStringSettings.CurrentValue.DeedReferenceTag}_{metadataCollectionCurrentCount - 1}")).Text ?? null;
                                var deedReferenceDataArray = deedReferenceData.Split('/') ?? null;
                                groundRentPdfModel.Book = deedReferenceDataArray[0] ?? null;
                                groundRentPdfModel.Page = deedReferenceDataArray[1] ?? null;
                                groundRentPdfModel.ClerkInitials = deedReferenceDataArray[2] ?? null;
                                int.TryParse(deedReferenceDataArray[3], out var yearRecordedResult);
                                groundRentPdfModel.YearRecorded = yearRecordedResult;
                                using (var uow = _dataContext.CreateUnitOfWork())
                                {
                                    var charDataService = _charDataServiceFactory.CreateExtractorDataService(uow);
                                    dbTransactionResultBool = await charDataService.CreateGroundRentPdf(new GroundRentPdfModel()
                                    {
                                        AccountId = groundRentPdfModel.AccountId,
                                        AddressId = groundRentPdfModel.AddressId,
                                        AcknowledgementNumber = groundRentPdfModel.AcknowledgementNumber,
                                        DocumentFiledType = groundRentPdfModel.DocumentFiledType,
                                        DateTimeFiled = groundRentPdfModel.DateTimeFiled,
                                        PdfPageCount = groundRentPdfModel.PdfPageCount,
                                        Book = groundRentPdfModel.Book,
                                        Page = groundRentPdfModel.Page,
                                        ClerkInitials = groundRentPdfModel.ClerkInitials,
                                        YearRecorded = groundRentPdfModel.YearRecorded
                                    });
                                }
                                if (dbTransactionResultBool is false)
                                {
                                    await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailCouldNotStoreMetadata.ToString());
                                    Serilog.Log.Error($"{County} accountId {iterAddress.AccountId}: " + ExceptionLog.TransactionFailCouldNotStoreMetadata.ToString());
                                    allDataDownloadedBool = false;
                                    metadataCollectionCurrentCount++;
                                    // If we cannot store metadata, we do not want the accompanying pdf, so we continue here
                                    continue;
                                }
                                metadataCollectionCurrentCount++;
                                // Double check at the top of PDF-opening to close any previously-lingering PDF windows
                                while (FirefoxDriver.WindowHandles.Count != 1)
                                {
                                    foreach (string window in FirefoxDriver.WindowHandles)
                                    {
                                        if (baseUrlWindow != window)
                                        {
                                            FirefoxDriver.Close();
                                        }
                                    }
                                }
                                // Click and open pdf
                                Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.Id(metadataCollection.ElementAt(metadataCollectionCurrentCount - 1).FindElement(By.TagName("a")).GetAttribute("id"))));
                                Input.Click();
                                // Switch to pdf window
                                foreach (string window in FirefoxDriver.WindowHandles)
                                {
                                    if (baseUrlWindow != window)
                                    {
                                        FirefoxDriver.SwitchTo().Window(window);
                                        if (WebDriverWait.Until(FirefoxDriver => ((IJavaScriptExecutor)FirefoxDriver).ExecuteScript("return document.readyState").Equals("complete")))
                                        {
                                            // Upload pdf to blob storage
                                            PrintOptions printOptions = new();
                                            //FirefoxDriver.Print(printOptions).SaveAsFile($"{PdfSaveFilePath}{groundRentPdfModel.AccountId}_{groundRentPdfModelList.FirstOrDefault().DocumentFiledType}_{groundRentPdfModelList.FirstOrDefault().AcknowledgementNumber}.pdf");
                                            var accountIdTrimmed = groundRentPdfModel.AccountId.Trim();
                                            var printDocument = FirefoxDriver.Print(printOptions);
                                            var pdfFileName = $"{accountIdTrimmed}_{groundRentPdfModelList.FirstOrDefault().DocumentFiledType}_{groundRentPdfModelList.FirstOrDefault().AcknowledgementNumber}.pdf";
                                            if (await _blobService.UploadBlob(pdfFileName, printDocument, _blobSettings.CurrentValue.CHARContainer))
                                            {
                                                pdfDownloadCount++;
                                            }
                                            else
                                            {
                                                await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailCouldNotStorePdf.ToString());
                                                Serilog.Log.Error($"{County} accountId {iterAddress.AccountId}: " + ExceptionLog.TransactionFailCouldNotStorePdf.ToString());
                                                allDataDownloadedBool = false;
                                            }
                                            groundRentPdfModelList = new();
                                            // Close pdf window and switch back to baseUrlWindow
                                            FirefoxDriver.Close();
                                            FirefoxDriver.SwitchTo().Window(baseUrlWindow);
                                        }
                                        else
                                        {
                                            await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailCouldNotStorePdf.ToString());
                                            Serilog.Log.Error($"{County} accountId {iterAddress.AccountId}: " + ExceptionLog.TransactionFailCouldNotStorePdf.ToString());
                                            allDataDownloadedBool = false;
                                            // Close pdf window and switch back to baseUrlWindow
                                            FirefoxDriver.Close();
                                            WebDriverWait.Until(FireFoxDriver => FirefoxDriver.WindowHandles.Count == 1);
                                            FirefoxDriver.SwitchTo().Window(baseUrlWindow);
                                        }
                                    }
                                }
                            }
                        }
                        // Ensure all pdf windows are closed, then switch to baseUrlWindow
                        foreach (string window in FirefoxDriver.WindowHandles)
                        {
                            if (baseUrlWindow != window) FirefoxDriver.Close();
                        }
                        FirefoxDriver.SwitchTo().Window(baseUrlWindow);
                        // Check if all metadata and pdfs stored in db
                        if (allDataDownloadedBool is true)
                        {
                            using (var uow = _dataContext.CreateUnitOfWork())
                            {
                                var charDataService = _charDataServiceFactory.CreateExtractorDataService(uow);
                                dbTransactionResultBool = await charDataService.UpdateAddress(new AddressModel()
                                {
                                    AccountId = iterAddress.AccountId,
                                    County = iterAddress.County,
                                    AccountNumber = iterAddress.AccountNumber,
                                    Ward = iterAddress.Ward,
                                    Section = iterAddress.Section,
                                    Block = iterAddress.Block,
                                    Lot = iterAddress.Lot,
                                    LandUseCode = iterAddress.LandUseCode,
                                    YearBuilt = iterAddress.YearBuilt,
                                    IsGroundRent = true,
                                    IsRedeemed = iterAddress.IsRedeemed,
                                    PdfCount = pdfDownloadCount,
                                    AllDataDownloaded = true
                                });
                            }
                            if (dbTransactionResultBool is false)
                            {
                                await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailCouldNotStoreAddress.ToString());
                                Serilog.Log.Error($"{County} accountId {iterAddress.AccountId}: " + ExceptionLog.TransactionFailCouldNotStoreAddress.ToString());
                            }
                        }
                        // pdf download count or metadata collection current are not count equal to pdf total count
                        else
                        {
                            using (var uow = _dataContext.CreateUnitOfWork())
                            {
                                var charDataService = _charDataServiceFactory.CreateExtractorDataService(uow);
                                dbTransactionResultBool = await charDataService.UpdateAddress(new AddressModel()
                                {
                                    AccountId = iterAddress.AccountId,
                                    County = iterAddress.County,
                                    AccountNumber = iterAddress.AccountNumber,
                                    Ward = iterAddress.Ward,
                                    Section = iterAddress.Section,
                                    Block = iterAddress.Block,
                                    Lot = iterAddress.Lot,
                                    LandUseCode = iterAddress.LandUseCode,
                                    YearBuilt = iterAddress.YearBuilt,
                                    IsGroundRent = true,
                                    IsRedeemed = iterAddress.IsRedeemed,
                                    PdfCount = pdfDownloadCount,
                                    AllDataDownloaded = false
                                });
                            }
                            if (dbTransactionResultBool is false)
                            {
                                await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailCouldNotStoreAddress.ToString());
                                Serilog.Log.Error($"{County} accountId {iterAddress.AccountId}: " + ExceptionLog.TransactionFailCouldNotStoreAddress.ToString());
                            }
                        }
                        currentCount++;
                        AddressList.Remove(iterAddress);
                    }
                }
            }
            accumulatedCount += currentCount;
            accumulatedElapsedTime = CalculateElapsedTime(stopWatch);
            ReportTotals(accumulatedCount, totalCount, accumulatedElapsedTime);
        }
        catch (WebDriverTimeoutException)
        {
            Serilog.Log.Information($"{County}: Timed out after 10 seconds. Restarting.");
            accumulatedElapsedTime = CalculateElapsedTime(stopWatch);
            await AdjustCountAndRestartOrQuit(exceptionCount, accumulatedElapsedTime, cancellationToken);
        }
        catch (ElementClickInterceptedException elementClickInterceptedException)
        {
            exceptionCount++;
            var exceptionMessage = elementClickInterceptedException.Message;
            Serilog.Log.Error($"{County} exception count: {exceptionCount} / 5. {exceptionMessage}");
            accumulatedElapsedTime = CalculateElapsedTime(stopWatch);
            await AdjustCountAndRestartOrQuit(exceptionCount, accumulatedElapsedTime, cancellationToken);
        }
        catch (StaleElementReferenceException staleElementReferenceException)
        {
            exceptionCount++;
            var exceptionMessage = staleElementReferenceException.Message;
            accumulatedElapsedTime = CalculateElapsedTime(stopWatch);
            await AdjustCountAndRestartOrQuit(exceptionCount, accumulatedElapsedTime, cancellationToken);
        }
        catch (NoSuchWindowException noSuchWindowException)
        {
            var exceptionMessage = noSuchWindowException.Message;
            Serilog.Log.Error($"{County} critical exception: Do not restart {County}. {exceptionMessage}");
            noSuchWindowExceptionBool = true;
            accumulatedElapsedTime = CalculateElapsedTime(stopWatch);
            await AdjustCountAndRestartOrQuit(exceptionCount, accumulatedElapsedTime, cancellationToken);
        }
        catch (ObjectDisposedException objectDisposedException)
        {
            var exceptionMessage = objectDisposedException.Message;
            Serilog.Log.Error($"{County} critical exception: Do not restart {County}. {exceptionMessage}");
            objectDisposedExceptionBool = true;
            accumulatedElapsedTime = CalculateElapsedTime(stopWatch);
            await AdjustCountAndRestartOrQuit(exceptionCount, accumulatedElapsedTime, cancellationToken);
        }
        catch (Azure.RequestFailedException requestFailedException)
        {
            var exceptionMessage = requestFailedException.Message;
            Serilog.Log.Information($"{County}: {exceptionMessage}");
            accumulatedElapsedTime = CalculateElapsedTime(stopWatch);
            await AdjustCountAndRestartOrQuit(exceptionCount, accumulatedElapsedTime, cancellationToken);
        }
        catch (Exception e)
        {
            exceptionCount++;
            var exceptionMessage = e.Message;
            Serilog.Log.Error($"{County} exception count: {exceptionCount} / 5. {exceptionMessage}");
            accumulatedElapsedTime = CalculateElapsedTime(stopWatch);
            await AdjustCountAndRestartOrQuit(exceptionCount, accumulatedElapsedTime, cancellationToken);
        }
    }
    private decimal CalculateElapsedTime(ValueStopwatch stopWatch)
    {
        elapsedTime = (decimal)stopWatch.GetElapsedTime().TotalMinutes;
        accumulatedElapsedTime += elapsedTime;
        return accumulatedElapsedTime;
    }
    private async Task AdjustCountAndRestartOrQuit(int exceptionCount, decimal accumulatedElapsedTime, CancellationToken cancellationToken)
    {
        accumulatedCount += currentCount;
        var remainingCount = totalCount - accumulatedCount;
        ReportTotals(accumulatedCount, totalCount, accumulatedElapsedTime);
        if (exceptionCount == 5 || noSuchWindowExceptionBool is true || objectDisposedExceptionBool is true)
        {
            FirefoxDriver.Quit();
        }
        await RestartExtract(remainingCount, cancellationToken);
    }
    private async Task RestartExtract(int remainingCount, CancellationToken cancellationToken)
    {
        AddressList.Clear();
        await Extract(remainingCount, cancellationToken);
    }
    private void ReportTotals(int accumulatedCount, int totalCount, decimal accumulatedElapsedTime)
    {
        var percentComplete = totalCount == 0 ? 0 : decimal.Divide(accumulatedCount, totalCount);
        var addressesProcessedPerMinute = (int)decimal.Divide(accumulatedCount, accumulatedElapsedTime);
        Serilog.Log.Information($"{County} report: {percentComplete:P0} processed out of {totalCount} total.");
        Serilog.Log.Information($"{County} report: {addressesProcessedPerMinute} processed on average per minute.");
    }
    public async Task LogException(string accountId, string exception)
    {
        ExceptionLogModel exceptionLogModel = new()
        {
            AccountId = accountId,
            Exception = exception
        };
        using (var uow = _dataContext.CreateUnitOfWork())
        {
            var exceptionLogDataService = _exceptionLogDataServiceFactory.CreateExceptionLogDataService(uow);
            await exceptionLogDataService.Create(exceptionLogModel);
        }
    }
}
