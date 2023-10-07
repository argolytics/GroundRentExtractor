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
    private IGroundRentDataServiceFactory _dataServiceFactory;
    private readonly BlobService _blobService;
    private readonly IOptionsMonitor<WebPageStringSettings> _webPageStringSettings;
    private readonly IOptionsMonitor<BlobSettings> _blobSettings;
    FirefoxDriver FirefoxDriver;
    WebDriverWait WebDriverWait;
    private IWebElement Input { get; set; }
    private string FirefoxDriverPath;
    private string GeckoDriverPath;
    private string DropDownSelect;
    private string BlobContainer;
    private string BaseUrlWindow;
    private List<GroundRentPropertyModel> AddressList = new();
    private int currentCount = 0;
    private int accumulatedCount = 0;
    private int remainingCount;
    private int totalCount = 0;
    private decimal elapsedTime = 0;
    private decimal accumulatedElapsedTime = 0;

    public Extractor(
        IDataContext dataContext,
        BlobService blobService,
        IOptionsMonitor<WebPageStringSettings> webPageStringSettings,
        IOptionsMonitor<BlobSettings> blobSettings)
    {
        _dataContext = dataContext;
        _blobService = blobService;
        _webPageStringSettings = webPageStringSettings;
        _blobSettings = blobSettings;
    }

    private void SetupDriver(string firefoxDriverPath, string geckoDriverPath)
    {
        try
        {
            if (!string.IsNullOrEmpty(firefoxDriverPath) && !string.IsNullOrEmpty(geckoDriverPath))
            {
                FirefoxProfile firefoxProfile = new(firefoxDriverPath);
                firefoxProfile.SetPreference("marionette", false);
                FirefoxOptions FirefoxOptions = new()
                {
                    Profile = firefoxProfile
                };
                //firefoxOptions.AddArguments("--headless");
                FirefoxDriver = new FirefoxDriver(geckoDriverPath, FirefoxOptions, TimeSpan.FromSeconds(60));
                WebDriverWait = new(FirefoxDriver, TimeSpan.FromSeconds(19));
                WebDriverWait.IgnoreExceptionTypes(
                    typeof(NoSuchElementException),
                    typeof(StaleElementReferenceException),
                    typeof(ElementNotSelectableException),
                    typeof(ElementNotVisibleException));
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex.ToString());
        }
    }
    public async Task Extract(
        IGroundRentDataServiceFactory dataServiceFactory,
        string county,
        int? amount,
        string firefoxDriverPath,
        string geckoDriverPath,
        string dropDownSelect,
        string blobContainer,
        CancellationToken cancellationToken)
    {
        if (FirefoxDriver == null && WebDriverWait == null)
        {
            SetupDriver(firefoxDriverPath, geckoDriverPath);
        }
        _dataServiceFactory = dataServiceFactory;
        FirefoxDriverPath = firefoxDriverPath;
        GeckoDriverPath = geckoDriverPath;
        DropDownSelect = dropDownSelect;
        BlobContainer = blobContainer;
        // Set BaseUrlWindow
        BaseUrlWindow = FirefoxDriver.CurrentWindowHandle;
        // This if condition ensures the db will only be read once, otherwise use existing AddressList
        if (AddressList.Count == 0)
        {
            if (county == "BACI1")
            {
                county = "BACI";
                using (var uow = _dataContext.CreateUnitOfWork())
                {
                    var dataService = _dataServiceFactory.CreateGroundRentDataService(uow);
                    AddressList = await dataService.SelectGroundRentPropertyByCountyAndOffset(county, 100, 0);
                }
            }
            else if (county == "BACI2")
            {
                county = "BACI";
                using (var uow = _dataContext.CreateUnitOfWork())
                {
                    var dataService = _dataServiceFactory.CreateGroundRentDataService(uow);
                    AddressList = await dataService.SelectGroundRentPropertyByCountyAndOffset(county, 100, 100);
                }
            }
            else if (county == "BACI3")
            {
                county = "BACI";
                using (var uow = _dataContext.CreateUnitOfWork())
                {
                    var dataService = _dataServiceFactory.CreateGroundRentDataService(uow);
                    AddressList = await dataService.SelectGroundRentPropertyByCountyAndOffset(county, 100, 200);
                }
            }
            else if (county == "BACI4")
            {
                county = "BACI";
                using (var uow = _dataContext.CreateUnitOfWork())
                {
                    var dataService = _dataServiceFactory.CreateGroundRentDataService(uow);
                    AddressList = await dataService.SelectGroundRentPropertyByCountyAndOffset(county, 100, 300);
                }
            }
            else if (county == "BACI5")
            {
                county = "BACI";
                using (var uow = _dataContext.CreateUnitOfWork())
                {
                    var dataService = _dataServiceFactory.CreateGroundRentDataService(uow);
                    AddressList = await dataService.SelectGroundRentPropertyByCountyAndOffset(county, 100, 400);
                }
            }
            else if (county == "BACO1")
            {
                county = "BACO";
                using (var uow = _dataContext.CreateUnitOfWork())
                {
                    var dataService = _dataServiceFactory.CreateGroundRentDataService(uow);
                    AddressList = await dataService.SelectGroundRentPropertyByCountyAndOffset(county, 1000, 0);
                }
            }
            else if (county == "BACO2")
            {
                county = "BACO";
                using (var uow = _dataContext.CreateUnitOfWork())
                {
                    var dataService = _dataServiceFactory.CreateGroundRentDataService(uow);
                    AddressList = await dataService.SelectGroundRentPropertyByCountyAndOffset(county, 1000, 1000);
                }
            }
            else if (county == "BACO3")
            {
                county = "BACO";
                using (var uow = _dataContext.CreateUnitOfWork())
                {
                    var dataService = _dataServiceFactory.CreateGroundRentDataService(uow);
                    AddressList = await dataService.SelectGroundRentPropertyByCountyAndOffset(county, 1000, 2000);
                }
            }
            else if (county == "BACO4")
            {
                county = "BACO";
                using (var uow = _dataContext.CreateUnitOfWork())
                {
                    var dataService = _dataServiceFactory.CreateGroundRentDataService(uow);
                    AddressList = await dataService.SelectGroundRentPropertyByCountyAndOffset(county, 1000, 3000);
                }
            }
            else if (county == "BACO5")
            {
                county = "BACO";
                using (var uow = _dataContext.CreateUnitOfWork())
                {
                    var dataService = _dataServiceFactory.CreateGroundRentDataService(uow);
                    AddressList = await dataService.SelectGroundRentPropertyByCountyAndOffset(county, 1000, 4000);
                }
            }
            else
            {
                using (var uow = _dataContext.CreateUnitOfWork())
                {
                    var dataService = _dataServiceFactory.CreateGroundRentDataService(uow);
                    AddressList = await dataService.SelectGroundRentPropertyByCounty(county);
                }
            }
        }
        // If after db read, all addresses for the county's db have been processed, the county is finished
        if (AddressList.Count == 0)
        {
            foreach (string window in FirefoxDriver.WindowHandles) FirefoxDriver.Close();
            FirefoxDriver.Quit();
            Serilog.Log.Error($"All properties for {county} are finished.");
        }
        totalCount = AddressList.Count + accumulatedCount;
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
                if (county == "ANNE")
                {
                    // Define Subdivision and ANNE-Specific AccountNumber
                    string Subdivision = iterAddress.AccountNumber.Substring(0, 4);
                    string AccountNumber = iterAddress.AccountNumber.Substring(4);
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
                else if (county == "BACI")
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
                    Serilog.Log.Error($"{iterAddress.AccountId} has error label in html even though it shouldn't.");
                    FirefoxDriver.Quit();
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
                            Serilog.Log.Error($"{iterAddress.AccountId} is not coming up as ground rent even though it should.");
                            FirefoxDriver.Quit();
                        }
                    }
                    // No ground rent error tag, therefore must be ground rent
                    else
                    {
                        // Load ground rent metadata table
                        WebDriverWait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.XPath(@_webPageStringSettings.CurrentValue.GroundRentMetadataTable)));
                        // Initialize variables keeping track of the total and current metadata collection count
                        var metadataCollectionCurrentCount = 0;
                        var pdfDownloadCount = 0;
                        var metadataCollectionTotalCount = FirefoxDriver.FindElements(By.XPath($@"{_webPageStringSettings.CurrentValue.GroundRentMetadataTable}/tbody/tr")).Count;
                        for (metadataCollectionCurrentCount = 0; metadataCollectionCurrentCount < metadataCollectionTotalCount;)
                        {
                            if (metadataCollectionCurrentCount == 0 ||
                                metadataCollectionCurrentCount == metadataCollectionTotalCount - 1)
                            {
                                metadataCollectionCurrentCount++;
                                continue;
                            }
                            // Per iteration, reinitialize the metadata collection to avoid stale element exception
                            WebDriverWait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.XPath(@_webPageStringSettings.CurrentValue.GroundRentMetadataTable)));
                            IReadOnlyCollection<IWebElement> metadataCollection = FirefoxDriver.FindElements(By.XPath($@"{_webPageStringSettings.CurrentValue.GroundRentMetadataTable}/tbody/tr"));
                            // Check if sql row for groundRentPdfModel already exists and if PdfDownloaded is true
                            string acknowledgementNumber = metadataCollection.ElementAt(metadataCollectionCurrentCount).FindElement(By.CssSelector($"{_webPageStringSettings.CurrentValue.AcknowledgementNumberTag}_{metadataCollectionCurrentCount - 1}")).Text ?? null;
                            using (var uow = _dataContext.CreateUnitOfWork())
                            {
                                var dataService = _dataServiceFactory.CreateGroundRentDataService(uow);
                                if (await dataService.SelectBoolGroundRentPdfIfExists(acknowledgementNumber, iterAddress.AccountId).ConfigureAwait(false))
                                {
                                    // If the row exists and its PDF is already downloaded, we skip this iteration but still count the increments
                                    GroundRentPdfModel? pdfModel = await dataService.SelectGroundRentPdf(acknowledgementNumber).ConfigureAwait(false);
                                    if (pdfModel.PdfDownloaded is true)
                                    {
                                        metadataCollectionCurrentCount++;
                                        pdfDownloadCount++;
                                        Serilog.Log.Information($"{iterAddress.AccountId} property for {acknowledgementNumber} already downloaded.");
                                        continue;
                                    }
                                }
                            }
                            // If this sql row doesn't exist, we collect pdf sql row data for a new pdfModel from the site's table's row
                            string? dateTimeFiledString = metadataCollection.ElementAt(metadataCollectionCurrentCount).FindElement(By.CssSelector($"{_webPageStringSettings.CurrentValue.DateTimeFiledTag}_{metadataCollectionCurrentCount - 1}")).Text ?? null;
                            // Ensure there are always two digits for the month for a given date so that DateTime.TryParse can parse dateTimeFiledString
                            if (dateTimeFiledString[0] != '1') if (!dateTimeFiledString.StartsWith("1/")) dateTimeFiledString = '0' + dateTimeFiledString;
                            DateTime? dateTimeFiled = DateTime.TryParse(dateTimeFiledString, out DateTime tempDate) ? tempDate : null;
                            string? documentFiledType = metadataCollection.ElementAt(metadataCollectionCurrentCount).FindElement(By.CssSelector($"{_webPageStringSettings.CurrentValue.DocumentFiledTypeTag}_{metadataCollectionCurrentCount - 1}")).Text ?? null;
                            string? pdfPageCount = metadataCollection.ElementAt(metadataCollectionCurrentCount).FindElement(By.CssSelector($"{_webPageStringSettings.CurrentValue.PdfPageCountTag}_{metadataCollectionCurrentCount - 1}")).Text ?? null;
                            string? deedReferenceData = metadataCollection.ElementAt(metadataCollectionCurrentCount).FindElement(By.CssSelector($"{_webPageStringSettings.CurrentValue.DeedReferenceTag}_{metadataCollectionCurrentCount - 1}")).Text ?? null;
                            string[]? deedReferenceDataArray = deedReferenceData.Split('/') ?? null;
                            int.TryParse(deedReferenceDataArray[3], out var yearRecordedResult);
                            // With all data populated, we can safely increment the metadata count
                            metadataCollectionCurrentCount++;
                            // Click PDF link and open PDF window
                            Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.Id(metadataCollection.ElementAt(metadataCollectionCurrentCount - 1).FindElement(By.TagName("a")).GetAttribute("id"))));
                            Input.Click();
                            foreach (string window in FirefoxDriver.WindowHandles)
                            {
                                if (BaseUrlWindow != window) FirefoxDriver.SwitchTo().Window(window);
                            }
                            // Download PDF
                            if (WebDriverWait.Until(FirefoxDriver =>
                            {
                                IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)FirefoxDriver;
                                var result = jsExecutor.ExecuteScript("return document.readyState;");
                                return result != null && result.Equals("complete");
                            }))
                            {
                                PrintOptions printOptions = new();
                                var printDocument = FirefoxDriver.Print(printOptions);
                                var blobName = $"{county}/{iterAddress.AccountId}/{documentFiledType}_{acknowledgementNumber}.pdf";
                                bool pdfDownloaded = await _blobService.UploadBlob(blobName, printDocument, blobContainer).ConfigureAwait(false);
                                if (pdfDownloaded) pdfDownloadCount++;
                                using (var uow = _dataContext.CreateUnitOfWork())
                                {
                                    var dataService = _dataServiceFactory.CreateGroundRentDataService(uow);
                                    if (await dataService.CreateOrUpdateGroundRentPdf(new GroundRentPdfModel()
                                    {
                                        AddressId = iterAddress.Id,
                                        AccountId = iterAddress.AccountId,
                                        AcknowledgementNumber = acknowledgementNumber,
                                        DateTimeFiled = dateTimeFiled,
                                        DocumentFiledType = documentFiledType,
                                        PdfPageCount = pdfPageCount,
                                        Book = deedReferenceDataArray[0] ?? null,
                                        Page = deedReferenceDataArray[1] ?? null,
                                        ClerkInitials = deedReferenceDataArray[2] ?? null,
                                        YearRecorded = yearRecordedResult,
                                        PdfDownloaded = pdfDownloaded
                                    }).ConfigureAwait(false)) { }
                                    else Serilog.Log.Error($"{iterAddress.AccountId} for PDF {documentFiledType}_{acknowledgementNumber} could not be created.");
                                }
                                // The row we just created is now read for so we can get access to its [Id] as one of the blob tags
                                GroundRentPdfModel pdfModelJustCreatedOrUpdatedButNowWithId = new();
                                using (var uow = _dataContext.CreateUnitOfWork())
                                {
                                    var dataService = _dataServiceFactory.CreateGroundRentDataService(uow);
                                    pdfModelJustCreatedOrUpdatedButNowWithId = await dataService.SelectGroundRentPdf(acknowledgementNumber).ConfigureAwait(false);
                                }
                                // A blob can only be tagged after its been uploaded, so we do so here
                                _blobService.TagBlob(blobName, iterAddress, pdfModelJustCreatedOrUpdatedButNowWithId, _blobSettings.CurrentValue.GroundRentPdfsContainer);
                                // Close out the PDF window and switch back to the baseUrlWindow
                            }
                            else { Serilog.Log.Error($"{iterAddress.AccountId} property for PDF {acknowledgementNumber} could not be downloaded."); }
                            if (FirefoxDriver.CurrentWindowHandle != BaseUrlWindow) FirefoxDriver.Close();
                            FirefoxDriver.SwitchTo().Window(BaseUrlWindow);
                        }
                        foreach (string window in FirefoxDriver.WindowHandles)
                        {
                            FirefoxDriver.SwitchTo().Window(window);
                            if (FirefoxDriver.CurrentWindowHandle != BaseUrlWindow) FirefoxDriver.Close();
                        }
                        FirefoxDriver.SwitchTo().Window(BaseUrlWindow);
                        // Check if total PDF count (as measured by the amount of rows in the table minus two) matches the actual count
                        bool pdfsDownloaded = pdfDownloadCount == metadataCollectionTotalCount - 2;
                        using (var uow = _dataContext.CreateUnitOfWork())
                        {
                            var dataService = _dataServiceFactory.CreateGroundRentDataService(uow);
                            if (await dataService.UpdateGroundRentProperty(new GroundRentPropertyModel()
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
                                PdfCount = metadataCollectionTotalCount - 2,
                                IsRedeemed = iterAddress.IsRedeemed,
                                PdfsDownloaded = pdfsDownloaded
                            }).ConfigureAwait(false)) { }
                            else Serilog.Log.Error($"{iterAddress.AccountId} could not be updated.");
                        }
                        currentCount++;
                        AddressList.Remove(iterAddress);
                    }
                }
            }
            Serilog.Log.Information($"{county}: batch complete.");
            accumulatedCount += currentCount;
            ReportTotals(stopWatch);
            foreach (string window in FirefoxDriver.WindowHandles) FirefoxDriver.Close();
            FirefoxDriver.Quit();
            FirefoxDriver = null;
            WebDriverWait = null;
        }
        catch (NoSuchWindowException e)
        {
            Serilog.Log.Error($"{county} critical exception: {e.Message}. Quitting.");
            accumulatedCount += currentCount;
            remainingCount = totalCount - accumulatedCount;
            AddressList.Clear();
            ReportTotals(stopWatch);
            foreach (string window in FirefoxDriver.WindowHandles) FirefoxDriver.Close();
            FirefoxDriver.Quit();
        }
        catch (ObjectDisposedException e)
        {
            Serilog.Log.Error($"{county} critical exception: {e.Message}. Quitting.");
            accumulatedCount += currentCount;
            remainingCount = totalCount - accumulatedCount;
            AddressList.Clear();
            ReportTotals(stopWatch);
            foreach (string window in FirefoxDriver.WindowHandles) FirefoxDriver.Close();
            FirefoxDriver.Quit();
        }
        catch (WebDriverException e)
        {
            Serilog.Log.Error($"{county} critical exception: {e.Message}. Quitting.");
            accumulatedCount += currentCount;
            remainingCount = totalCount - accumulatedCount;
            AddressList.Clear();
            ReportTotals(stopWatch);
            foreach (string window in FirefoxDriver.WindowHandles) FirefoxDriver.Close();
            FirefoxDriver.Quit();
        }
        catch (System.Net.WebException e)
        {
            Serilog.Log.Error($"{county} critical exception: {e.Message}. Quitting.");
            accumulatedCount += currentCount;
            remainingCount = totalCount - accumulatedCount;
            AddressList.Clear();
            ReportTotals(stopWatch);
            foreach (string window in FirefoxDriver.WindowHandles) FirefoxDriver.Close();
            FirefoxDriver.Quit();
        }
        catch (TimeoutException e)
        {
            Serilog.Log.Information($"{county} exception: {e.Message}. Restarting.");
            accumulatedCount += currentCount;
            remainingCount = totalCount - accumulatedCount;
            ReportTotals(stopWatch);
            await RestartExtract(remainingCount, county, cancellationToken);
        }
        catch (Exception e)
        {
            Serilog.Log.Error($"{county} exception: {e.Message}. Restarting.");
            accumulatedCount += currentCount;
            remainingCount = totalCount - accumulatedCount;
            ReportTotals(stopWatch);
            await RestartExtract(remainingCount, county, cancellationToken);
        }
    }
    private void ReportTotals(ValueStopwatch stopWatch)
    {
        accumulatedElapsedTime = CalculateElapsedTime(stopWatch);
        var percentComplete = totalCount == 0 ? 0 : decimal.Divide(accumulatedCount, totalCount);
        var addressesProcessedPerMinute = (int)decimal.Divide(accumulatedCount, accumulatedElapsedTime);
        Serilog.Log.Information($"{percentComplete:P0} of {totalCount} completed. {addressesProcessedPerMinute} average per minute.");
    }
    private async Task RestartExtract(int remainingCount, string county, CancellationToken cancellationToken)
    {
        await Extract(
            _dataServiceFactory,
            county,
            remainingCount,
            FirefoxDriverPath,
            GeckoDriverPath,
            DropDownSelect,
            BlobContainer,
            cancellationToken);
    }
    private decimal CalculateElapsedTime(ValueStopwatch stopWatch)
    {
        elapsedTime = (decimal)stopWatch.GetElapsedTime().TotalMinutes;
        accumulatedElapsedTime += elapsedTime;
        return accumulatedElapsedTime;
    }
}
