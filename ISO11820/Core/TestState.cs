namespace ISO11820.Core;

/// <summary>
/// 试验状态枚举
/// </summary>
public enum TestState
{
    /// <summary>
    /// 空闲状态：炉子未加热，等待新建试验
    /// </summary>
    Idle,

    /// <summary>
    /// 升温状态：炉子正在升温，等待温度稳定
    /// </summary>
    Preparing,

    /// <summary>
    /// 就绪状态：温度已稳定，可以开始记录
    /// </summary>
    Ready,

    /// <summary>
    /// 记录状态：正在记录温度数据
    /// </summary>
    Recording,

    /// <summary>
    /// 完成状态：试验已完成，等待保存现象记录
    /// </summary>
    Complete
}