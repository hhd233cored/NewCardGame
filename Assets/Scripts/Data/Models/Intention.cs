using SerializeReferenceEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum IntentionType { Attack, Defend, Buff, Debuff }//在显示对应意图图标时可能要用到？
[System.Serializable]
public class Intention
{
    public IntentionType type;
    [field: SerializeReference, SR] public List<AutoTargetEffect> ATEffects { get; private set; }
}
/*
public class AttackPlayerIntention : Intention
{
    [SerializeField] private int damgeAmount;
    public override GameAction GetGameAction(List<Character> targets ,Character source)
    {
        AttackPlayerGA attackPlayerGA = new(source, damgeAmount);
        return attackPlayerGA;
    }
}

public class GainBlockIntention : Intention
{
    [SerializeField] private int amount;
    public override GameAction GetGameAction(List<Character> targets, Character source)
    {
        GainBlockGA gainBlockGA = new(amount, source);
        return gainBlockGA;
    }
}

public class RecoverIntention : Intention
{
    [SerializeField] private int amount;
    public override GameAction GetGameAction(List<Character> targets, Character source)
    {
        RecoverGA recoverGA = new(amount, source);
        return recoverGA;
    }
}

public class GainBuffIntention : Intention
{
    [field: SerializeReference, SR] private Buff buff;
    public override GameAction GetGameAction(List<Character> targets, Character source)
    {
        GainBuffGA gainBuffGA = new(targets, source, buff);
        return gainBuffGA;
    }
}

public class DealDebuffIntention : Intention
{
    [field: SerializeReference, SR] private Buff buff;
    public override GameAction GetGameAction(List<Character> targets, Character source)
    {
        GainBuffGA gainBuffGA = new(targets, PlayerSystem.Instance.player, buff);
        return gainBuffGA;
    }
}
*/
//TODO：施展负面效果意图、强化自生意图

