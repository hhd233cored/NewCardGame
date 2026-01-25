using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class ActionSystem : Singleton<ActionSystem>
{
    private List<GameAction> reactions = null;

    public bool IsPerforming { get; private set; } = false;

    //订阅：在某个 action 流程开始/结束时做额外逻辑（UI/音效/日志）
    private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();
    private static Dictionary<Type, List<Action<GameAction>>> postSubs = new();

    //执行器：每种 GameAction 对应一个协程 performer
    private static Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();

    //存储每个performer注册时所用的组件,便于在OnDisable时移除
    private static readonly Dictionary<object, HashSet<Type>> performerOwners = new();

    //队列：保证同一时间只跑一个 action flow，其余排队
    private readonly Queue<(GameAction action, Action onDone)> queue = new();

    private readonly Stack<List<GameAction>> reactionsStack = new();
    [Header("Safety")]
    [SerializeField] private int maxChainActions = 200;// 防无限连锁
    [SerializeField] private int maxDepth = 30;// 防过深递归链

    /// <summary>
    ///执行一个 action。若系统有任务，则排队。
    /// </summary>
    public void Perform(GameAction action, System.Action OnPerformFinished = null)
    {
        if (action == null)
        {
            OnPerformFinished?.Invoke();
            return;
        }

        queue.Enqueue((action, OnPerformFinished));

        if (!IsPerforming)
            StartCoroutine(RunQueue());
    }

    /// <summary>
    ///允许performer或subscriber在当前阶段追加反应action。
    ///追加到当前 reactions 列表（也就是正在结算的那一段）。
    /// </summary>
    public void AddReacyion(GameAction gameAction)
    {
        if (gameAction == null) return;
        if (reactionsStack.Count == 0) return;
        reactionsStack.Peek().Add(gameAction);
    }


    private IEnumerator RunQueue()
    {
        IsPerforming = true;

        while (queue.Count > 0)
        {
            var (action, onDone) = queue.Dequeue();
            bool finished = false;

            yield return Flow(action, () =>
            {
                finished = true;
                onDone?.Invoke();
            });

            if (!finished)
                onDone?.Invoke();
        }

        IsPerforming = false;
    }

    private IEnumerator Flow(GameAction action, Action OnFlowFinished = null)
    {
        if (action == null)
        {
            OnFlowFinished?.Invoke();
            yield break;
        }

        //深度限制
        if (action.Depth > maxDepth)
        {
            Debug.LogWarning($"[ActionSystem] Max depth reached for {action.DebugName}, abort chain.");
            OnFlowFinished?.Invoke();
            yield break;
        }

        //pre
        reactions = action.PreReactions;
        reactionsStack.Push(reactions);
        PerformSubscribers(action, preSubs);
        yield return PerformReactions();//执行 pre reactions
        reactionsStack.Pop();

        //perform
        reactions = action.PreformReactions;
        reactionsStack.Push(reactions);
        yield return PerformPerformer(action);
        yield return PerformReactions();
        reactionsStack.Pop();

        //post
        reactions = action.PostReactions;
        reactionsStack.Push(reactions);
        PerformSubscribers(action, postSubs);
        yield return PerformReactions();//执行 post reactions
        reactionsStack.Pop();

        reactions = null;
        OnFlowFinished?.Invoke();
    }

    /// <summary>
    /// 执行当前 reactions 列表里的所有 action，并允许在执行过程中不断 AddReacyion 插入新反应。
    /// </summary>
    private IEnumerator PerformReactions()
    {
        // 拷贝当前reactions引用
        var list = reactions;

        if (list == null || list.Count == 0)
            yield break;

        int safetyCounter = 0;
        int i = 0;

        while (i < list.Count)
        {
            if (++safetyCounter > maxChainActions)
            {
                Debug.LogWarning("[ActionSystem] Reaction chain exceeded maxChainActions, abort.");
                yield break;
            }

            GameAction r = list[i];
            i++;

            if (r == null) continue;

            r.Depth = Mathf.Max(r.Depth, 1);

            bool done = false;
            yield return Flow(r, () => done = true);

            if (!done)
                yield return null;
        }
    }

    private static void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> dict)
    {
        if (action == null) return;

        Type t = action.GetType();

        if (dict.TryGetValue(t, out var list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                try { list[i]?.Invoke(action); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }
    }

    private static IEnumerator EmptyPerformer(GameAction action)
    {
        Debug.LogWarning($"[ActionSystem] No performer registered for action type: {action?.GetType().Name}");
        yield break;
    }
    private IEnumerator PerformPerformer(GameAction action)
    {
        if (action == null) yield break;

        Type t = action.GetType();
        if (!performers.TryGetValue(t, out var perf) || perf == null)
        {
            yield return EmptyPerformer(action);
            yield break;
        }

        yield return perf(action);
    }
    public static void RegisterPerformer<T>(object owner, Func<T, IEnumerator> performer) where T : GameAction
    {
        if (owner == null) throw new ArgumentNullException(nameof(owner));
        if (performer == null) throw new ArgumentNullException(nameof(performer));

        var type = typeof(T);

        // 注册 performer（覆盖同类型）
        performers[type] = (ga) => performer((T)ga);

        // 记录 owner -> types
        if (!performerOwners.TryGetValue(owner, out var set))
        {
            set = new HashSet<Type>();
            performerOwners[owner] = set;
        }
        set.Add(type);
    }

    /// <summary>
    /// 删除该 owner 注册过的所有 performer
    /// </summary>
    public static int UnregisterPerformersByOwner(object owner)
    {
        if (owner == null) return 0;

        if (!performerOwners.TryGetValue(owner, out var set))
            return 0;

        int removed = 0;
        foreach (var type in set)
        {
            if (performers.Remove(type))
                removed++;
        }

        performerOwners.Remove(owner);
        return removed;
    }

    /// <summary>
    /// 删除某个 owner 注册的某个 actionType performer
    /// </summary>
    public static bool UnregisterPerformerByOwner<T>(object owner) where T : GameAction
    {
        if (owner == null) return false;

        var type = typeof(T);

        bool ok = performers.Remove(type);

        if (performerOwners.TryGetValue(owner, out var set))
        {
            set.Remove(type);
            if (set.Count == 0) performerOwners.Remove(owner);
        }

        return ok;
    }

    public static void SubscribePre<T>(Action<T> sub) where T : GameAction
    {
        AddSub(preSubs, sub);
    }

    public static void SubscribePost<T>(Action<T> sub) where T : GameAction
    {
        AddSub(postSubs, sub);
    }

    private static void AddSub<T>(Dictionary<Type, List<Action<GameAction>>> dict, Action<T> sub) where T : GameAction
    {
        if (sub == null) return;

        Type t = typeof(T);
        if (!dict.TryGetValue(t, out var list))
        {
            list = new List<Action<GameAction>>();
            dict[t] = list;
        }

        list.Add((ga) => sub((T)ga));
    }
}

