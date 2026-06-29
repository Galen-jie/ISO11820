namespace ISO11820.Global;

/// <summary>
/// 常量定义
/// </summary>
public static class Constants
{
    // 温度相关常量
    public const double TargetFurnaceTemp = 750.0;
    public const double StableTempMin = 745.0;
    public const double StableTempMax = 755.0;

    // 时间相关常量
    public const int StandardTestDuration = 3600; // 60分钟（秒）
    public const int DaqIntervalMs = 800; // 数据采集间隔800ms
    public const int MaxHistoryPoints = 600; // 最大历史数据点（10分钟）

    // 状态标记
    public const string TestCompleteFlag = "10000000"; // 试验已保存完成标记

    // 判定标准
    public const double MaxWeightLossPercent = 50.0; // 最大失重率%
    public const double MaxTemperatureRise = 50.0; // 最大温升°C
    public const int MaxFlameDuration = 5; // 最大火焰持续时间秒

    // 默认账号
    public const string DefaultAdminUser = "admin";
    public const string DefaultAdminPassword = "123456";
    public const string DefaultOperatorUser = "experimenter";
    public const string DefaultOperatorPassword = "123456";
}
