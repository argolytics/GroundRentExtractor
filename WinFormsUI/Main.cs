using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using DataLibrary.DbAccess;
using DataLibrary.DbServices;
using DataLibrary.Services;
using DataLibrary.AutoMapperProfiles;
using AutoMapper;
using DataLibrary.Settings;

namespace WinFormsUI;

public partial class Main : Form
{
    public Main()
    {
        InitializeComponent();
        var azureSqlDbConnectionString = Program.Configuration.GetConnectionString("AzureSqlDb");
        var azureBlobStorageConnectionString = Program.Configuration.GetConnectionString("AzureBlobStorage");
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(_ => Program.Configuration);
        services.AddSingleton<IDataContext>(new DataContext(azureSqlDbConnectionString));
        services.AddSingleton(new BlobService(azureBlobStorageConnectionString));
        services.Configure<DriverPathSettings>(Program.Configuration.GetSection("DriverPathSettings"));
        services.Configure<BlobSettings>(Program.Configuration.GetSection("BlobSettings"));
        services.AddWindowsFormsBlazorWebView();
        services.AddBlazorWebViewDeveloperTools();
        // Data service factories
        services.AddTransient<ExceptionLogDataServiceFactory>();
        services.AddTransient<ALLEDataServiceFactory>();
        services.AddTransient<ANNEDataServiceFactory>();
        services.AddTransient<BACIDataServiceFactory>();
        services.AddTransient<BACODataServiceFactory>();
        services.AddTransient<CALVDataServiceFactory>();
        services.AddTransient<CARODataServiceFactory>();
        services.AddTransient<CARRDataServiceFactory>();
        services.AddTransient<CECIDataServiceFactory>();
        services.AddTransient<CHARDataServiceFactory>();
        services.AddTransient<DORCDataServiceFactory>();
        services.AddTransient<FREDDataServiceFactory>();
        services.AddTransient<GARRDataServiceFactory>();
        services.AddTransient<HARFDataServiceFactory>();
        services.AddTransient<HOWADataServiceFactory>();
        services.AddTransient<KENTDataServiceFactory>();
        services.AddTransient<MONTDataServiceFactory>();
        services.AddTransient<PRINDataServiceFactory>();
        services.AddTransient<QUEEDataServiceFactory>();
        services.AddTransient<SOMEDataServiceFactory>();
        services.AddTransient<STMADataServiceFactory>();
        services.AddTransient<TALBDataServiceFactory>();
        services.AddTransient<WASHDataServiceFactory>();
        services.AddTransient<WICODataServiceFactory>();
        services.AddTransient<WORCDataServiceFactory>();
        // Extractors
        services.AddTransient<ALLEExtractor>();
        services.AddTransient<ANNEExtractor>();
        services.AddTransient<BACIExtractor>();
        services.AddTransient<BACOExtractor>();
        services.AddTransient<CALVExtractor>();
        services.AddTransient<CAROExtractor>();
        services.AddTransient<CARRExtractor>();
        services.AddTransient<CECIExtractor>();
        services.AddTransient<CHARExtractor>();
        services.AddTransient<DORCExtractor>();
        services.AddTransient<FREDExtractor>();
        services.AddTransient<GARRExtractor>();
        services.AddTransient<HARFExtractor>();
        services.AddTransient<HOWAExtractor>();
        services.AddTransient<KENTExtractor>();
        services.AddTransient<MONTExtractor>();
        services.AddTransient<PRINExtractor>();
        services.AddTransient<QUEEExtractor>();
        services.AddTransient<SOMEExtractor>();
        services.AddTransient<STMAExtractor>();
        services.AddTransient<TALBExtractor>();
        services.AddTransient<WASHExtractor>();
        services.AddTransient<WICOExtractor>();
        services.AddTransient<WORCExtractor>();
        // Extractor Controllers
        services.AddSingleton<ALLEExtractController>();
        services.AddSingleton<ANNEExtractController>();
        services.AddSingleton<BACIExtractController>();
        services.AddSingleton<BACOExtractController>();
        services.AddSingleton<CALVExtractController>();
        services.AddSingleton<CAROExtractController>();
        services.AddSingleton<CARRExtractController>();
        services.AddSingleton<CECIExtractController>();
        services.AddSingleton<CHARExtractController>();
        services.AddSingleton<DORCExtractController>();
        services.AddSingleton<FREDExtractController>();
        services.AddSingleton<GARRExtractController>();
        services.AddSingleton<HARFExtractController>();
        services.AddSingleton<HOWAExtractController>();
        services.AddSingleton<KENTExtractController>();
        services.AddSingleton<MONTExtractController>();
        services.AddSingleton<PRINExtractController>();
        services.AddSingleton<QUEEExtractController>();
        services.AddSingleton<SOMEExtractController>();
        services.AddSingleton<STMAExtractController>();
        services.AddSingleton<TALBExtractController>();
        services.AddSingleton<WASHExtractController>();
        services.AddSingleton<WICOExtractController>();
        services.AddSingleton<WORCExtractController>();
        services.AddAutoMapper(typeof(DataLibrary.AutoMapperEntryPoint).Assembly);
        var mapper = new MapperConfiguration(options =>
        {
            options.AddProfile<AddressProfile>();
        });
        var executingDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        blazorWebView.HostPage = Path.Combine(executingDir, "wwwroot\\index.html");
        blazorWebView.Services = services.BuildServiceProvider();
        blazorWebView.RootComponents.Add<App>("#app");
    }
}