using Microsoft.Extensions.Configuration;
using Serilog;
using ISO11820.Forms;
using ISO11820.Global;

namespace ISO11820;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // 加载配置文件
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // 初始化全局上下文
        MyAppContext.Instance.Initialize(config);

        // 配置 Serilog 日志
        var logDir = MyAppContext.Instance.Config.Logging.LogDirectory;
        Directory.CreateDirectory(logDir);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(logDir, "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: MyAppContext.Instance.Config.Logging.RetainedFileCount,
                outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Log.Information("System started");
            Application.Run(new LoginForm());
            Log.Information("System shutdown");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application crashed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}