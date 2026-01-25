using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public abstract class Intention
{
    public Character source;
    public abstract GameAction GetGameAction(List<Character> targets);
}

public class DealDamegeIntention : Intention
{
    [SerializeField] private int damgeAmount;
    public override GameAction GetGameAction(List<Character> targets)
    {
        DealDamageGA dealDamageGA = new(damgeAmount, targets, source);
        return dealDamageGA;
    }
}

