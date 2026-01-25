using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPlayerGA : GameAction
{
    public Enemy Attacker {  get; private set; }
    public AttackPlayerGA(Enemy attacker)
    {
        Attacker = attacker;
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
    public PerformEffectGA(Effect effect, List<Character> targets)
    {
        Effect = effect;
        Targets = targets == null ? null : new(targets);
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
