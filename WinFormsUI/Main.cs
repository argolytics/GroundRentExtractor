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
        services.Configure<WebPageStringSettings>(Program.Configuration.GetSection("WebPageStringSettings"));
        services.Configure<RazorPageSettings>(Program.Configuration.GetSection("RazorPageSettings"));
        services.AddWindowsFormsBlazorWebView();
        services.AddBlazorWebViewDeveloperTools();
        // Extractor, ExtractController, and Data service factories
        services.AddTransient<IExceptionLogDataServiceFactory, ExceptionLogDataServiceFactory>();
        services.AddTransient<ExtractController>();
        services.AddTransient<Extractor>();

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