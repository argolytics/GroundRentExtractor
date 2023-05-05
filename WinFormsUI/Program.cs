using Microsoft.Extensions.Configuration;
using Serilog;

namespace WinFormsUI;

internal static class Program
{
    public static IConfigurationRoot Configuration { get; private set; }
    [STAThread]
    static void Main()
    {
        var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        Configuration = builder.Build();
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration)
            .WriteTo.Console()
            .CreateLogger();
        ApplicationConfiguration.Initialize();
        Application.Run(new Main());
    }
}