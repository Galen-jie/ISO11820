using Microsoft.Extensions.Configuration;
using ISO11820.Data;
using ISO11820.Core;

namespace ISO11820.Global;

public sealed class MyAppContext
{
    private static readonly Lazy<MyAppContext> _instance = new(() => new MyAppContext());
    public static MyAppContext Instance => _instance.Value;

    public AppConfig Config { get; private set; } = null!;
    public DbHelper DbHelper { get; private set; } = null!;
    public TestMaster? CurrentTestMaster { get; set; }
    public string CurrentOperator { get; set; } = "";
    public string CurrentOperatorType { get; set; } = "";

    private MyAppContext() { }

    public void Initialize(IConfiguration configuration)
    {
        Config = configuration.Get<AppConfig>() ?? new AppConfig();

        var dbPath = Config.Database.SqlitePath;
        var dbDir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dbDir)) Directory.CreateDirectory(dbDir);

        DbHelper = new DbHelper(dbPath);
        DatabaseInitializer.Initialize(DbHelper);

        Directory.CreateDirectory(Config.FileStorage.BaseDirectory);
        Directory.CreateDirectory(Config.FileStorage.TestDataDirectory);
        Directory.CreateDirectory(Config.Report.OutputDirectory);
        Directory.CreateDirectory(Config.Logging.LogDirectory);
    }
}
