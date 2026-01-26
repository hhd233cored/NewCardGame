using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
//public enum IntentionType { attack, defend, buff, debuff }//在显示对应意图图标时可能要用到？
public abstract class Intention
{
    //public IntentionType type;
    public abstract GameAction GetGameAction(List<Character> targets, Character source);
}

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
//TODO：施展负面效果意图、强化自生意图

