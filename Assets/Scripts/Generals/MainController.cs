using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainController:Singleton<MainController>
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private BattleData battleData;
    [SerializeField] private BuffData strengthData;
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
    public void EndTurn()
    {
        //弃牌阶段
        //DiscardCardsGA discardCardsGA = new(1, true);
        //ActionSystem.Instance.AddReaction(discardCardsGA);

        //重置“上一张牌”的花色点数
        SetSuitAndNumGA setSuitAndNumGA = new(SuitStyle.Nul, 0);
        ActionSystem.Instance.Perform(setSuitAndNumGA);

        //进入敌人回合
        EnemyTurnGA enemyTurnGA = new();
        ActionSystem.Instance.Perform(enemyTurnGA);
    }
    public void ToggleDrawPileView()
    {
        DeckViewUI.Instance.ToggleDeckView(CardSystem.Instance.DrawPile.OrderBy(c => c.Num).ToList());
    }
    public void ToggleDiscardPileView()
    {
        DeckViewUI.Instance.ToggleDeckView(CardSystem.Instance.DisCardPile);
    }

    public int TotalDamage(int basicDamage, List<Character> targets, Character source)
    {
        int damage = basicDamage;

        //力量加成，1点力量+2点伤害
        Buff power = source.BuffList.Find(buff => buff.data == strengthData);
        if (power != null) damage += power.stacks * 2;

        return damage;
    }
}
