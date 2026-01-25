using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameAction
{
    /// <summary>在 action 发生前要结算的反应（例如：打出前触发“每当你打出牌…”）</summary>
    public List<GameAction> PreReactions { get; } = new();

    /// <summary>执行 action 本体之后立刻结算的反应（例如：伤害造成后触发吸血、护盾破裂等）</summary>
    public List<GameAction> PerformReactions { get; } = new();

    /// <summary>整个 action 链结束前的反应（例如：回合结束、清理状态）</summary>
    public List<GameAction> PostReactions { get; } = new();

    public virtual string DebugName => GetType().Name;

    /// <summary>用于防止无限连锁，默认 0</summary>
    public int Depth { get; internal set; } = 0;
}