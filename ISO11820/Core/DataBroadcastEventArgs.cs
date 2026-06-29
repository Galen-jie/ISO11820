namespace ISO11820.Core;

/// <summary>
/// 数据广播事件参数
/// 用于从后台线程向UI层传递温度数据和状态信息
/// </summary>
public class DataBroadcastEventArgs : EventArgs
{
    /// <summary>
    /// 炉温1（°C）
    /// </summary>
    public double Tf1 { get; set; }

    /// <summary>
    /// 炉温2（°C）
    /// </summary>
    public double Tf2 { get; set; }

    /// <summary>
    /// 表面温（°C）
    /// </summary>
    public double Ts { get; set; }

    /// <summary>
    /// 中心温（°C）
    /// </summary>
    public double Tc { get; set; }

    /// <summary>
    /// 校准温（°C）
    /// </summary>
    public double Tcal { get; set; }

    /// <summary>
    /// 已记录秒数（记录状态）
    /// </summary>
    public int ElapsedSeconds { get; set; }

    /// <summary>
    /// 加热阶段秒数（从开始加热计时）
    /// </summary>
    public int HeatingElapsedSeconds { get; set; }

    /// <summary>
    /// 当前状态
    /// </summary>
    public TestState CurrentState { get; set; }

    /// <summary>
    /// 温漂（°C/10min）
    /// </summary>
    public double TempDrift { get; set; }

    /// <summary>
    /// 系统消息列表
    /// </summary>
    public List<MasterMessage> Messages { get; set; } = new();

    /// <summary>
    /// PID输出值（恒功率模式）
    /// </summary>
    public double PidOutput { get; set; }
}