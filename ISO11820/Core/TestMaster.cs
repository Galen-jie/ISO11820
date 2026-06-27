using ISO11820.Global;
using ISO11820.Services;

namespace ISO11820.Core;

public class SensorDataPoint
{
    public int Time { get; set; }
    public double Tf1 { get; set; }
    public double Tf2 { get; set; }
    public double Ts { get; set; }
    public double Tc { get; set; }
    public double Tcal { get; set; }
}

public class TestMaster : IDisposable
{
    private TestState _state = TestState.Idle;
    private System.Threading.Timer? _workTimer;
    private readonly SensorSimulator _simulator;
    private readonly DaqWorker _daqWorker;

    private readonly List<SensorDataPoint> _tempHistory = new();
    private readonly List<double> _pidOutputHistory = new();

    public string? CurrentProductId { get; private set; }
    public string? CurrentTestId { get; private set; }
    public int TotalTestTime { get; private set; }
    public string Flag { get; set; } = "";
    public int TargetDuration { get; private set; } = 3600;

    private int _stableCounter = 0;
    private double _ambTemp = 25.0;
    private double _preWeight = 0;
    private int _constPower = 0;

    private double _maxTf1, _maxTf2, _maxTs, _maxTc;
    private int _maxTf1Time, _maxTf2Time, _maxTsTime, _maxTcTime;
    private double _finalTf1, _finalTf2, _finalTs, _finalTc;

    private string? _csvFilePath;
    private readonly List<MasterMessage> _pendingMessages = new();

    public event EventHandler<DataBroadcastEventArgs>? DataBroadcast;
    public event EventHandler<TestState>? StateChanged;

    public TestState CurrentState => _state;

    public TestMaster(SimulationConfig config)
    {
        _simulator = new SensorSimulator(config);
        _daqWorker = new DaqWorker(_simulator);
    }

    public void StartHeating()
    {
        if (_state != TestState.Idle)
            throw new InvalidOperationException("Cannot start heating in current state");

        _state = TestState.Preparing;
        _simulator.StartHeating();
        _stableCounter = 0;
        StartWorker();
        AddMessage("加热开始");
        OnStateChanged(_state);
    }

    public void StartRecording()
    {
        if (_state != TestState.Ready)
            throw new InvalidOperationException("Temperature not stable, cannot start recording");

        _state = TestState.Recording;
        _simulator.EnterRecordingMode();
        TotalTestTime = 0;
        _tempHistory.Clear();

        if (_pidOutputHistory.Count > 0)
            _constPower = (int)_pidOutputHistory.TakeLast(600).Average();

        if (!string.IsNullOrEmpty(CurrentProductId) && !string.IsNullOrEmpty(CurrentTestId))
        {
            var baseDir = MyAppContext.Instance.Config.FileStorage.TestDataDirectory;
            var dir = Path.Combine(baseDir, CurrentProductId, CurrentTestId);
            Directory.CreateDirectory(dir);
            _csvFilePath = Path.Combine(dir, "sensor_data.csv");
            using var writer = new StreamWriter(_csvFilePath);
            writer.WriteLine("Time,Temp1,Temp2,TempSurface,TempCenter,TempCalibration");
        }

        _maxTf1 = _maxTf2 = _maxTs = _maxTc = 0;
        _maxTf1Time = _maxTf2Time = _maxTsTime = _maxTcTime = 0;

        AddMessage("记录开始");
        OnStateChanged(_state);
    }

    public void StopRecording()
    {
        if (_state != TestState.Recording) return;
        _state = TestState.Complete;
        AddMessage("记录完成");
        StopWorker();
        OnStateChanged(_state);
    }

    public void StopHeating()
    {
        _simulator.StopHeating();
        if (_state == TestState.Preparing || _state == TestState.Ready || _state == TestState.Complete)
        {
            _state = TestState.Idle;
            StopWorker();
            AddMessage("加热停止");
            OnStateChanged(_state);
        }
    }

    public void ResetToIdle()
    {
        CurrentProductId = null;
        CurrentTestId = null;
        TotalTestTime = 0;
        Flag = "";
        _tempHistory.Clear();
        _pidOutputHistory.Clear();
        _csvFilePath = null;

        if (_state == TestState.Complete)
        {
            _state = TestState.Preparing;
            StartWorker();
        }
        else
        {
            _state = TestState.Idle;
        }
        OnStateChanged(_state);
    }

