namespace ISO11820.Data.Models;

/// <summary>
/// 试验记录模型（核心表）
/// </summary>
public class TestMasterRecord
{
    // 基本信息
    public string ProductId { get; set; } = "";
    public string TestId { get; set; } = "";
    public DateTime TestDate { get; set; }
    public double AmbTemp { get; set; } // 环境温度
    public double AmbHumi { get; set; } // 环境湿度
    public string According { get; set; } = "ISO 11820:2022";
    public string Operator { get; set; } = "";
    public string ApparatusId { get; set; } = "";
    public string ApparatusName { get; set; } = "";
    public DateTime ApparatusChkDate { get; set; }
    public string RptNo { get; set; } = "";

    // 质量数据
    public double PreWeight { get; set; } // 试验前质量g
    public double PostWeight { get; set; } // 试验后质量g
    public double LostWeight { get; set; } // 失重量g
    public double LostWeightPer { get; set; } // 失重率%

    // 试验过程
    public int TotalTestTime { get; set; } // 总试验时长秒
    public int ConstPower { get; set; } // 恒功率值
    public string PhenoCode { get; set; } = ""; // 现象编码
    public int FlameTime { get; set; } // 火焰开始时刻秒
    public int FlameDuration { get; set; } // 火焰持续时间秒

    // 各通道温度最大值
    public double MaxTf1 { get; set; }
    public double MaxTf2 { get; set; }
    public double MaxTs { get; set; }
    public double MaxTc { get; set; }
    public int MaxTf1Time { get; set; }
    public int MaxTf2Time { get; set; }
    public int MaxTsTime { get; set; }
    public int MaxTcTime { get; set; }

    // 各通道温度最终值
    public double FinalTf1 { get; set; }
    public double FinalTf2 { get; set; }
    public double FinalTs { get; set; }
    public double FinalTc { get; set; }
    public int FinalTf1Time { get; set; }
    public int FinalTf2Time { get; set; }
    public int FinalTsTime { get; set; }
    public int FinalTcTime { get; set; }

    // 温升
    public double DeltaTf1 { get; set; }
    public double DeltaTf2 { get; set; }
    public double DeltaTf { get; set; } // 样品温升（判定项）
    public double DeltaTs { get; set; }
    public double DeltaTc { get; set; }

    // 备注
    public string? Memo { get; set; }
    public string? Flag { get; set; }

    // 关联样品名称（用于显示，不存数据库）
    public string ProductName { get; set; } = "";
}