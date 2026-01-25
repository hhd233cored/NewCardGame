using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformEffectGA : GameAction
{
    public Effect Effect { get; set; }
    public List<Character> Targets { get; set; }
    public PerformEffectGA(Effect effect,List<Character>targets)
    {
        Effect = effect;
        Targets = targets == null ? null : new(targets);
    }
}
