using DataLibrary.DbAccess;
using DataLibrary.DbServices;
using DataLibrary.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Azure.Storage.Blobs;
using DataLibrary.Settings;

namespace DataLibrary.Services;

public class BACIExtractor
{
    private readonly IDataContext _dataContext;
    private readonly BlobService _blobService;
    private readonly BACIDataServiceFactory _baciDataServiceFactory;
    private readonly ExceptionLogDataServiceFactory _exceptionLogDataServiceFactory;
    FirefoxDriver FirefoxDriver;
    WebDriverWait WebDriverWait;
    private IWebElement Input { get; set; }
    private readonly string BACIDropDownSelect = "#cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlCounty > option:nth-child(4)";
    private readonly string PropertyAccountIdentifierSelect = "#cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucSearchType_ddlSearchType > option:nth-child(3)";
    private readonly string ContinueClick = "#cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StartNavigationTemplateContainerID_btnContinue";
    private readonly string WardInput = "#cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtWard";
    private readonly string SectionInput = "#cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtSection";
    private readonly string BlockInput = "#cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtBlock";
    private readonly string LotInput = "#cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucEnterData_txtLot";
    private readonly string NextClick = "#cphMainContentArea_ucSearchType_wzrdRealPropertySearch_StepNavigationTemplateContainerID_btnStepNextButton";
    private readonly string GenericErrorTag = "#cphMainContentArea_ucSearchType_lblErr";
    private readonly string NoRecordsMessage = "There are no records that match your criteria";
    private readonly string InvisibilityCatch = "head > style:nth-child(29)";
    private readonly string GroundRentRegistrationLinkClick = "#cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucDetailsSearch_dlstDetaisSearch_lnkGroundRentRegistration_0";
    private readonly string GroundRentErrorTag = "#cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucGroundRent_lblErr";
    private readonly string NoGroundRentMessage = "There is currently no ground rent";
    private readonly string GroundRentMetadataTable = @"//table[@id='cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucGroundRent_gv_GRRegistratonResult']";
    private readonly string PdfSaveFilePath = @"C:\Users\Jason\Desktop\GroundRentRegistrationPdfs\BACI\";
    private string BaseUrl { get; set; } = "https://sdat.dat.maryland.gov/RealProperty/Pages/default.aspx";
    private enum ExceptionLog
    {
        TransactionFailAddressNotGroundRent,
        TransactionFailCouldNotStoreAddress,
        TransactionFailCouldNotStoreMetadata,
        TransactionFailCouldNotStorePdf,
        TransactionFailCouldNotDeleteAddress,
        DateTimeFiledIsNull
    }
    private List<AddressModel> AddressList = new();
    private List<string?> CatchList = new();
    private bool? dbTransactionResult = null;
    private bool? allDataDownloaded = true;
    private int exceptionCount = 0;

