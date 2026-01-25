using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Effect
{
    public abstract GameAction GetGameAction(List<Character> targets);
}
public class DealDamegeEffect : Effect
{
    [SerializeField] private int damgeAmount;
    public override GameAction GetGameAction(List<Character> targets)
    {
        DealDamageGA dealDamageGA = new(damgeAmount, targets, PlayerSystem.Instance.player);
        return dealDamageGA;
    }
}
public class DrawCardsEffect : Effect
{
    [SerializeField] private int drawAmount;
    public override GameAction GetGameAction(List<Character> targets)
    {
        DrawCardsGA drawCardsGA = new(drawAmount);
        return drawCardsGA;
    }
}
