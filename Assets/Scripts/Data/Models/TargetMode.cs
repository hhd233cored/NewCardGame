using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public abstract class TargetMode
{
    public abstract List<Character> GetTargets();
}

public class AllEnemiesTM : TargetMode
{
    public override List<Character> GetTargets()
    {
        return new(EnemySystem.Instance.Enemies);
    }
}

public class SelfTM : TargetMode
{
    public override List<Character> GetTargets()
    {
        return null;
    }
}

public class RandomEnemyTM : TargetMode
{
    public override List<Character> GetTargets()
    {
        Character target = EnemySystem.Instance.Enemies[Random.Range(0, EnemySystem.Instance.Enemies.Count)];
        return new() { target };
    }
}

public class PlayerTM : TargetMode
{
    public override List<Character> GetTargets()
    {
        return new() { PlayerSystem.Instance.player };
    }
}