    public void CreateNewTest(string productId, string testId, double preWeight,
        double ambTemp, double ambHumi, string operatorName, int duration,
        string apparatusId, string apparatusName, DateTime apparatusChkDate)
    {
        CurrentProductId = productId;
        CurrentTestId = testId;
        TotalTestTime = 0;
        Flag = "";
        TargetDuration = duration;
        _ambTemp = ambTemp;
        _preWeight = preWeight;
        _constPower = MyAppContext.Instance.Config.Hardware.ConstPower;

        _simulator.Reset();

        var record = new Data.Models.TestMasterRecord
        {
            ProductId = productId,
            TestId = testId,
            AmbTemp = ambTemp,
            AmbHumi = ambHumi,
            Operator = operatorName,
            PreWeight = preWeight,
            According = MyAppContext.Instance.Config.TestDefaults.According,
            ApparatusId = apparatusId,
            ApparatusName = apparatusName,
            ApparatusChkDate = apparatusChkDate,
            RptNo = productId,
            ConstPower = _constPower
        };
        MyAppContext.Instance.DbHelper.InsertNewTest(record);
        AddMessage($"新建试验: {productId}-{testId}");
    }

    public void SaveTestRecord(double postWeight, bool hasFlame, int flameTime, int flameDuration, string? memo)
    {
        if (string.IsNullOrEmpty(CurrentProductId) || string.IsNullOrEmpty(CurrentTestId)) return;

        double lostWeightPer = (_preWeight - postWeight) / _preWeight * 100.0;

        _finalTf1 = _simulator.Tf1;
        _finalTf2 = _simulator.Tf2;
        _finalTs = _simulator.Ts;
        _finalTc = _simulator.Tc;

        double deltaTf1 = _finalTf1 - _ambTemp;
        double deltaTf2 = _finalTf2 - _ambTemp;
        double deltaTs = _finalTs - _ambTemp;
        double deltaTc = _finalTc - _ambTemp;
        double deltaTf = deltaTs;

        string phenoCode = hasFlame ? "1" : "0";

        MyAppContext.Instance.DbHelper.UpdateTestResult(
            CurrentProductId, CurrentTestId,
            postWeight, lostWeightPer, deltaTf,
            TotalTestTime, phenoCode, flameTime, flameDuration,
            _maxTf1, _maxTf2, _maxTs, _maxTc,
            _maxTf1Time, _maxTf2Time, _maxTsTime, _maxTcTime,
            _finalTf1, _finalTf2, _finalTs, _finalTc,
            deltaTf1, deltaTf2, deltaTs, deltaTc, memo);

        Flag = "10000000";
        AddMessage("试验记录已保存");
    }

    private void StartWorker()
    {
        if (_workTimer != null) return;
        _workTimer = new System.Threading.Timer(DoWork, null, 0, Constants.DaqIntervalMs);
    }

    private void StopWorker()
    {
        if (_workTimer != null)
        {
            _workTimer.Dispose();
            _workTimer = null;
        }
    }

