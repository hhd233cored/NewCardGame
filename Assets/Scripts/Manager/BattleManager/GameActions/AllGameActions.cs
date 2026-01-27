using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPlayerGA : GameAction
{
    public Character Attacker {  get; private set; }
    public int Amount { get; set; }
    public AttackPlayerGA(Character attacker, int amount)
    {
        Attacker = attacker;
        Amount = amount;
    }
}

public class DealDamageGA : GameAction
{
    public int Amount { get; set; }
    public List<Character> Targets { get; set; }
    public Character Source { get; set; }

    public DealDamageGA(int amount, List<Character> targets, Character source)
    {
        Amount = amount;
        Targets = targets;
        Source = source;
    }
}

public class DiscardCardsGA : GameAction
{
    public int Amount { get; set; }
    public bool IsChoose { get; set; }
    public DiscardCardsGA(int amount, bool isChoose)
    {
        Amount = amount;
        IsChoose = isChoose;
    }
}

public class DrawCardsGA : GameAction
{
    public int Amount { get; set; }
    public DrawCardsGA(int amount)
    {
        Amount = amount;
    }
}

public class EnemyTurnGA : GameAction
{

}

public class KillEnemyGA : GameAction
{
    public Enemy Enemy { get; private set; }
    public KillEnemyGA(Enemy enemy)
    {
        Enemy = enemy;
    }
}

public class PerformEffectGA : GameAction
{
    public Effect Effect { get; set; }
    public List<Character> Targets { get; set; }
    public Character Source { get; set; }
    public PerformEffectGA(Effect effect, List<Character> targets, Character source)
    {
        Effect = effect;
        Targets = targets;
        Source = source;
    }
}

public class PlayCardGA : GameAction
{
    public Enemy Target { get; private set; }
    public CardView CardView { get; }
    public PlayCardGA(CardView cardView)
    {
        CardView = cardView;
        Target = null;
    }
    public PlayCardGA(CardView cardView, Enemy target)
    {
        CardView = cardView;
        Target = target;
    }
}

public class SetSuitAndNumGA : GameAction
{
    public SuitStyle Suit { get; set; }
    public int Num { get; set; }
    public SetSuitAndNumGA(SuitStyle suit, int num)
    {
        Suit = suit;
        Num = num;
    }
}

public class GainBlockGA : GameAction
{
    public int Amount { get; set; }
    public List<Character> Target { get; set; }
    public GainBlockGA(int amount, Character target)
    {
        Amount = amount;
        Target = new() { target };
    }
    public GainBlockGA(int amount, List<Character> target)
    {
        Amount = amount;
        Target = target;
    }
}

public class RecoverGA : GameAction
{
    public int Amount { get; set; }
    public List<Character> Target { get; set; }
    public RecoverGA(int amount, Character target)
    {
        Amount = amount;
        Target = new() { target };
    }
    public RecoverGA(int amount, List<Character> target)
    {
        Amount = amount;
        Target = target;
    }
}

public class GainBuffGA : GameAction
{
    public List<Character> Targets {  get; set; }
    public Character User { get; set; }
    public Buff Buff { get; set; }
    public GainBuffGA(Character targets, Character user, Buff buff)
    {
        Targets = new() { targets };
        User = user;
        Buff = buff;
    }
    public GainBuffGA(List<Character> targets, Character user, Buff buff)
    {
        Targets = targets;
        User = user;
        Buff = buff;
    }
}
public class ChangeGoldGA : GameAction
{
    public int Amount { get; private set; }

    //source可以是null，也可以是来源（比如战斗奖励、宝箱、商店）
    public object Source { get; private set; }

    public ChangeGoldGA(int amount, object source = null)
    {
        Amount = amount;
        Source = source;
    }
}
/*
public class AllBuffsTickGA : GameAction//仅作为一个状态机用于处理回合前buff结算
{
    public bool isPlayerTurn;
    public AllBuffsTickGA(bool isPlayerTurn)
    {
        this.isPlayerTurn = isPlayerTurn;
    }
}
<<<<<<< Updated upstream


=======
*/
