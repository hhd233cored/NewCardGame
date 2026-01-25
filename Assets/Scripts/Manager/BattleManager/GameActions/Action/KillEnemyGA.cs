using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillEnemyGA : GameAction
{
   public Enemy Enemy { get; private set; }
    public KillEnemyGA(Enemy enemy)
    {
        Enemy = enemy;
    }
}
