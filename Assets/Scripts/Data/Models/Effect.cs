using SerializeReferenceEditor;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Effect
{
    public abstract GameAction GetGameAction(List<Character> targets, Character source);
}
public class DealDamegeEffect : Effect//玩家为来源，关系到动画，所以分开来了
{
    [SerializeField] private int damgeAmount;
    public int damage => damgeAmount;
    public override GameAction GetGameAction(List<Character> targets, Character source)
    {
        DealDamageGA dealDamageGA = new(damgeAmount, targets, PlayerSystem.Instance.player);
        return dealDamageGA;
    }
}

public class AttackPlayerEffect : Effect//敌人为来源
{
    [SerializeField] private int damgeAmount;
    public int damage => damgeAmount;
    public override GameAction GetGameAction(List<Character> targets, Character source)
    {
        AttackPlayerGA attackPlayerGA = new(source, damgeAmount);
        return attackPlayerGA;
    }
}

public class DrawCardsEffect : Effect
{
    [SerializeField] private int drawAmount;
    public override GameAction GetGameAction(List<Character> targets, Character source)
    {
        DrawCardsGA drawCardsGA = new(drawAmount);
        return drawCardsGA;
    }
}

public class DiscardCardsEffect : Effect
{
    [SerializeField] private int discardAmount;
    [SerializeField] private bool isChoose;
    public override GameAction GetGameAction(List<Character> targets, Character source)
    {
        DiscardCardsGA discardCardsGA = new(discardAmount, isChoose);
        return discardCardsGA;
    }
}


public class GainBlockEffect : Effect
{
    [SerializeField] private int amount;
    public override GameAction GetGameAction(List<Character> targets, Character source)
    {
        GainBlockGA gainBlockGA = new(amount, targets);
        return gainBlockGA;
    }
}
public class RecoverEffect : Effect
{
    [SerializeField] private int amount;
    public override GameAction GetGameAction(List<Character> targets, Character source)
    {
        RecoverGA recoverGA = new(amount, targets);
        return recoverGA;
    }
}

public class GainBuffEffect : Effect
{
    [field: SerializeReference, SR] private Buff buff;
    public override GameAction GetGameAction(List<Character> targets, Character source)
    {
        GainBuffGA gainBuffGA = new(targets, source, buff);
        return gainBuffGA;
    }
}

