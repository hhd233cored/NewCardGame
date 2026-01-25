using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameAction
{
    /// <summary>在 action 发生前要结算的反应</summary>
    public List<GameAction> PreReactions { get; } = new();

    /// <summary>执行 action 本体之后立刻结算的反应</summary>
    public List<GameAction> PreformReactions { get; } = new(); // 建议改名 PerformReactions

    /// <summary>整个action链结束前的反应</summary>
    public List<GameAction> PostReactions { get; } = new();

    public virtual string DebugName => GetType().Name;

    /// <summary>用于防止无限连锁，默认0</summary>
    public int Depth { get; internal set; } = 0;
}
