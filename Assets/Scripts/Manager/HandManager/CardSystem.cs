using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;
public enum PileType { DrawPile, DiscardPile, Hand }
public class CardSystem : Singleton<CardSystem>
{
    [Header("组件")]
    [SerializeField] private HandView handView;
    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;
    [SerializeField] private Transform exhaustPilePoint;
    [SerializeField] private CardView cardViewPrefab;


    [field: SerializeField] private readonly List<Card> drawPile = new();
    [field: SerializeField] private readonly List<Card> discardPile = new();
    [field: SerializeField] private readonly List<Card> hand = new();
    [field: SerializeField] private readonly List<Card> exhaustPile= new();//消耗牌堆
    [SerializeField] private List<Card> UseCardsHistory = new();

    // 关键：在 CardSystem 里维护手牌视图
    private readonly List<CardView> handViews = new();

    // 选择弃牌状态
    private bool isChoosingDiscard = false;
    private int needChooseAmount = 0;
    private readonly HashSet<CardView> chosen = new();

    private const int maxHand = 10;
    public bool IsChoosingDiscard => isChoosingDiscard;

    public List<Card> DrawPile => drawPile;
    public List<Card> DisCardPile => discardPile;
    public List<Card> ExhaustPile => exhaustPile;
    private void OnEnable()
    {
        ActionSystem.RegisterPerformer<DrawCardsGA>(this, DrawCardsPerformer);
        ActionSystem.RegisterPerformer<DiscardCardsGA>(this, DiscardCardsPerformer);
        ActionSystem.RegisterPerformer<PlayCardGA>(this, PlayCardPerformer);
        ActionSystem.RegisterPerformer<ExhaustCardsGA>(this, ExhaustCardPerformer);
        ActionSystem.RegisterPerformer<GainCardGA>(this, GainCardPerformer);
        ActionSystem.SubscribePre<EnemyTurnGA>(EnemyTurnPreReaction);
        ActionSystem.SubscribePost<EnemyTurnGA>(EnemyTurnPostReaction);
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribePre<EnemyTurnGA>(EnemyTurnPreReaction);
        ActionSystem.UnsubscribePost<EnemyTurnGA>(EnemyTurnPostReaction);
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
    public void Unsub()
    {
        ActionSystem.UnsubscribePre<EnemyTurnGA>(EnemyTurnPreReaction);
        ActionSystem.UnsubscribePost<EnemyTurnGA>(EnemyTurnPostReaction);
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

        // 也可以加一个“确认按钮”，那就改成等 ConfirmPressed
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
        //bool takeEffect = BattleSystem.Instance.HasSameSuitOrNum(ga.CardView.CardSuit, ga.CardView.CardNum);//如果花色点数接不上，仍然能打出，但不触发效果

        bool takeEffect = BattleSystem.Instance.StrictCheckSuitOrNum(ga.CardView.CardSuit, ga.CardView.CardNum);//如果花色点数接不上，仍然能打出，但不触发效果

       

        //takeEffect = true;//假设必然会执行效果
        if (takeEffect)
        {
            //执行效果
            //Debug.Log("Take Effect");
            foreach(var effect in ga.CardView.card.ManualTargetEffects)
            {
                PerformEffectGA performEffectGA = new(effect, new() { ga.Target }, PlayerSystem.Instance.player);
                ActionSystem.Instance.AddReaction(performEffectGA);
            }
            foreach (var effectWrapper in ga.CardView.card.OtherEffects)
            {
                List<Character> targets = effectWrapper.TargetMode.GetTargets();
                PerformEffectGA performEffectGA = new(effectWrapper.Effect, targets, PlayerSystem.Instance.player);
                ActionSystem.Instance.AddReaction(performEffectGA);
            }
        }

        UseCardsHistory.Add(cv.card);

        //改变当前花色和点数
        SetSuitAndNumGA setSuitAndNumGA = new(ga.CardView.CardSuit, ga.CardView.CardNum);
        ActionSystem.Instance.AddReaction(setSuitAndNumGA);

        if (cv.card.data.Exhaust && takeEffect)
        {
            ExhaustCardsGA exhaustCardsGA = new(cv);
            ActionSystem.Instance.AddReaction(exhaustCardsGA);
        }
        else if (cv.card.data.CardType == CardType.Power && takeEffect) yield return DestroyPowerCard(cv);
        else yield return DiscardOne(cv);
    }

    private IEnumerator GainCardPerformer(GainCardGA ga)
    {
        int[] quadrantCounts = new int[4];

        // 1. 确定目标位置的逻辑列表和物理坐标点
        List<Card> targetList = null;
        Transform targetPoint = null;

        // 根据 PileType 映射
        switch (ga.pilePosition)
        {
            case PileType.DrawPile:
                targetList = drawPile;
                targetPoint = drawPilePoint;
                break;
            case PileType.DiscardPile:
                targetList = discardPile;
                targetPoint = discardPilePoint;
                break;
            case PileType.Hand:
                // 手牌不需要 targetPoint，因为它会进入 HandView 的排布系统
                break;
        }

        // 2. 遍历并生成卡牌
        for (int i = 0; i < ga.cardDatas.Count; i++)
        {
            for (int j = 0; j < ga.amounts[i]; j++)
            {
                Card newCard = new Card(ga.cardDatas[i]);

                if (ga.pilePosition == PileType.Hand && hand.Count < maxHand)
                {
                    hand.Add(newCard);

                    CardView cardView = MainController.Instance.CreateCardView(
                        newCard, drawPilePoint.position, drawPilePoint.rotation
                    );
                    cardView.transform.parent = handView.transform;
                    // 维护 handViews
                    handViews.Add(cardView);

                    yield return handView.AddCard(cardView);
                    continue;
                }


                // 2. 寻找卡牌最少的象限索引
                int targetQuad = 0;
                int minCount = quadrantCounts[0];
                for (int k = 1; k < 4; k++)
                {
                    if (quadrantCounts[k] < minCount)
                    {
                        minCount = quadrantCounts[k];
                        targetQuad = k;
                    }
                }
                quadrantCounts[targetQuad]++; // 该象限计数加一

                // 3. 根据选定的象限计算随机世界坐标
                Vector2 rangeX = targetQuad % 2 == 0 ? new Vector2(0.2f, 0.45f) : new Vector2(0.55f, 0.8f);
                Vector2 rangeY = targetQuad < 2 ? new Vector2(0.55f, 0.8f) : new Vector2(0.2f, 0.45f);

                float rx = UnityEngine.Random.Range(rangeX.x, rangeX.y);
                float ry = UnityEngine.Random.Range(rangeY.x, rangeY.y);

                Vector3 spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(rx, ry, 0));
                spawnPos.z = -5f;

                // 4. 生成并初始化
                CardView cv = Instantiate(cardViewPrefab, spawnPos, Quaternion.identity);
                cv.Setup(newCard);
                cv.transform.parent = this.transform;

                // 5. 简单的出现动画：让卡牌从中心微小弹出
                cv.transform.localScale = Vector3.zero;
                cv.transform.DOScale(0.7f, 0.3f).SetEase(Ease.OutBack);

                // 6. 执行后续去向逻辑

                var finalTargetList = (ga.pilePosition == PileType.Hand) ? discardPile : targetList;
                var finalTargetPoint = (ga.pilePosition == PileType.Hand) ? discardPilePoint : targetPoint;
                StartCoroutine(FlyToPileSpecial(cv, finalTargetList, finalTargetPoint));

                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    private IEnumerator FlyToPileSpecial(CardView cv, List<Card> targetList, Transform targetPoint)
    {
        targetList.Add(cv.card);

        cv.transform.localScale = Vector3.one * 0.7f; // 初始大小 0.7
        cv.transform.rotation = Quaternion.identity;  // 飞向牌堆前不旋转

        float duration = 0.2f; // 总时间 0.5s

        yield return new WaitForSeconds(0.5f);

        // 创建序列
        DG.Tweening.Sequence seq = DOTween.Sequence();

        // 同时执行：移动到目标点 + 逐渐变小（缩放到 0.1 或 0 视情况而定）
        seq.Append(cv.transform.DOMove(targetPoint.position, duration).SetEase(Ease.InQuad));
        seq.Join(cv.transform.DOScale(0.1f, duration).SetEase(Ease.InQuad));

        // 等待动画结束
        yield return seq.WaitForCompletion();

        // 4. 销毁视图
        Destroy(cv.gameObject);
    }

    private IEnumerator ExhaustCardPerformer(ExhaustCardsGA exhaustCardsGA)
    {
        yield return StartCoroutine(ExhaustOne(exhaustCardsGA.card));
    }

    public bool TryPlayCardFromDrag(CardView cv)
    {
        if (cv == null) return false;

        // 正在结算动作时不允许
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
    private IEnumerator DestroyPowerCard(CardView cv)
    {
        if (cv == null) yield break;

        Card card = cv.card;

        //1.数据层：hand -> ExhaustPile
        if (card != null)
        {
            hand.Remove(card);
        }

        //2.视图层：从手牌视图列表移除
        handViews.Remove(cv);

        var tr = cv.transform;

        // handView.LockCard(cv);

        tr.DOKill();
        // 让手牌重排
        yield return handView.RemoveCard(cv, 0.12f);

        // handView.UnlockCard(cv);
        handView.UnlockCard(cv);
        Destroy(cv.gameObject);
    }
    //消耗一张牌
    private IEnumerator ExhaustOne(CardView cv)
    {
        if (cv == null) yield break;
        Card card = cv.card;

        // 1. 数据层操作
        if (card != null)
        {
            hand.Remove(card);
            exhaustPile.Add(card);
        }

        // 2. 视图层操作
        handViews.Remove(cv);

        // 停止由于 HandView 布局导致的自动移动，防止和动画冲突
        cv.transform.DOKill();

        // 3. 播放消耗特效
        yield return cv.ExhaustEffectRoutine(exhaustPilePoint);

        // 4. 重排手牌
        yield return handView.RemoveCard(cv, 0.12f);
        handView.UnlockCard(cv);

        // 5. 销毁物体
        Destroy(cv.gameObject);
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
        handView.ExhaustAllEthereal();
    }
    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        if (EnemySystem.Instance.Enemies.Count == 0) return;

        //结算敌人buff，更新意图
        foreach(var enemy in EnemySystem.Instance.Enemies)
        {
            for (int i = enemy.BuffList.Count - 1; i >= 0; i--)
            {
                var buff = enemy.BuffList[i];
                if (!buff.OnTick())
                {
                    enemy.RemoveBuff(buff);
                }
            }
            enemy.UpdateIntentionText();
        }

        Player player = PlayerSystem.Instance.player;

        //清空格挡
        player.ClearBlock();

        //执行Buff
        for (int i = player.BuffList.Count - 1; i >= 0; i--)
        {
            var buff = player.BuffList[i];
            if (!buff.OnTick())
            {
                player.RemoveBuff(buff);
            }
        }


        //摸牌阶段，摸三张
        DrawCardsGA drawCardsGA = new(3);
        ActionSystem.Instance.AddReaction(drawCardsGA);
    }
}