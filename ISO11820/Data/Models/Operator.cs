namespace ISO11820.Data.Models;

/// <summary>
/// 操作员/用户模型
/// </summary>
public class Operator
{
    public string UserId { get; set; } = "";
    public string Username { get; set; } = "";
    public string Pwd { get; set; } = "";
    public string UserType { get; set; } = ""; // admin 或 operator
}