using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;

public class MainController:Singleton<MainController>
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private BattleData battleData;
    [SerializeField] private BuffData strengthData;
    [SerializeField] private BuffData dexterityData;
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
        BattleSystem.Instance.ResetDir();

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

        //Debug.Log(source.name + "-basicDamage:" + basicDamage + "-power:" + power?.stacks + "-damage:" + damage);
        if (damage <= 0) damage = 1;

        return damage;
    }

    public int TotalBlock(int basicBlock, List<Character> targets, Character source)
    {
        int block = basicBlock;
        Buff Dex = source.BuffList.Find(buff => buff.data == dexterityData);
        if (Dex != null) block += Dex.stacks * 2;

        if(block <= 0) block = 1;

        return block;
    }

    /// <summary>
    /// 获得防御
    /// </summary>
    public static GameAction Block(int amount, List<Character> targets)
    {
        GainBlockGA gainBlockGA = new(amount, targets);
        return gainBlockGA;
    }
    /// <summary>
    /// 回血
    /// </summary>
    public static GameAction Recover(int amount, List<Character> targets)
    {
        RecoverGA recoverGA = new(amount, targets);
        return recoverGA;
    }
    /// <summary>
    /// 获得buff
    /// </summary>
    public static GameAction AddBuff(List<Character> targets, Character source, Buff buff)
    {
        GainBuffGA gainBuffGA = new(targets, source, buff);
        return gainBuffGA;
    }
    /// <summary>
    /// 玩家对目标造成伤害，因为会影响动画所以和敌人分开
    /// </summary>
    public static GameAction PlayerDealDamage(int amount, List<Character> targets)
    {
        DealDamageGA dealDamageGA = new(amount, targets, PlayerSystem.Instance.player);
        return dealDamageGA;
    }
    /// <summary>
    /// 敌人对玩家造成伤害
    /// </summary>
    public static GameAction EnemyDealDamage(int amount,Character source)
    {
        AttackPlayerGA attackPlayerGA = new(source, amount);
        return attackPlayerGA;
    }
    /// <summary>
    /// 摸牌
    /// </summary>
    public static GameAction Draw(int amount)
    {
        DrawCardsGA drawCardsGA = new(amount);
        return drawCardsGA;
    }
    /// <summary>
    /// 弃牌，不足则会全弃
    /// </summary>
    /// <param name="isChoose">true为玩家选择弃置若干张牌，false为随机弃置。</param>
    /// <returns></returns>
    public static GameAction Discard(int amount,bool isChoose)
    {
        DiscardCardsGA discardCardsGA = new(amount, isChoose);
        return discardCardsGA;
    }
}
