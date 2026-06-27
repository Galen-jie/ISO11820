using ISO11820.Global;
using ISO11820.Services;

namespace ISO11820.Services;

public class SensorSimulator
{
    private readonly SimulationConfig _config;
    private readonly Random _random = new();

    public double Tf1 { get; private set; }
    public double Tf2 { get; private set; }
    public double Ts { get; private set; }
    public double Tc { get; private set; }
    public double Tcal { get; private set; }
    public double PidOutput { get; private set; }

    private bool _isHeating = false;
    private bool _isRecording = false;

    public SensorSimulator(SimulationConfig config)
    {
        _config = config;
        Reset();
    }

    public void Reset()
    {
        Tf1 = _config.InitialFurnaceTemp;
        Tf2 = _config.InitialFurnaceTemp;
        Ts = Tf1 * 0.3;
        Tc = Tf1 * 0.25;
        Tcal = Tf1;
        PidOutput = 0;
        _isHeating = false;
        _isRecording = false;
    }

    public void StartHeating()
    {
        _isHeating = true;
        _isRecording = false;
    }

    public void StopHeating()
    {
        _isHeating = false;
        _isRecording = false;
    }

    public void EnterRecordingMode()
    {
        _isRecording = true;
    }

    public void Update()
    {
        double noise = (_random.NextDouble() * 2 - 1) * _config.TempFluctuation;

        if (_isHeating)
        {
            if (Tf1 < _config.TargetFurnaceTemp - _config.StableThreshold)
            {
                double heatingRate = _config.HeatingRatePerSecond * 0.8;
                Tf1 += heatingRate + noise;
                Tf2 += heatingRate + noise;
                Ts = Tf1 * 0.3 + noise;
                Tc = Tf1 * 0.25 + noise;
                PidOutput = 25600;
            }
            else
            {
                Tf1 = _config.TargetFurnaceTemp + noise;
                Tf2 = _config.TargetFurnaceTemp + noise;

                if (_isRecording)
                {
                    double surfaceTarget = Math.Min(Tf1 * 0.95, 800);
                    double centerTarget = Math.Min(Tf1 * 0.85, 750);
                    Ts += (surfaceTarget - Ts) * 0.02 + noise;
                    Tc += (centerTarget - Tc) * 0.01 + noise;
                }
                else
                {
                    Ts = Tf1 * 0.3 + noise;
                    Tc = Tf1 * 0.25 + noise;
                }
                PidOutput = MyAppContext.Instance.Config.Hardware.ConstPower + noise * 10;
            }
        }
        else
        {
            Tf1 -= 0.5 + noise * 0.1;
            Tf2 -= 0.5 + noise * 0.1;
            Ts = Tf1 * 0.3 + noise;
            Tc = Tf1 * 0.25 + noise;
            PidOutput = 0;
        }

        Tcal = Tf1 + noise * 2;
        Tf1 = Math.Max(25, Tf1);
        Tf2 = Math.Max(25, Tf2);
        Ts = Math.Max(25, Ts);
        Tc = Math.Max(25, Tc);
        Tcal = Math.Max(25, Tcal);
        PidOutput = Math.Max(0, Math.Min(25600, PidOutput));
    }
}