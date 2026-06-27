using Microsoft.Data.Sqlite;
using ISO11820.Data.Models;

namespace ISO11820.Data;

/// <summary>
/// SQLite数据库操作封装类
/// </summary>
public class DbHelper
{
    private readonly string _connectionString;

    public DbHelper(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }

    private SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    #region 登录验证

    /// <summary>
    /// 验证登录
    /// </summary>
    public bool ValidateLogin(string username, string password)
    {
        using var conn = CreateConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(1) FROM operators
            WHERE username = $username AND pwd = $pwd";
        cmd.Parameters.AddWithValue("$username", username);
        cmd.Parameters.AddWithValue("$pwd", password);
        var result = cmd.ExecuteScalar();
        return Convert.ToInt32(result) > 0;
    }

    /// <summary>
    /// 获取操作员类型
    /// </summary>
    public string GetOperatorType(string username)
    {
        using var conn = CreateConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT usertype FROM operators WHERE username = $username";
        cmd.Parameters.AddWithValue("$username", username);
        var result = cmd.ExecuteScalar();
        return result?.ToString() ?? "";
    }

    /// <summary>
    /// 获取所有操作员用户名列表
    /// </summary>
    public List<string> GetAllOperatorNames()
    {
        var list = new List<string>();
        using var conn = CreateConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT username FROM operators ORDER BY username";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(reader.GetString(0));
        }
        return list;
    }

    #endregion

    #region 设备信息

    /// <summary>
    /// 获取设备信息
    /// </summary>
    public Apparatus? GetApparatus(int apparatusId)
    {
        using var conn = CreateConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM apparatus WHERE apparatusid = $id";
        cmd.Parameters.AddWithValue("$id", apparatusId);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Apparatus
            {
                ApparatusId = reader.GetInt32(0),
                InnerNumber = reader.GetString(1),
                ApparatusName = reader.GetString(2),
                CheckDateF = DateTime.Parse(reader.GetString(3)),
                CheckDateT = DateTime.Parse(reader.GetString(4)),
                PidPort = reader.GetString(5),
                PowerPort = reader.GetString(6),
                ConstPower = reader.IsDBNull(7) ? null : reader.GetInt32(7)
            };
        }
        return null;
    }

    #endregion

    #region 样品信息

    /// <summary>
    /// 检查样品是否存在
    /// </summary>
    public bool ProductExists(string productId)
    {
        using var conn = CreateConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM productmaster WHERE productid = $pid";
        cmd.Parameters.AddWithValue("$pid", productId);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    /// <summary>
    /// 插入样品信息
    /// </summary>
    public void InsertProduct(ProductMaster product)
    {
        using var conn = CreateConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO productmaster (productid, productname, specific, diameter, height, flag)
            VALUES ($pid, $pname, $spec, $dia, $height, $flag)";
        cmd.Parameters.AddWithValue("$pid", product.ProductId);
        cmd.Parameters.AddWithValue("$pname", product.ProductName);
        cmd.Parameters.AddWithValue("$spec", product.Specific);
        cmd.Parameters.AddWithValue("$dia", product.Diameter);
        cmd.Parameters.AddWithValue("$height", product.Height);
        cmd.Parameters.AddWithValue("$flag", product.Flag ?? "");
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 获取样品信息
    /// </summary>
    public ProductMaster? GetProduct(string productId)
    {
        using var conn = CreateConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM productmaster WHERE productid = $pid";
        cmd.Parameters.AddWithValue("$pid", productId);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new ProductMaster
            {
                ProductId = reader.GetString(0),
                ProductName = reader.GetString(1),
                Specific = reader.GetString(2),
                Diameter = reader.GetDouble(3),
                Height = reader.GetDouble(4),
                Flag = reader.IsDBNull(5) ? null : reader.GetString(5)
            };
        }
        return null;
    }

    #endregion

    #region 试验记录

    /// <summary>
    /// 插入新试验（初始记录）
    /// </summary>
    public void InsertNewTest(TestMasterRecord record)
    {
        using var conn = CreateConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO testmaster (
                productid, testid, testdate, ambtemp, ambhumi, according,
                operator, apparatusid, apparatusname, apparatuschkdate, rptno,
                preweight, postweight, lostweight, lostweight_per,
                totaltesttime, constpower, phenocode, flametime, flameduration,
                maxtf1, maxtf2, maxts, maxtc,
                maxtf1_time, maxtf2_time, maxts_time, maxtc_time,
                finaltf1, finaltf2, finalts, finaltc,
                finaltf1_time, finaltf2_time, finalts_time, finaltc_time,
                deltatf1, deltatf2, deltatf, deltats, deltatc, memo, flag
            ) VALUES (
                $productid, $testid, date('now'), $ambtemp, $ambhumi, $according,
                $operator, $apparatusid, $apparatusname, $apparatuschkdate, $rptno,
                $preweight, 0, 0, 0,
                0, $constpower, '', 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0, 0, $memo, ''
            )";
        cmd.Parameters.AddWithValue("$productid", record.ProductId);
        cmd.Parameters.AddWithValue("$testid", record.TestId);
        cmd.Parameters.AddWithValue("$ambtemp", record.AmbTemp);
        cmd.Parameters.AddWithValue("$ambhumi", record.AmbHumi);
        cmd.Parameters.AddWithValue("$according", record.According);
        cmd.Parameters.AddWithValue("$operator", record.Operator);
        cmd.Parameters.AddWithValue("$apparatusid", record.ApparatusId);
        cmd.Parameters.AddWithValue("$apparatusname", record.ApparatusName);
        cmd.Parameters.AddWithValue("$apparatuschkdate", record.ApparatusChkDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$rptno", record.RptNo);
        cmd.Parameters.AddWithValue("$preweight", record.PreWeight);
        cmd.Parameters.AddWithValue("$constpower", record.ConstPower);
        cmd.Parameters.AddWithValue("$memo", record.Memo ?? "");
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 更新试验结果（完成后）
    /// </summary>
    public void UpdateTestResult(string productId, string testId,
        double postWeight, double lostWeightPer, double deltaTf,
        int totalTime, string phenoCode, int flameTime, int flameDuration,
        double maxTf1, double maxTf2, double maxTs, double maxTc,
        int maxTf1Time, int maxTf2Time, int maxTsTime, int maxTcTime,
        double finalTf1, double finalTf2, double finalTs, double finalTc,
        double deltaTf1, double deltaTf2, double deltaTs, double deltaTc,
        string? memo = null)
    {
        using var conn = CreateConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE testmaster SET
                postweight = $postweight,
                lostweight = preweight - $postweight,
                lostweight_per = $lostweightPer,
                deltatf = $deltaTf,
                totaltesttime = $totalTime,
                phenocode = $phenoCode,
                flametime = $flameTime,
                flameduration = $flameDuration,
                maxtf1 = $maxTf1, maxtf2 = $maxTf2, maxts = $maxTs, maxtc = $maxTc,
                maxtf1_time = $maxTf1Time, maxtf2_time = $maxTf2Time,
                maxts_time = $maxTsTime, maxtc_time = $maxTcTime,
                finaltf1 = $finalTf1, finaltf2 = $finalTf2,
                finalts = $finalTs, finaltc = $finalTc,
                deltatf1 = $deltaTf1, deltatf2 = $deltaTf2,
                deltats = $deltaTs, deltatc = $deltaTc,
                memo = COALESCE($memo, memo),
                flag = '10000000'
            WHERE productid = $productid AND testid = $testid";
        cmd.Parameters.AddWithValue("$productid", productId);
        cmd.Parameters.AddWithValue("$testid", testId);
        cmd.Parameters.AddWithValue("$postweight", postWeight);
        cmd.Parameters.AddWithValue("$lostweightPer", lostWeightPer);
        cmd.Parameters.AddWithValue("$deltaTf", deltaTf);
        cmd.Parameters.AddWithValue("$totalTime", totalTime);
        cmd.Parameters.AddWithValue("$phenoCode", phenoCode);
        cmd.Parameters.AddWithValue("$flameTime", flameTime);
        cmd.Parameters.AddWithValue("$flameDuration", flameDuration);
        cmd.Parameters.AddWithValue("$maxTf1", maxTf1);
        cmd.Parameters.AddWithValue("$maxTf2", maxTf2);
        cmd.Parameters.AddWithValue("$maxTs", maxTs);
        cmd.Parameters.AddWithValue("$maxTc", maxTc);
        cmd.Parameters.AddWithValue("$maxTf1Time", maxTf1Time);
        cmd.Parameters.AddWithValue("$maxTf2Time", maxTf2Time);
        cmd.Parameters.AddWithValue("$maxTsTime", maxTsTime);
        cmd.Parameters.AddWithValue("$maxTcTime", maxTcTime);
        cmd.Parameters.AddWithValue("$finalTf1", finalTf1);
        cmd.Parameters.AddWithValue("$finalTf2", finalTf2);
        cmd.Parameters.AddWithValue("$finalTs", finalTs);
        cmd.Parameters.AddWithValue("$finalTc", finalTc);
        cmd.Parameters.AddWithValue("$deltaTf1", deltaTf1);
        cmd.Parameters.AddWithValue("$deltaTf2", deltaTf2);
        cmd.Parameters.AddWithValue("$deltaTs", deltaTs);
        cmd.Parameters.AddWithValue("$deltaTc", deltaTc);
        cmd.Parameters.AddWithValue("$memo", memo ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 获取试验记录
    /// </summary>
    public TestMasterRecord? GetTest(string productId, string testId)
    {
        using var conn = CreateConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT t.*, p.productname
            FROM testmaster t
            LEFT JOIN productmaster p ON t.productid = p.productid
            WHERE t.productid = $pid AND t.testid = $tid";
        cmd.Parameters.AddWithValue("$pid", productId);
        cmd.Parameters.AddWithValue("$tid", testId);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapTestMasterRecord(reader);
        }
        return null;
    }

    /// <summary>
    /// 查询试验列表（历史记录）
    /// </summary>
    public List<TestMasterRecord> QueryTests(DateTime? fromDate, DateTime? toDate,
        string? productId, string? operatorName)
    {
        var results = new List<TestMasterRecord>();
        using var conn = CreateConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT t.*, p.productname
            FROM testmaster t
            LEFT JOIN productmaster p ON t.productid = p.productid
            WHERE 1=1
              AND ($fromDate IS NULL OR t.testdate >= $fromDate)
              AND ($toDate IS NULL OR t.testdate <= $toDate)
              AND ($productId IS NULL OR t.productid LIKE '%' || $productId || '%')
              AND ($operator IS NULL OR t.operator = $operator)
            ORDER BY t.testdate DESC";
        cmd.Parameters.AddWithValue("$fromDate", fromDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$toDate", toDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$productId", productId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$operator", operatorName ?? (object)DBNull.Value);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(MapTestMasterRecord(reader));
        }
        return results;
    }

    /// <summary>
    /// 检查试验是否已保存完成
    /// </summary>
    public bool IsTestCompleted(string productId, string testId)
    {
        using var conn = CreateConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT flag FROM testmaster
            WHERE productid = $pid AND testid = $tid";
        cmd.Parameters.AddWithValue("$pid", productId);
        cmd.Parameters.AddWithValue("$tid", testId);
        var result = cmd.ExecuteScalar();
        return result?.ToString() == "10000000";
    }

    private TestMasterRecord MapTestMasterRecord(SqliteDataReader reader)
    {
        return new TestMasterRecord
        {
            ProductId = reader.GetString(0),
            TestId = reader.GetString(1),
            TestDate = DateTime.Parse(reader.GetString(2)),
            AmbTemp = reader.GetDouble(3),
            AmbHumi = reader.GetDouble(4),
            According = reader.GetString(5),
            Operator = reader.GetString(6),
            ApparatusId = reader.GetString(7),
            ApparatusName = reader.GetString(8),
            ApparatusChkDate = DateTime.Parse(reader.GetString(9)),
            RptNo = reader.GetString(10),
            PreWeight = reader.GetDouble(11),
            PostWeight = reader.GetDouble(12),
            LostWeight = reader.GetDouble(13),
            LostWeightPer = reader.GetDouble(14),
            TotalTestTime = reader.GetInt32(15),
            ConstPower = reader.GetInt32(16),
            PhenoCode = reader.GetString(17),
            FlameTime = reader.GetInt32(18),
            FlameDuration = reader.GetInt32(19),
            MaxTf1 = reader.GetDouble(20),
            MaxTf2 = reader.GetDouble(21),
            MaxTs = reader.GetDouble(22),
            MaxTc = reader.GetDouble(23),
            MaxTf1Time = reader.GetInt32(24),
            MaxTf2Time = reader.GetInt32(25),
            MaxTsTime = reader.GetInt32(26),
            MaxTcTime = reader.GetInt32(27),
            FinalTf1 = reader.GetDouble(28),
            FinalTf2 = reader.GetDouble(29),
            FinalTs = reader.GetDouble(30),
            FinalTc = reader.GetDouble(31),
            FinalTf1Time = reader.GetInt32(32),
            FinalTf2Time = reader.GetInt32(33),
            FinalTsTime = reader.GetInt32(34),
            FinalTcTime = reader.GetInt32(35),
            DeltaTf1 = reader.GetDouble(36),
            DeltaTf2 = reader.GetDouble(37),
            DeltaTf = reader.GetDouble(38),
            DeltaTs = reader.GetDouble(39),
            DeltaTc = reader.GetDouble(40),
            Memo = reader.IsDBNull(41) ? null : reader.GetString(41),
            Flag = reader.IsDBNull(42) ? null : reader.GetString(42),
            ProductName = reader.IsDBNull(43) ? "" : reader.GetString(43)
        };
    }

    #endregion

    #region 校准记录

    /// <summary>
    /// 插入校准记录
    /// </summary>
    public void InsertCalibrationRecord(CalibrationRecord record)
    {
        using var conn = CreateConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO CalibrationRecords (
                Id, CalibrationDate, CalibrationType, ApparatusId, Operator,
                TemperatureData, UniformityResult, MaxDeviation, AverageTemperature,
                PassedCriteria, Remarks, CreatedAt,
                TempA1, TempA2, TempA3, TempB1, TempB2, TempB3,
                TempC1, TempC2, TempC3,
                TAvg, TAvgAxis1, TAvgAxis2, TAvgAxis3,
                TAvgLevela, TAvgLevelb, TAvgLevelc,
                TDevAxis1, TDevAxis2, TDevAxis3,
                TDevLevela, TDevLevelb, TDevLevelc,
                TAvgDevAxis, TAvgDevLevel, CenterTempData, Memo
            ) VALUES (
                $id, $date, $type, $apparatusId, $operator,
                $tempData, $uniformity, $maxDev, $avgTemp,
                $passed, $remarks, $createdAt,
                $tempA1, $tempA2, $tempA3, $tempB1, $tempB2, $tempB3,
                $tempC1, $tempC2, $tempC3,
                $tAvg, $tAvg1, $tAvg2, $tAvg3,
                $tAvgA, $tAvgB, $tAvgC,
                $tDev1, $tDev2, $tDev3,
                $tDevA, $tDevB, $tDevC,
                $tAvgDevAxis, $tAvgDevLevel, $centerData, $memo
            )";
        cmd.Parameters.AddWithValue("$id", record.Id);
        cmd.Parameters.AddWithValue("$date", record.CalibrationDate);
        cmd.Parameters.AddWithValue("$type", record.CalibrationType);
        cmd.Parameters.AddWithValue("$apparatusId", record.ApparatusId);
        cmd.Parameters.AddWithValue("$operator", record.Operator);
        cmd.Parameters.AddWithValue("$tempData", record.TemperatureData);
        cmd.Parameters.AddWithValue("$uniformity", record.UniformityResult ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$maxDev", record.MaxDeviation ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$avgTemp", record.AverageTemperature ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$passed", record.PassedCriteria);
        cmd.Parameters.AddWithValue("$remarks", record.Remarks);
        cmd.Parameters.AddWithValue("$createdAt", record.CreatedAt);
        cmd.Parameters.AddWithValue("$tempA1", record.TempA1 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tempA2", record.TempA2 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tempA3", record.TempA3 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tempB1", record.TempB1 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tempB2", record.TempB2 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tempB3", record.TempB3 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tempC1", record.TempC1 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tempC2", record.TempC2 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tempC3", record.TempC3 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tAvg", record.TAvg ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tAvg1", record.TAvgAxis1 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tAvg2", record.TAvgAxis2 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tAvg3", record.TAvgAxis3 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tAvgA", record.TAvgLevela ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tAvgB", record.TAvgLevelb ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tAvgC", record.TAvgLevelc ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tDev1", record.TDevAxis1 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tDev2", record.TDevAxis2 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tDev3", record.TDevAxis3 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tDevA", record.TDevLevela ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tDevB", record.TDevLevelb ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tDevC", record.TDevLevelc ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tAvgDevAxis", record.TAvgDevAxis ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$tAvgDevLevel", record.TAvgDevLevel ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$centerData", record.CenterTempData ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$memo", record.Memo ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 查询校准记录列表
    /// </summary>
    public List<CalibrationRecord> QueryCalibrationRecords(DateTime? fromDate, DateTime? toDate)
    {
        var results = new List<CalibrationRecord>();
        using var conn = CreateConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT * FROM CalibrationRecords
            WHERE ($fromDate IS NULL OR CalibrationDate >= $fromDate)
              AND ($toDate IS NULL OR CalibrationDate <= $toDate)
            ORDER BY CalibrationDate DESC";
        cmd.Parameters.AddWithValue("$fromDate", fromDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$toDate", toDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(MapCalibrationRecord(reader));
        }
        return results;
    }

    private CalibrationRecord MapCalibrationRecord(SqliteDataReader reader)
    {
        return new CalibrationRecord
        {
            Id = reader.GetString(0),
            CalibrationDate = reader.GetString(1),
            CalibrationType = reader.GetString(2),
            ApparatusId = reader.GetInt32(3),
            Operator = reader.GetString(4),
            TemperatureData = reader.GetString(5),
            UniformityResult = reader.IsDBNull(6) ? null : reader.GetDouble(6),
            MaxDeviation = reader.IsDBNull(7) ? null : reader.GetDouble(7),
            AverageTemperature = reader.IsDBNull(8) ? null : reader.GetDouble(8),
            PassedCriteria = reader.GetInt32(9),
            Remarks = reader.GetString(10),
            CreatedAt = reader.GetString(11),
            TempA1 = reader.IsDBNull(12) ? null : reader.GetDouble(12),
            TempA2 = reader.IsDBNull(13) ? null : reader.GetDouble(13),
            TempA3 = reader.IsDBNull(14) ? null : reader.GetDouble(14),
            TempB1 = reader.IsDBNull(15) ? null : reader.GetDouble(15),
            TempB2 = reader.IsDBNull(16) ? null : reader.GetDouble(16),
            TempB3 = reader.IsDBNull(17) ? null : reader.GetDouble(17),
            TempC1 = reader.IsDBNull(18) ? null : reader.GetDouble(18),
            TempC2 = reader.IsDBNull(19) ? null : reader.GetDouble(19),
            TempC3 = reader.IsDBNull(20) ? null : reader.GetDouble(20),
            TAvg = reader.IsDBNull(21) ? null : reader.GetDouble(21),
            TAvgAxis1 = reader.IsDBNull(22) ? null : reader.GetDouble(22),
            TAvgAxis2 = reader.IsDBNull(23) ? null : reader.GetDouble(23),
            TAvgAxis3 = reader.IsDBNull(24) ? null : reader.GetDouble(24),
            TAvgLevela = reader.IsDBNull(25) ? null : reader.GetDouble(25),
            TAvgLevelb = reader.IsDBNull(26) ? null : reader.GetDouble(26),
            TAvgLevelc = reader.IsDBNull(27) ? null : reader.GetDouble(27),
            TDevAxis1 = reader.IsDBNull(28) ? null : reader.GetDouble(28),
            TDevAxis2 = reader.IsDBNull(29) ? null : reader.GetDouble(29),
            TDevAxis3 = reader.IsDBNull(30) ? null : reader.GetDouble(30),
            TDevLevela = reader.IsDBNull(31) ? null : reader.GetDouble(31),
            TDevLevelb = reader.IsDBNull(32) ? null : reader.GetDouble(32),
            TDevLevelc = reader.IsDBNull(33) ? null : reader.GetDouble(33),
            TAvgDevAxis = reader.IsDBNull(34) ? null : reader.GetDouble(34),
            TAvgDevLevel = reader.IsDBNull(35) ? null : reader.GetDouble(35),
            CenterTempData = reader.IsDBNull(36) ? null : reader.GetString(36),
            Memo = reader.IsDBNull(37) ? null : reader.GetString(37)
        };
    }

    #endregion

    #region 传感器配置

    /// <summary>
    /// 获取所有传感器配置
    /// </summary>
    public List<Sensor> GetAllSensors()
    {
        var list = new List<Sensor>();
        using var conn = CreateConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM sensors ORDER BY sensorid";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Sensor
            {
                SensorId = reader.GetInt32(0),
                SensorName = reader.GetString(1),
                DispName = reader.GetString(2),
                SensorGroup = reader.GetString(3),
                Unit = reader.GetString(4),
                Description = reader.GetString(5),
                Flag = reader.GetString(6),
                SignalZero = reader.GetDouble(7),
                SignalSpan = reader.GetDouble(8),
                OutputZero = reader.GetDouble(9),
                OutputSpan = reader.GetDouble(10),
                OutputValue = reader.GetDouble(11),
                InputValue = reader.GetDouble(12),
                SignalType = reader.GetInt32(13)
            });
        }
        return list;
    }

    #endregion
}