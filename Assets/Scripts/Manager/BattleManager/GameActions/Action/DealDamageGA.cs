using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
