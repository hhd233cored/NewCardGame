using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalController : Singleton<GlobalController>
{
    [SerializeField] private PlayerData playerData;
    /// <summary>
    /// 初始化系统
    /// </summary>
    private void Start()
    {
        PlayerSystem.Instance.Setup(playerData);
    }
    /// <summary>
    /// 初始化新战斗场景信息
    /// </summary>
    public void NewBattle(List<EnemyData> enemiesD, List<CardData> cardD=null)
    {
        EnemySystem.Instance.Setup(enemiesD);
        CardSystem.Instance.Setup(playerData.Deck);
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
    public void NewBattle(BattleData battle, List<CardData> cardD=null)
    {
        EnemySystem.Instance.Setup(battle.enemies);
        CardSystem.Instance.Setup(playerData.Deck);
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
}
