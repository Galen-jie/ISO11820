namespace ISO11820.Data.Models;

/// <summary>
/// 样品信息模型
/// </summary>
public class ProductMaster
{
    public string ProductId { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string Specific { get; set; } = ""; // 规格型号
    public double Diameter { get; set; } // 直径mm
    public double Height { get; set; } // 高度mm
    public string? Flag { get; set; }
}