namespace ISO11820.Core;

/// <summary>
/// 系统消息类型
/// </summary>
public enum MessageType
{
    /// <summary>
    /// 普通消息（白色）
    /// </summary>
    Normal,

    /// <summary>
    /// 警告消息（黄色）
    /// </summary>
    Warning,

    /// <summary>
    /// 错误消息（红色）
    /// </summary>
    Error
}

/// <summary>
/// 系统消息数据类
/// </summary>
public class MasterMessage
{
    /// <summary>
    /// 消息时间，格式 HH:mm:ss
    /// </summary>
    public string Time { get; set; } = "";

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType Type { get; set; } = MessageType.Normal;
}