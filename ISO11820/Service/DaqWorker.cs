namespace ISO11820.Services;

/// <summary>
/// 数据采集服务
/// 每800ms从仿真引擎或真实传感器读取温度数据
/// </summary>
public class DaqWorker
{
    private readonly SensorSimulator _simulator;

    public DaqWorker(SensorSimulator simulator)
    {
        _simulator = simulator;
    }

    /// <summary>
    /// 读取温度数据
    /// 返回：(Tf1, Tf2, Ts, Tc, Tcal, PidOutput)
    /// </summary>
    public (double Tf1, double Tf2, double Ts, double Tc, double Tcal, double PidOutput) ReadTemperatures()
    {
        // 仿真模式：调用仿真器更新并返回数据
        _simulator.Update();

        return (
            _simulator.Tf1,
            _simulator.Tf2,
            _simulator.Ts,
            _simulator.Tc,
            _simulator.Tcal,
            _simulator.PidOutput
        );
    }
}