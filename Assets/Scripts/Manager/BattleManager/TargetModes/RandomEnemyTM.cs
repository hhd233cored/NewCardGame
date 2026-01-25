using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEnemyTM : TargetMode
{
    public override List<Character> GetTargets()
    {
        Character target = EnemySystem.Instance.Enemies[Random.Range(0, EnemySystem.Instance.Enemies.Count)];
        return new() { target };
    }
}
