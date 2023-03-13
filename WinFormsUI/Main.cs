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
        services.AddSingleton<IDataContext>(new DataContext(azureSqlDbConnectionString));
        services.AddSingleton(new BlobService(azureBlobStorageConnectionString));
        services.AddWindowsFormsBlazorWebView();
        services.AddBlazorWebViewDeveloperTools();
        services.AddTransient<ExceptionLogDataServiceFactory>();
        services.AddTransient<BACIDataServiceFactory>();
        services.AddTransient<BACIExtractor>();
        services.AddSingleton<DriverPathSettings>();
        services.AddAutoMapper(typeof(DataLibrary.AutoMapperEntryPoint).Assembly);
        var mapper = new MapperConfiguration(options =>
        {
            options.AddProfile<AddressProfile>();
        });
        blazorWebView.HostPage = "wwwroot\\index.html";
        blazorWebView.Services = services.BuildServiceProvider();
        blazorWebView.RootComponents.Add<App>("#app");
    }
}