    private void DoWork(object? state)
    {
        try
        {
            var temps = _daqWorker.ReadTemperatures();
            _pidOutputHistory.Add(temps.PidOutput);
            if (_pidOutputHistory.Count > 1000) _pidOutputHistory.RemoveAt(0);

            CheckStateTransition(temps);

            if (_state == TestState.Recording)
            {
                var dataPoint = new SensorDataPoint
                {
                    Time = TotalTestTime,
                    Tf1 = temps.Tf1, Tf2 = temps.Tf2,
                    Ts = temps.Ts, Tc = temps.Tc, Tcal = temps.Tcal
                };
                _tempHistory.Add(dataPoint);
                UpdateMaxValues(temps);
                SaveToCsv(dataPoint);
                TotalTestTime++;

                if (ShouldAutoStop())
                {
                    StopRecording();
                    AddMessage($"记录在 {TotalTestTime} 秒停止");
                }
            }

            double tempDrift = CalculateTemperatureDrift();
            BroadcastData(temps, tempDrift);
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "TestMaster.DoWork error");
        }
    }

    private void CheckStateTransition((double Tf1, double Tf2, double Ts, double Tc, double Tcal, double PidOutput) temps)
    {
        if (_state == TestState.Preparing)
        {
            bool inRange = temps.Tf1 >= Constants.StableTempMin && temps.Tf1 <= Constants.StableTempMax;
            if (inRange) _stableCounter++;
            else _stableCounter = 0;

            if (_stableCounter > MyAppContext.Instance.Config.Simulation.StableCountThreshold)
            {
                _state = TestState.Ready;
                AddMessage("温度稳定，可以开始记录");
                OnStateChanged(_state);
            }
        }

        if (_state == TestState.Ready)
        {
            bool inRange = temps.Tf1 >= Constants.StableTempMin && temps.Tf1 <= Constants.StableTempMax;
            if (!inRange)
            {
                _state = TestState.Preparing;
                _stableCounter = 0;
                AddMessage("温度波动，继续加热");
                OnStateChanged(_state);
            }
        }
    }

    private void UpdateMaxValues((double Tf1, double Tf2, double Ts, double Tc, double Tcal, double PidOutput) temps)
    {
        if (temps.Tf1 > _maxTf1) { _maxTf1 = temps.Tf1; _maxTf1Time = TotalTestTime; }
        if (temps.Tf2 > _maxTf2) { _maxTf2 = temps.Tf2; _maxTf2Time = TotalTestTime; }
        if (temps.Ts > _maxTs) { _maxTs = temps.Ts; _maxTsTime = TotalTestTime; }
        if (temps.Tc > _maxTc) { _maxTc = temps.Tc; _maxTcTime = TotalTestTime; }
    }

    private void SaveToCsv(SensorDataPoint point)
    {
        if (_csvFilePath == null) return;
        try
        {
            using var writer = new StreamWriter(_csvFilePath, append: true);
            writer.WriteLine($"{point.Time},{point.Tf1:F1},{point.Tf2:F1},{point.Ts:F1},{point.Tc:F1},{point.Tcal:F1}");
        }
        catch (Exception ex) { Serilog.Log.Error(ex, "CSV save error"); }
    }

    private double CalculateTemperatureDrift()
    {
        if (_tempHistory.Count < 100) return 0;
        var recent = _tempHistory.TakeLast(Constants.MaxHistoryPoints).ToList();
        double[] x = recent.Select((p, i) => (double)i).ToArray();
        double[] y = recent.Select(p => p.Tf1).ToArray();
        var (slope, _) = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(x, y);
        return slope * Constants.MaxHistoryPoints;
    }

    private bool ShouldAutoStop()
    {
        if (TargetDuration > 0) return TotalTestTime >= TargetDuration;
        if (TotalTestTime >= Constants.StandardTestDuration) return true;

        if (TotalTestTime % (MyAppContext.Instance.Config.TestDefaults.CheckIntervalMinutes * 60) == 0)
        {
            double tempDrift = CalculateTemperatureDrift();
            if (Math.Abs(tempDrift) < MyAppContext.Instance.Config.Hardware.MaxTemperatureDriftPerTenMinutes)
            {
                AddMessage("达到终止条件", MessageType.Warning);
                return true;
            }
        }
        return false;
    }

    private void BroadcastData((double Tf1, double Tf2, double Ts, double Tc, double Tcal, double PidOutput) temps, double tempDrift)
    {
        var args = new DataBroadcastEventArgs
        {
            Tf1 = temps.Tf1, Tf2 = temps.Tf2, Ts = temps.Ts, Tc = temps.Tc, Tcal = temps.Tcal,
            ElapsedSeconds = TotalTestTime, CurrentState = _state, TempDrift = tempDrift, PidOutput = temps.PidOutput
        };
        args.Messages.AddRange(_pendingMessages);
        _pendingMessages.Clear();
        DataBroadcast?.Invoke(this, args);
    }

    private void AddMessage(string message, MessageType type = MessageType.Normal)
    {
        _pendingMessages.Add(new MasterMessage { Time = DateTime.Now.ToString("HH:mm:ss"), Message = message, Type = type });
    }

    private void OnStateChanged(TestState newState) { StateChanged?.Invoke(this, newState); }

    public List<SensorDataPoint> GetTemperatureHistory() => _tempHistory.ToList();
    public bool HasUnsavedTest() => TotalTestTime > 0 && Flag != Constants.TestCompleteFlag;
    public (string? ProductId, string? TestId, int TotalTime, double PreWeight) GetCurrentTestInfo() => (CurrentProductId, CurrentTestId, TotalTestTime, _preWeight);

    public void Dispose() { StopWorker(); _workTimer?.Dispose(); }
}