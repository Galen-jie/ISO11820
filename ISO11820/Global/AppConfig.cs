namespace ISO11820.Global;

/// <summary>
/// 应用程序配置映射类
/// </summary>
public class AppConfig
{
    public DatabaseConfig Database { get; set; } = new();
    public HardwareConfig Hardware { get; set; } = new();
    public SimulationConfig Simulation { get; set; } = new();
    public FileStorageConfig FileStorage { get; set; } = new();
    public ReportConfig Report { get; set; } = new();
    public LoggingConfig Logging { get; set; } = new();
    public TestDefaultsConfig TestDefaults { get; set; } = new();
}

public class DatabaseConfig
{
    public string Provider { get; set; } = "Sqlite";
    public string SqlitePath { get; set; } = "Data\\ISO11820.db";
}

public class HardwareConfig
{
    public int ConstPower { get; set; } = 2048;
    public double PidTemperature { get; set; } = 750;
    public string SensorProtocol { get; set; } = "ModbusRtu";
    public double MaxTemperatureDriftPerTenMinutes { get; set; } = 2.0;
}

public class SimulationConfig
{
    public bool EnableSimulation { get; set; } = true;
    public bool SimulateSensors { get; set; } = true;
    public bool SimulatePidController { get; set; } = true;
    public double InitialFurnaceTemp { get; set; } = 25.0;
    public double TargetFurnaceTemp { get; set; } = 750.0;
    public double HeatingRatePerSecond { get; set; } = 5.0;
    public double TempFluctuation { get; set; } = 0.5;
    public double StableThreshold { get; set; } = 3.0;
    public int StableCountThreshold { get; set; } = 3;
    public bool SimulateFlame { get; set; } = false;
}

public class FileStorageConfig
{
    public string BaseDirectory { get; set; } = "D:\\ISO11820";
    public string TestDataDirectory { get; set; } = "D:\\ISO11820\\TestData";
}

public class ReportConfig
{
    public string OutputDirectory { get; set; } = "D:\\ISO11820\\Reports";
    public bool EnablePdfExport { get; set; } = true;
    public bool EnableExcelExport { get; set; } = true;
    public bool EnableCsvExport { get; set; } = true;
}

public class LoggingConfig
{
    public string LogDirectory { get; set; } = "Data\\Logs";
    public string RollingInterval { get; set; } = "Day";
    public int RetainedFileCount { get; set; } = 30;
    public string MinimumLevel { get; set; } = "Debug";
}

public class TestDefaultsConfig
{
    public int StandardDuration { get; set; } = 3600;
    public string According { get; set; } = "ISO 11820:2022";
    public int CheckIntervalMinutes { get; set; } = 5;
}
