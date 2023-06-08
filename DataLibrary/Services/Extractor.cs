using Microsoft.Extensions.Options;
using DataLibrary.DbAccess;
using DataLibrary.DbServices;
using DataLibrary.Models;
using DataLibrary.Settings;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DataLibrary.Services;

public class Extractor
{
    private readonly IDataContext _dataContext;
    private IDataServiceFactory _dataServiceFactory;
    private readonly IExceptionLogDataServiceFactory _exceptionLogDataServiceFactory;
    private readonly BlobService _blobService;
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
    private string County;
    private string FirefoxDriverPath;
    private string GeckoDriverPath;
    private string DropDownSelect;
    private string BlobContainer;
    private string BaseUrlWindow;
    private List<AddressModel> AddressList = new();
    private bool? dbTransactionResultBool = null;
    private int currentCount = 0;
    private int accumulatedCount = 0;
    private int remainingCount;
    private int totalCount = 0;
    private decimal elapsedTime = 0;
    private decimal accumulatedElapsedTime = 0;

    public Extractor(
        IDataContext dataContext,
        IExceptionLogDataServiceFactory exceptionLogDataServiceFactory,
        BlobService blobService,
        IOptionsMonitor<WebPageStringSettings> webPageStringSettings)
    {
        _dataContext = dataContext;
        _exceptionLogDataServiceFactory = exceptionLogDataServiceFactory;
        _blobService = blobService;
        _webPageStringSettings = webPageStringSettings;
    }

