using SerializeReferenceEditor;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Effect
{
    public abstract GameAction GetGameAction(List<Character> targets, Character source);
}
public class DealDamegeEffect : Effect
{
    [SerializeField] private int damgeAmount;
    public int damage => damgeAmount;
    public override GameAction GetGameAction(List<Character> targets, Character source)
    {
        DealDamageGA dealDamageGA = new(damgeAmount, targets, PlayerSystem.Instance.player);
        return dealDamageGA;
    }
}

public class AttackPlayerEffect : Effect
{
    [SerializeField] private int damgeAmount;
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

