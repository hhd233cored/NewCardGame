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
    /// 初始化系统
    /// </summary>
    private void Start()
    {
        PlayerSystem.Instance.Setup(playerData);
        EnemySystem.Instance.Setup(battleData.enemies);
        CardSystem.Instance.Setup(playerData.Deck);
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
    /// <summary>
    /// 初始化新战斗场景信息
    /// </summary>
    public void NewBattle(List<EnemyData> enemiesD, List<CardData> cardD)
    {
        EnemySystem.Instance.Setup(enemiesD);
        CardSystem.Instance.Setup(cardD);
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
    public void NewBattle(BattleData battle, List<CardData> cardD)
    {
        EnemySystem.Instance.Setup(battle.enemies);
        CardSystem.Instance.Setup(cardD);
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
        EnemyTurnGA enemyTurnGA = new();
        ActionSystem.Instance.Perform(enemyTurnGA);
    }
}
