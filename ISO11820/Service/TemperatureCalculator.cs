using ISO11820.Global;

namespace ISO11820.Services;

/// <summary>
/// 温度计算工具类
/// </summary>
public static class TemperatureCalculator
{
    /// <summary>
    /// 计算温升
    /// </summary>
    public static double CalculateTemperatureRise(double finalTemp, double ambientTemp)
    {
        return finalTemp - ambientTemp;
    }

    /// <summary>
    /// 计算失重率
    /// </summary>
    public static double CalculateWeightLossPercentage(double preWeight, double postWeight)
    {
        if (preWeight <= 0)
            return 0;
        return (preWeight - postWeight) / preWeight * 100.0;
    }

    /// <summary>
    /// 判断试验结果是否合格
    /// </summary>
    public static bool JudgeResult(double deltaTf, double weightLossPer, int flameDuration)
    {
        return deltaTf <= Constants.MaxTemperatureRise &&
               weightLossPer <= Constants.MaxWeightLossPercent &&
               flameDuration < Constants.MaxFlameDuration;
    }

    /// <summary>
    /// 获取判定结论文本
    /// </summary>
    public static string GetJudgmentText(double deltaTf, double weightLossPer, int flameDuration)
    {
        bool passed = JudgeResult(deltaTf, weightLossPer, flameDuration);
        return passed ? "合格" : "不合格";
    }

    /// <summary>
    /// 计算失重量
    /// </summary>
    public static double CalculateWeightLoss(double preWeight, double postWeight)
    {
        return preWeight - postWeight;
    }
}