using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController:Singleton<MainController>
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private BattleData battleData;
    /// <summary>
    /// 初始化新战斗场景信息
    /// </summary>
    public void NewBattle(List<EnemyData> enemiesD, List<CardData> cardD)
    {
        EnemySystem.Instance.Setup(enemiesD);
        CardSystem.Instance.Setup(PlayerSystem.Instance.player.CurrentCards);
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
    public void NewBattle(BattleData battle, List<CardData> cardD)
    {
        EnemySystem.Instance.Setup(battle.enemies);
        CardSystem.Instance.Setup(PlayerSystem.Instance.player.CurrentCards);
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
    /// <summary>
    /// 生成敌怪
    /// </summary>
    public Enemy CreateEnemy(EnemyData enemyData, Vector3 position, Quaternion rotation)
    {
        Enemy enemy = Instantiate(enemyPrefab, position, rotation);
        enemy.Setup(enemyData);
        return enemy;
    }
    /// <summary>
    /// 生成卡牌
    /// </summary>
    public CardView CreateCardView(Card card, Vector3 position, Quaternion rotation)
    {
        CardView cardView = Instantiate(cardViewPrefab, position, rotation);
        cardView.Setup(card);
        return cardView;
    }
    /// <summary>
    /// 结束回合
    /// </summary>
    public static void EndTurn()
    {
        //弃牌阶段
        //DiscardCardsGA discardCardsGA = new(1, true);
        //ActionSystem.Instance.AddReaction(discardCardsGA);

        //重置“上一张牌”的花色点数
        SetSuitAndNumGA setSuitAndNumGA = new(SuitStyle.Nul, 0);
        ActionSystem.Instance.AddReaction(setSuitAndNumGA);

        //执行buff结算
        foreach (var enemy in EnemySystem.Instance.Enemies)
        {
            for (int i = enemy.BuffList.Count - 1; i >= 0; i--)
            {
                var buff = enemy.BuffList[i];
                if (!buff.OnTick())
                {
                    enemy.RemoveBuff(buff);
                }
            }
        }

        //进入敌人回合
        EnemyTurnGA enemyTurnGA = new();
        ActionSystem.Instance.Perform(enemyTurnGA);
    }
    public void ToggleDrawPileView()
    {
        DeckViewUI.Instance.ToggleDeckView(CardSystem.Instance.DrawPile);
    }
    public void ToggleDiscardPileView()
    {
        DeckViewUI.Instance.ToggleDeckView(CardSystem.Instance.DisCardPile);
    }
}
