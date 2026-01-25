using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPlayerGA : GameAction
{
    public Enemy Attacker {  get; private set; }
    public AttackPlayerGA(Enemy attacker)
    {
        Attacker = attacker;
    }
}
