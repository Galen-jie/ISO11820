namespace ISO11820.Data.Models;

/// <summary>
/// 传感器配置模型
/// </summary>
public class Sensor
{
    public int SensorId { get; set; }
    public string SensorName { get; set; } = "";
    public string DispName { get; set; } = "";
    public string SensorGroup { get; set; } = "";
    public string Unit { get; set; } = "℃";
    public string Description { get; set; } = "";
    public string Flag { get; set; } = "启用";
    public double SignalZero { get; set; }
    public double SignalSpan { get; set; }
    public double OutputZero { get; set; }
    public double OutputSpan { get; set; }
    public double OutputValue { get; set; }
    public double InputValue { get; set; }
    public int SignalType { get; set; } // 4=数字量
}