    private void SetupDriver(string firefoxDriverPath, string geckoDriverPath)
    {
        try
        {
            if (!string.IsNullOrEmpty(firefoxDriverPath) && !string.IsNullOrEmpty(geckoDriverPath))
            {
                FirefoxProfile firefoxProfile = new(firefoxDriverPath);
                FirefoxOptions FirefoxOptions = new()
                {
                    Profile = firefoxProfile,
                };
                //firefoxOptions.AddArguments("--headless");
                FirefoxDriver = new FirefoxDriver(geckoDriverPath, FirefoxOptions, TimeSpan.FromSeconds(60));
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
    public async Task Extract(
        IDataServiceFactory dataServiceFactory,
        string county,
        string firefoxDriverPath,
        string geckoDriverPath,
        string dropDownSelect,
        string blobContainer,
        int count,
        CancellationToken cancellationToken)
    {
        if (FirefoxDriver == null && WebDriverWait == null)
        {
            SetupDriver(firefoxDriverPath, geckoDriverPath);
        }
        _dataServiceFactory = dataServiceFactory;
        County = county;
        FirefoxDriverPath = firefoxDriverPath;
        GeckoDriverPath = geckoDriverPath;
        DropDownSelect = dropDownSelect;
        BlobContainer = blobContainer;
        // Set BaseUrlWindow
        BaseUrlWindow = FirefoxDriver.CurrentWindowHandle;
        // This if condition ensures the db will only be read once, otherwise use existing AddressList
        if (AddressList.Count == 0)
        {
            using (var uow = _dataContext.CreateUnitOfWork())
            {
                var dataService = _dataServiceFactory.CreateExtractorDataService(uow);
                AddressList = await dataService.ReadAddressTopAmountWhereIsGroundRentNull(count);
            }
        }
        // If after db read, all addresses for the county's db have been processed, the county is finished
        if (AddressList.Count == 0)
        {
            foreach (string window in FirefoxDriver.WindowHandles) FirefoxDriver.Close();
            FirefoxDriver.Quit();
            Serilog.Log.Error($"{County} finished.");
        }
        totalCount = count + accumulatedCount;
        currentCount = 0;
        var stopWatch = ValueStopwatch.StartNew();
        try
        {
            var iterList = AddressList.ToList();
            foreach (var iterAddress in iterList)
            {
                if (cancellationToken.IsCancellationRequested) break;
                // Select appropriate county
                FirefoxDriver.Navigate().GoToUrl(_webPageStringSettings.CurrentValue.BaseUrl);
                Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(dropDownSelect)));
                Input.Click();
                // Select Property Account Identifier
                Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(_webPageStringSettings.CurrentValue.PropertyAccountIdentifierSelect)));
                Input.Click();
                // Click Continue button
                Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(_webPageStringSettings.CurrentValue.ContinueClick)));
                Input.Click();
                if (County == "ANNE")
                {
                    // Define Subdivision and ANNE-Specific AccountNumber
                    string Subdivision = iterAddress.AccountNumber.Substring(0, 4).Trim();
                    string AccountNumber = iterAddress.AccountNumber.Substring(4).Trim();
                    // Input District, Subdivision, and AccountNumber
                    Input = WebDriverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(_webPageStringSettings.CurrentValue.DistrictInput)));
                    Input.Clear();
                    Input.SendKeys(iterAddress.Ward);
                    Input = WebDriverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(_webPageStringSettings.CurrentValue.SubdivisionInput)));
                    Input.Clear();
                    Input.SendKeys(Subdivision);
                    Input = WebDriverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(_webPageStringSettings.CurrentValue.AccountNumberInput)));
                    Input.Clear();
                    Input.SendKeys(AccountNumber);
                }
                else if (County == "BACI")
                {
                    // Input Ward, Section, Block, and Lot
                    Input = WebDriverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(_webPageStringSettings.CurrentValue.WardInput)));
                    Input.Clear();
                    Input.SendKeys(iterAddress.Ward);
                    Input = WebDriverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(_webPageStringSettings.CurrentValue.SectionInput)));
                    Input.Clear();
                    Input.SendKeys(iterAddress.Section);
                    Input = WebDriverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(_webPageStringSettings.CurrentValue.BlockInput)));
                    Input.Clear();
                    Input.SendKeys(iterAddress.Block);
                    Input = WebDriverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(_webPageStringSettings.CurrentValue.LotInput)));
                    Input.Clear();
                    Input.SendKeys(iterAddress.Lot);
                }
                else
                {
                    // Input District and AccountNumber
                    Input = WebDriverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(_webPageStringSettings.CurrentValue.DistrictInput)));
                    Input.Clear();
                    Input.SendKeys(iterAddress.Ward);
                    Input = WebDriverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(_webPageStringSettings.CurrentValue.AccountNumberInput)));
                    Input.Clear();
                    Input.SendKeys(iterAddress.AccountNumber);
                }
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
                            var dataService = _dataServiceFactory.CreateExtractorDataService(uow);
                            dbTransactionResultBool = await dataService.DeleteAddress(iterAddress.AccountId);
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
                            // Property is not ground rent
                            using (var uow = _dataContext.CreateUnitOfWork())
                            {
                                var dataService = _dataServiceFactory.CreateExtractorDataService(uow);
                                dbTransactionResultBool = await dataService.UpdateAddress(new AddressModel()
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
                                // so we check for null here. If null, we skip metadata and PDF download but make note in log
                                if (dateTimeFiledString is null)
                                {
                                    await LogException(iterAddress.AccountId, ExceptionLog.DateTimeFiledIsNull.ToString());
                                    Serilog.Log.Error($"{County} accountId {iterAddress.AccountId}: " + ExceptionLog.DateTimeFiledIsNull.ToString());
                                    metadataCollectionCurrentCount++;
                                    continue;
                                }
                                // Ensure there are always two digits for the month for a given date
                                // so that DateTime.TryParse can parse dateTimeFiledString
                                if (dateTimeFiledString[0] != '1')
                                {
                                    if (!dateTimeFiledString.StartsWith("1/")) dateTimeFiledString = '0' + dateTimeFiledString;
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
                                    var dataService = _dataServiceFactory.CreateExtractorDataService(uow);
                                    dbTransactionResultBool = await dataService.CreateGroundRentPdf(new GroundRentPdfModel()
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
                                    metadataCollectionCurrentCount++;
                                    // If we cannot store metadata, we do not want the accompanying PDF, so we continue here
                                    continue;
                                }
                                metadataCollectionCurrentCount++;
                                foreach (string window in FirefoxDriver.WindowHandles)
                                {
                                    FirefoxDriver.SwitchTo().Window(window);
                                    if (FirefoxDriver.CurrentWindowHandle != BaseUrlWindow) FirefoxDriver.Close();
                                }
                                FirefoxDriver.SwitchTo().Window(BaseUrlWindow);
                                // Click and open PDF
                                Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.Id(metadataCollection.ElementAt(metadataCollectionCurrentCount - 1).FindElement(By.TagName("a")).GetAttribute("id"))));
                                Input.Click();
                                // Switch to PDF window and, at any point within the try catch block, if 
                                // the WebDriverTimeoutException occurs, catch it and break out of the foreach loop
                                foreach (string window in FirefoxDriver.WindowHandles)
                                {
                                    try
                                    {
                                        if (BaseUrlWindow != window)
                                        {
                                            FirefoxDriver.SwitchTo().Window(window);
                                            IJavaScriptExecutor jsExecutor = FirefoxDriver;
                                            if (WebDriverWait.Until(FirefoxDriver => jsExecutor.ExecuteScript("return document.readyState").Equals("complete")))
                                            {
                                                // Upload PDF to blob storage
                                                PrintOptions printOptions = new();
                                                var printDocument = FirefoxDriver.Print(printOptions);
                                                var accountIdTrimmed = groundRentPdfModel.AccountId.Trim();
                                                var pdfFileName = $"{accountIdTrimmed}_{groundRentPdfModelList.FirstOrDefault().DocumentFiledType}_{groundRentPdfModelList.FirstOrDefault().AcknowledgementNumber}.pdf";
                                                if (await _blobService.UploadBlob(pdfFileName, printDocument, blobContainer))
                                                {
                                                    pdfDownloadCount++;
                                                }
                                                else
                                                {
                                                    Serilog.Log.Information($"{County} accountId {iterAddress.AccountId}: {pdfFileName} already exists in the blob container.");
                                                    if (FirefoxDriver.CurrentWindowHandle != BaseUrlWindow) FirefoxDriver.Close();
                                                    FirefoxDriver.SwitchTo().Window(BaseUrlWindow);
                                                    pdfDownloadCount++;
                                                    continue;
                                                }
                                            }
                                        }
                                        if (FirefoxDriver.CurrentWindowHandle != BaseUrlWindow) FirefoxDriver.Close();
                                        FirefoxDriver.SwitchTo().Window(BaseUrlWindow);
                                    }
                                    catch (WebDriverTimeoutException)
                                    {
                                        await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailCouldNotStorePdf.ToString());
                                        Serilog.Log.Error($"{County} accountId {iterAddress.AccountId}: " + ExceptionLog.TransactionFailCouldNotStorePdf.ToString());
                                        if (FirefoxDriver.CurrentWindowHandle != BaseUrlWindow) FirefoxDriver.Close();
                                        FirefoxDriver.SwitchTo().Window(BaseUrlWindow);
                                        continue;
                                    }
                                    catch (NullReferenceException)
                                    {
                                        await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailCouldNotStorePdf.ToString());
                                        Serilog.Log.Error($"{County} accountId {iterAddress.AccountId}: " + ExceptionLog.TransactionFailCouldNotStorePdf.ToString());
                                        if (FirefoxDriver.CurrentWindowHandle != BaseUrlWindow) FirefoxDriver.Close();
                                        FirefoxDriver.SwitchTo().Window(BaseUrlWindow);
                                        continue;
                                    }
                                }
                            }
                        }
                        foreach (string window in FirefoxDriver.WindowHandles)
                        {
                            FirefoxDriver.SwitchTo().Window(window);
                            if (FirefoxDriver.CurrentWindowHandle != BaseUrlWindow) FirefoxDriver.Close();
                        }
                        FirefoxDriver.SwitchTo().Window(BaseUrlWindow);
                        // Check if all PDFs have been downloaded by checking against the true amount of metadata table rows
                        var pdfTotalCount = metadataCollectionTotalCount - 2;
                        if (pdfDownloadCount == pdfTotalCount)
                        {
                            using (var uow = _dataContext.CreateUnitOfWork())
                            {
                                var dataService = _dataServiceFactory.CreateExtractorDataService(uow);
                                dbTransactionResultBool = await dataService.UpdateAddress(new AddressModel()
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
                                    PdfCount = pdfTotalCount,
                                    AllDataDownloaded = true
                                });
                            }
                            if (dbTransactionResultBool is false)
                            {
                                await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailCouldNotStoreAddress.ToString());
                                Serilog.Log.Error($"{County} accountId {iterAddress.AccountId}: " + ExceptionLog.TransactionFailCouldNotStoreAddress.ToString());
                            }
                        }
                        // PDF download count or metadata collection current are not count equal to PDF total count
                        else
                        {
                            using (var uow = _dataContext.CreateUnitOfWork())
                            {
                                var dataService = _dataServiceFactory.CreateExtractorDataService(uow);
                                dbTransactionResultBool = await dataService.UpdateAddress(new AddressModel()
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
                                    PdfCount = pdfTotalCount,
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
            Serilog.Log.Information($"{County}: batch complete.");
            accumulatedCount += currentCount;
            ReportTotals(stopWatch);
            foreach (string window in FirefoxDriver.WindowHandles) FirefoxDriver.Close();
            FirefoxDriver.Quit();
        }
        catch (NoSuchWindowException e)
        {
            Serilog.Log.Error($"{County} critical exception: {e.Message}. Quitting.");
            accumulatedCount += currentCount;
            remainingCount = totalCount - accumulatedCount;
            AddressList.Clear();
            ReportTotals(stopWatch);
            foreach (string window in FirefoxDriver.WindowHandles) FirefoxDriver.Close();
            FirefoxDriver.Quit();
        }
        catch (ObjectDisposedException e)
        {
            Serilog.Log.Error($"{County} critical exception: {e.Message}. Quitting.");
            accumulatedCount += currentCount;
            remainingCount = totalCount - accumulatedCount;
            AddressList.Clear();
            ReportTotals(stopWatch);
            foreach (string window in FirefoxDriver.WindowHandles) FirefoxDriver.Close();
            FirefoxDriver.Quit();
        }
        catch (Exception e)
        {
            Serilog.Log.Error($"{County} exception: {e.Message}. Restarting.");
            accumulatedCount += currentCount;
            remainingCount = totalCount - accumulatedCount;
            ReportTotals(stopWatch);
            await RestartExtract(remainingCount, cancellationToken);
        }
    }
    private void ReportTotals(ValueStopwatch stopWatch)
    {
        accumulatedElapsedTime = CalculateElapsedTime(stopWatch);
        var percentComplete = totalCount == 0 ? 0 : decimal.Divide(accumulatedCount, totalCount);
        var addressesProcessedPerMinute = (int)decimal.Divide(accumulatedCount, accumulatedElapsedTime);
        Serilog.Log.Information($"{County}: {percentComplete:P0} of {totalCount} completed. {addressesProcessedPerMinute} average per minute.");
    }
    private async Task RestartExtract(int count, CancellationToken cancellationToken)
    {
        await Extract(
            _dataServiceFactory,
            County,
            FirefoxDriverPath,
            GeckoDriverPath,
            DropDownSelect,
            BlobContainer,
            count,
            cancellationToken);
    }
    private decimal CalculateElapsedTime(ValueStopwatch stopWatch)
    {
        elapsedTime = (decimal)stopWatch.GetElapsedTime().TotalMinutes;
        accumulatedElapsedTime += elapsedTime;
        return accumulatedElapsedTime;
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