    public BACIExtractor(
        IDataContext dataContext,
        BlobService blobService,
        BACIDataServiceFactory baciDataServiceFactory,
        ExceptionLogDataServiceFactory exceptionLogDataServiceFactory,
        DriverPathSettings pathSettings)
    {
        _dataContext = dataContext;
        _blobService = blobService;
        _baciDataServiceFactory = baciDataServiceFactory;
        _exceptionLogDataServiceFactory = exceptionLogDataServiceFactory;

        FirefoxProfile firefoxProfile = new(pathSettings.FirefoxProfilePath);
        FirefoxOptions FirefoxOptions = new()
        {
            Profile = firefoxProfile,
        };
        //firefoxOptions.AddArguments("--headless");
        FirefoxDriver = new FirefoxDriver(pathSettings.GeckoDriverPath, FirefoxOptions, TimeSpan.FromSeconds(20));
        WebDriverWait = new(FirefoxDriver, TimeSpan.FromSeconds(20));
        WebDriverWait.IgnoreExceptionTypes(
            typeof(NoSuchElementException),
            typeof(StaleElementReferenceException),
            typeof(ElementNotSelectableException),
            typeof(ElementNotVisibleException));
    }
    private async Task RestartExtract(int amountToExtract)
    {
        AddressList.Clear();
        await Extract(amountToExtract);
    }
    private static void ReportTotals(FirefoxDriver FirefoxDriver, int addressListIterationCount, int addressListIterationTotal)
    {
        var percentComplete = addressListIterationTotal == 0 ? 0 : decimal.Divide(addressListIterationCount, addressListIterationTotal);
        Console.WriteLine($"{FirefoxDriver} has processed {percentComplete:P0} of {addressListIterationTotal} addresses.");
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
    public async Task Extract(int amountToExtract)
    {
        // Define baseUrlWindow
        var baseUrlWindow = FirefoxDriver.CurrentWindowHandle;
        // Read and populate address list
        using (var uow = _dataContext.CreateUnitOfWork())
        {
            var baciDataService = _baciDataServiceFactory.CreateExtractorDataService(uow);
            AddressList = await baciDataService.ReadAddressTopAmountWhereIsGroundRentNull(amountToExtract);
        }
        if (AddressList.Count == 0)
        {
            foreach (string window in FirefoxDriver.WindowHandles) FirefoxDriver.Close();
            FirefoxDriver.Quit();
            Console.WriteLine("Baltimore City complete.");
        }
        var addressListIterationCount = 0;
        var addressListIterationTotal = AddressList.Count;
        try
        {
            var iterList = AddressList.ToList();
            foreach (var iterAddress in iterList)
            {
                iterAddress.County = "BACI";
                // Selecting appropriate county
                FirefoxDriver.Navigate().GoToUrl(BaseUrl);
                Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(BACIDropDownSelect)));
                Input.Click();
                // Selecting "PROPERTY ACCOUNT IDENTIFIER"
                Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(PropertyAccountIdentifierSelect)));
                Input.Click();
                // Click Continue button
                Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(ContinueClick)));
                Input.Click();
                if (iterAddress.County == "BACI")
                {
                    // Input Ward, Section, Block, and Lot
                    Input = WebDriverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(WardInput)));
                    Input.Clear();
                    Input.SendKeys(iterAddress.Ward);
                    Input = WebDriverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(SectionInput)));
                    Input.Clear();
                    Input.SendKeys(iterAddress.Section);
                    Input = WebDriverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(BlockInput)));
                    Input.Clear();
                    Input.SendKeys(iterAddress.Block);
                    Input = WebDriverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(LotInput)));
                    Input.Clear();
                    Input.SendKeys(iterAddress.Lot);
                }
                // Click Next button
                Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(NextClick)));
                Input.Click();
                // Check for any error labels
                if (FirefoxDriver.FindElements(By.CssSelector(GenericErrorTag)).Count != 0)
                {
                    // If address does not exist in SDAT
                    if (FirefoxDriver.FindElement(By.CssSelector(GenericErrorTag))
                        .Text.Contains(NoRecordsMessage))
                    {
                        using (var uow = _dataContext.CreateUnitOfWork())
                        {
                            var baciDataService = _baciDataServiceFactory.CreateExtractorDataService(uow);
                            dbTransactionResult = await baciDataService.DeleteAddress(iterAddress.AccountId);
                        }
                        addressListIterationCount++;
                        AddressList.Remove(iterAddress);
                    }
                    // There must be an error tag that is different from the normal "no records exist" message
                    else
                    {
                        await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailCouldNotDeleteAddress.ToString());
                    }
                    addressListIterationCount++;
                    AddressList.Remove(iterAddress);
                }
                // There are no error tags so we are free to check if address is fee simple or ground rent
                else
                {
                    // Click Ground Rent Registration link
                    WebDriverWait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(InvisibilityCatch)));
                    Input = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(GroundRentRegistrationLinkClick)));
                    Input.Click();
                    // Check if html has ground rent error tag (meaning property has no ground rent registered)
                    if (FirefoxDriver.FindElements(By.CssSelector(GroundRentErrorTag)).Count != 0)
                    {
                        if (FirefoxDriver.FindElement(By.CssSelector(GroundRentErrorTag))
                            .Text.Contains(NoGroundRentMessage))
                        {
                            // Property is fee simple (not ground rent)
                            using (var uow = _dataContext.CreateUnitOfWork())
                            {
                                var baciDataService = _baciDataServiceFactory.CreateExtractorDataService(uow);
                                dbTransactionResult = await baciDataService.UpdateAddress(new AddressModel()
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
                                    PdfCount = 0,
                                    AllDataDownloaded = null
                                });
                            }
                            if (dbTransactionResult is false)
                            {
                                await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailAddressNotGroundRent.ToString());
                            }
                        }
                        addressListIterationCount++;
                        AddressList.Remove(iterAddress);
                    }
                    // No ground rent error tag, therefore must be ground rent
                    else
                    {
                        // Load ground rent metadata table
                        WebDriverWait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.XPath(GroundRentMetadataTable)));
                        // Initialize variables keeping track of the total and current metadata collection count
                        var metadataCollectionCurrentCount = 0;
                        var pdfDownloadCount = 0;
                        var metadataCollectionTotalCount = FirefoxDriver.FindElements(By.XPath($"{GroundRentMetadataTable}/tbody/tr")).Count;
                        List<GroundRentPdfModel> groundRentPdfModelList = new();
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
                                WebDriverWait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.XPath(GroundRentMetadataTable)));
                                IReadOnlyCollection<IWebElement> metadataCollection = FirefoxDriver.FindElements(By.XPath($"{GroundRentMetadataTable}/tbody/tr"));
                                GroundRentPdfModel groundRentPdfModel = new();
                                groundRentPdfModelList.Add(groundRentPdfModel);
                                groundRentPdfModel.AddressId = iterAddress.Id;
                                groundRentPdfModel.AccountId = iterAddress.AccountId;
                                string? dateTimeFiledString = metadataCollection.ElementAt(metadataCollectionCurrentCount).FindElement(By.CssSelector($"#cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucGroundRent_gv_GRRegistratonResult_txtDateFiled_{metadataCollectionCurrentCount - 1}")).Text ?? null;
                                // DateTimeFiled cannot be null since it is used as the sql table's Primary Key
                                // so we check for null here. If null, we skip metadata and pdf download but make note in log
                                if (dateTimeFiledString is null)
                                {
                                    await LogException(iterAddress.AccountId, ExceptionLog.DateTimeFiledIsNull.ToString());
                                    allDataDownloaded = false;
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
                                groundRentPdfModel.DocumentFiledType = metadataCollection.ElementAt(metadataCollectionCurrentCount).FindElement(By.CssSelector($"#cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucGroundRent_gv_GRRegistratonResult_txtDocument_{metadataCollectionCurrentCount - 1}")).Text ?? null;
                                groundRentPdfModel.AcknowledgementNumber = metadataCollection.ElementAt(metadataCollectionCurrentCount).FindElement(By.CssSelector($"#cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucGroundRent_gv_GRRegistratonResult_txtAcknowledgementNub_{metadataCollectionCurrentCount - 1}")).Text ?? null;
                                groundRentPdfModel.PdfPageCount = metadataCollection.ElementAt(metadataCollectionCurrentCount).FindElement(By.CssSelector($"#cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucGroundRent_gv_GRRegistratonResult_txtpages_{metadataCollectionCurrentCount - 1}")).Text ?? null;
                                string? deedReferenceData = metadataCollection.ElementAt(metadataCollectionCurrentCount).FindElement(By.CssSelector($"#cphMainContentArea_ucSearchType_wzrdRealPropertySearch_ucGroundRent_gv_GRRegistratonResult_txtDeedRef_{metadataCollectionCurrentCount - 1}")).Text ?? null;
                                var deedReferenceDataArray = deedReferenceData.Split('/') ?? null;
                                groundRentPdfModel.Book = deedReferenceDataArray[0] ?? null;
                                groundRentPdfModel.Page = deedReferenceDataArray[1] ?? null;
                                groundRentPdfModel.ClerkInitials = deedReferenceDataArray[2] ?? null;
                                int.TryParse(deedReferenceDataArray[3], out var yearRecordedResult);
                                groundRentPdfModel.YearRecorded = yearRecordedResult;
                                using (var uow = _dataContext.CreateUnitOfWork())
                                {
                                    var baciDataService = _baciDataServiceFactory.CreateExtractorDataService(uow);
                                    dbTransactionResult = await baciDataService.CreateGroundRentPdf(new GroundRentPdfModel()
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
                                if (dbTransactionResult is false)
                                {
                                    await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailCouldNotStoreMetadata.ToString());
                                    allDataDownloaded = false;
                                    metadataCollectionCurrentCount++;
                                    // If we cannot store metadata, we do not want the accompanying pdf, so we continue here
                                    continue;
                                }
                                metadataCollectionCurrentCount++;
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
                                            // Download pdf, store locally (change later to blob db)
                                            PrintOptions printOptions = new();
                                            //FirefoxDriver.Print(printOptions).SaveAsFile($"{PdfSaveFilePath}{groundRentPdfModel.AccountId}_{groundRentPdfModelList.FirstOrDefault().DocumentFiledType}_{groundRentPdfModelList.FirstOrDefault().AcknowledgementNumber}.pdf");
                                            var printDocument = FirefoxDriver.Print(printOptions);
                                            var pdfFileName = $"{PdfSaveFilePath}{groundRentPdfModel.AccountId}_{groundRentPdfModelList.FirstOrDefault().DocumentFiledType}_{groundRentPdfModelList.FirstOrDefault().AcknowledgementNumber}.pdf";
                                            dbTransactionResult = await _blobService.UploadBlob(pdfFileName, printDocument, "BACI");
                                            if (dbTransactionResult is true) pdfDownloadCount++;
                                            if (dbTransactionResult is false)
                                            {
                                                await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailCouldNotStorePdf.ToString());
                                                allDataDownloaded = false;
                                            }
                                            groundRentPdfModelList.RemoveAt(0);
                                            // Close pdf window and switch back to baseUrlWindow
                                            FirefoxDriver.Close();
                                            FirefoxDriver.SwitchTo().Window(baseUrlWindow);
                                        }
                                        else
                                        {
                                            await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailCouldNotStorePdf.ToString());
                                            allDataDownloaded = false;
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
                        if (allDataDownloaded is true)
                        {
                            using (var uow = _dataContext.CreateUnitOfWork())
                            {
                                var baciDataService = _baciDataServiceFactory.CreateExtractorDataService(uow);
                                dbTransactionResult = await baciDataService.UpdateAddress(new AddressModel()
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
                            if (dbTransactionResult is false)
                            {
                                await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailCouldNotStoreAddress.ToString());
                            }
                        }
                        // pdf download count or metadata collection current are not count equal to pdf total count
                        else
                        {
                            using (var uow = _dataContext.CreateUnitOfWork())
                            {
                                var baciDataService = _baciDataServiceFactory.CreateExtractorDataService(uow);
                                dbTransactionResult = await baciDataService.UpdateAddress(new AddressModel()
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
                            if (dbTransactionResult is false)
                            {
                                await LogException(iterAddress.AccountId, ExceptionLog.TransactionFailCouldNotStoreAddress.ToString());
                            }
                        }
                        addressListIterationCount++;
                        AddressList.Remove(iterAddress);
                    }
                }
            }
        }
        catch (ElementClickInterceptedException elementClickInterceptedException)
        {
            exceptionCount++;
            CatchList.Add(elementClickInterceptedException.Message);
            Console.WriteLine($"Total exception count: {exceptionCount}");
            Console.WriteLine($"Error Message: {elementClickInterceptedException.Message}");
            if (exceptionCount > 5) FirefoxDriver.Quit();
            await RestartExtract(amountToExtract);
        }
        catch (WebDriverTimeoutException webDriverTimeoutException)
        {
            exceptionCount++;
            CatchList.Add(webDriverTimeoutException.Message);
            Console.WriteLine($"Total exception count: {exceptionCount}");
            Console.WriteLine($"Error Message: {webDriverTimeoutException.Message}");
            if (exceptionCount > 5) FirefoxDriver.Quit();
            await RestartExtract(amountToExtract);
        }
        catch (NoSuchWindowException noSuchWindowException)
        {
            exceptionCount++;
            CatchList.Add(noSuchWindowException.Message);
            Console.WriteLine($"Total exception count: {exceptionCount}");
            Console.WriteLine($"Error Message: {noSuchWindowException.Message}");
            if (exceptionCount > 5) FirefoxDriver.Quit();
            FirefoxDriver.Quit();
        }
        catch (Exception e)
        {
            exceptionCount++;
            CatchList.Add(e.Message);
            Console.WriteLine($"Total exception count: {exceptionCount}");
            Console.WriteLine($"Error Message: {e.Message}");
            if (exceptionCount > 5) FirefoxDriver.Quit();
            await RestartExtract(amountToExtract);
        }
        finally
        {
            CatchList.Clear();
            ReportTotals(FirefoxDriver, addressListIterationCount, addressListIterationTotal);
        }
    }
}
