using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DealDamegeEffect : Effect
{
    [SerializeField] private int damgeAmount;
    public override GameAction GetGameAction(List<Character> targets)
    {
        DealDamageGA dealDamageGA = new(damgeAmount, targets, PlayerSystem.Instance.player);
        return dealDamageGA;
    }
}
