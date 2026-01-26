using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CardSystem : Singleton<CardSystem>
{
    [Header("组件")]
    [SerializeField] private HandView handView;
    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;

   
    [field: SerializeField] private readonly List<Card> drawPile = new();
    [field: SerializeField] private readonly List<Card> discardPile = new();
    [field: SerializeField] private readonly List<Card> hand = new();
    [SerializeField] private List<Card> UseCardsHistory = new();

    // 关键：在 CardSystem 里维护手牌视图
    private readonly List<CardView> handViews = new();

    // 选择弃牌状态
    private bool isChoosingDiscard = false;
    private int needChooseAmount = 0;
    private readonly HashSet<CardView> chosen = new();

    private const int maxHand = 10;
    public bool IsChoosingDiscard => isChoosingDiscard;
    private void OnEnable()
    {
        ActionSystem.RegisterPerformer<DrawCardsGA>(this, DrawCardsPerformer);
        ActionSystem.RegisterPerformer<DiscardCardsGA>(this, DiscardCardsPerformer);
        ActionSystem.RegisterPerformer<PlayCardGA>(this, PlayCardPerformer);
        ActionSystem.SubscribePre<EnemyTurnGA>(EnemyTurnPreReaction);
        ActionSystem.SubscribePost<EnemyTurnGA>(EnemyTurnPostReaction);
    }

    private void OnDisable()
    {
        ActionSystem.UnregisterPerformersByOwner(this);
        ExitDiscardChooseMode(); // 防止卸载时还挂着事件
    }
    public void Setup(List<Card> cards)
    {
        foreach(var card in cards)
        {
            drawPile.Add(card);
        }
    }
    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGA)
    {
        int actualAmount = Mathf.Min(drawCardsGA.Amount, drawPile.Count);
        int notDrawnAmount = drawCardsGA.Amount - actualAmount;

        yield return DrawCard(actualAmount);

        if (notDrawnAmount > 0)
        {
            RefillDeck();
            yield return DrawCard(Mathf.Min(notDrawnAmount, drawPile.Count));
        }
    }

    private IEnumerator DiscardCardsPerformer(DiscardCardsGA ga)
    {
        if (ga == null) yield break;
        if (handViews.Count == 0) yield break;

        int amount = Mathf.Clamp(ga.Amount, 0, handViews.Count);
        if (amount == 0) yield break;

        if (!ga.IsChoose)
        {
            // 随机弃牌
            for (int i = 0; i < amount; i++)
            {
                int idx = UnityEngine.Random.Range(0, handViews.Count);
                var cv = handViews[idx];
                yield return DiscardOne(cv);
            }
            yield break;
        }

        // 玩家选择弃牌
        yield return ChooseAndDiscard(amount);
    }

    private IEnumerator ChooseAndDiscard(int amount)
    {
        EnterDiscardChooseMode(amount);

        // 等玩家点够
        while (chosen.Count < needChooseAmount)
            yield return null;

        // 你也可以加一个“确认按钮”，那就改成等 ConfirmPressed
        // 这里简单：点够就自动弃

        // 把选择结果拷贝出来（避免弃牌时修改集合）
        var list = new List<CardView>(chosen);

        ExitDiscardChooseMode();

        for (int i = 0; i < list.Count; i++)
            yield return DiscardOne(list[i]);
    }
    private IEnumerator PlayCardPerformer(PlayCardGA ga)
    {
        if (ga == null) yield break;
        var cv = ga.CardView;
        if (cv == null) yield break;

        // 保护：必须仍在手牌中
        if (!handViews.Contains(cv)) yield break;

        // TODO：这里可以加费用检查、目标选择等
        bool takeEffect = BattleSystem.Instance.HasSameSuitOrNum(ga.CardView.CardSuit, ga.CardView.CardNum);//如果花色点数接不上，仍然能打出，但不触发效果

        //改变当前花色和点数
        SetSuitAndNumGA setSuitAndNumGA = new(ga.CardView.CardSuit, ga.CardView.CardNum);
        ActionSystem.Instance.AddReaction(setSuitAndNumGA);

        if (takeEffect)
        {
            //执行效果
            Debug.Log("Take Effect");
            if (ga.CardView.card.ManualTargetEffects != null)
            {
                PerformEffectGA performEffectGA = new(ga.CardView.card.ManualTargetEffects, new() { ga.Target });
                ActionSystem.Instance.AddReaction(performEffectGA);
            }
            foreach (var effectWrapper in ga.CardView.card.OtherEffects)
            {
                List<Character> targets = effectWrapper.TargetMode.GetTargets();
                PerformEffectGA performEffectGA = new(effectWrapper.Effect, targets);
                ActionSystem.Instance.AddReaction(performEffectGA);
            }
        }

        UseCardsHistory.Add(cv.card);

        yield return DiscardOne(cv);
    }
    public bool TryPlayCardFromDrag(CardView cv)
    {
        if (cv == null) return false;

        // 正在结算动作时不允许（你的交互规则）
        if (!Interactions.Instance.PlayerCanInteract()) return false;

        // 必须在手牌
        if (!handViews.Contains(cv)) return false;

        handView.LockCard(cv);//锁定牌的位置
        handView.OnCardHoverExit(cv); // 防止 hover 触发布局（可选）

        // 发起动作：让 ActionSystem 来跑
        ActionSystem.Instance.Perform(new PlayCardGA(cv));
        return true;
    }

    private void EnterDiscardChooseMode(int amount)
    {
        Interactions.Instance.AllowInteractWhilePerforming = true;
        isChoosingDiscard = true;
        needChooseAmount = amount;
        chosen.Clear();

        // 给所有手牌 CardView 订阅点击
        for (int i = 0; i < handViews.Count; i++)
            handViews[i].Clicked += OnHandCardClickedForDiscard;
    }

    private void ExitDiscardChooseMode()
    {
        if (!isChoosingDiscard) return;

        // 取消订阅，避免重复绑定
        for (int i = 0; i < handViews.Count; i++)
            handViews[i].Clicked -= OnHandCardClickedForDiscard;

        isChoosingDiscard = false;
        needChooseAmount = 0;
        chosen.Clear();
        Interactions.Instance.AllowInteractWhilePerforming = false;
    }

    private void OnHandCardClickedForDiscard(CardView cv)
    {
        Debug.Log("click");
        if (!isChoosingDiscard) return;
        if (cv == null) return;
        if (!handViews.Contains(cv)) return;

        // 简单的 toggle 选择
        if (chosen.Contains(cv))
        {
            chosen.Remove(cv);
            // 这里可以做个“取消选中”的视觉（缩回去/描边取消）
        }
        else
        {
            if (chosen.Count >= needChooseAmount) return;
            chosen.Add(cv);
            // 这里可以做个“选中”的视觉（抬起一点/描边）
        }
    }

    private IEnumerator DiscardOne(CardView cv)
    {
        if (cv == null) yield break;

        Card card = cv.card;

        //1.数据层：hand -> discardPile
        if (card != null)
        {
            hand.Remove(card);
            discardPile.Add(card);
        }

        //2.视图层：从手牌视图列表移除
        handViews.Remove(cv);

        var tr = cv.transform;

        // handView.LockCard(cv);

        tr.DOKill();

        float moveT = 0.25f;
        float shrinkT = 0.18f;
        float endScale = 0.1f;

        Vector3 startScale = tr.localScale;

        DG.Tweening.Sequence seq = DOTween.Sequence();
        seq.Join(tr.DOMove(discardPilePoint.position, moveT).SetEase(Ease.InCubic));
        seq.Join(tr.DORotateQuaternion(discardPilePoint.rotation, moveT).SetEase(Ease.InCubic));
        seq.Insert(moveT - shrinkT, tr.DOScale(startScale * endScale, shrinkT).SetEase(Ease.InBack));

        yield return seq.WaitForCompletion();

        // 让手牌重排
        yield return handView.RemoveCard(cv, 0.12f);

        // handView.UnlockCard(cv);
        handView.UnlockCard(cv);
        Destroy(cv.gameObject);
    }

    private IEnumerator DrawCard(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (handViews.Count >= maxHand) break;
            Card card = drawPile.Draw();
            hand.Add(card);

            CardView cardView = MainController.Instance.CreateCardView(
                card, drawPilePoint.position, drawPilePoint.rotation
            );
            cardView.transform.parent = handView.transform;
            // 维护 handViews
            handViews.Add(cardView);

            yield return handView.AddCard(cardView);
        }
    }
    private void RefillDeck()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
    }
    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    {
      
    }
    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.AddReaction(drawCardsGA);
    }
}