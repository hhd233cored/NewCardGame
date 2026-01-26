using SerializeReferenceEditor;
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

public class GainBlockEffect : Effect
{
    [SerializeField] private int amount;
    public override GameAction GetGameAction(List<Character> targets)
    {
        GainBlockGA gainBlockGA = new(amount, PlayerSystem.Instance.player);
        return gainBlockGA;
    }
}
public class RecoverEffect : Effect
{
    [SerializeField] private int amount;
    public override GameAction GetGameAction(List<Character> targets)
    {
        RecoverGA recoverGA = new(amount, PlayerSystem.Instance.player);
        return recoverGA;
    }
}

public class GainBuffEffect : Effect
{
    [field: SerializeReference, SR] private Buff buff;
    public override GameAction GetGameAction(List<Character> targets)
    {
        GainBuffGA gainBuffGA = new(targets, PlayerSystem.Instance.player, buff);
        return gainBuffGA;
    }
}

