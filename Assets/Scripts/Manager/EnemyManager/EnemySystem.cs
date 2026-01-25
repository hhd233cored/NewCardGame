using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField] private EnemyBoardView enemyBoardView;
    public List<Enemy> Enemies => enemyBoardView.Enemies;
    private void OnEnable()
    {
        ActionSystem.RegisterPerformer<EnemyTurnGA>(this, EnemyTurnPerformer);
        ActionSystem.RegisterPerformer<AttackPlayerGA>(this, AttackPlayerPerformer);
        ActionSystem.RegisterPerformer<KillEnemyGA>(this, KillEnemyPerformer);
    }
    private void OnDisable()
    {
        int removed = ActionSystem.UnregisterPerformersByOwner(this);
        // Debug.Log($"Removed performers: {removed}");
    }
    public void Setup(List<EnemyData> enemyDatas)
    {
        foreach(var enemyData in enemyDatas)
        {
            enemyBoardView.AddEnemy(enemyData);
        }
    }
    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
       foreach(var enemy in enemyBoardView.Enemies)
       {
            AttackPlayerGA attackPlayerGA = new(enemy);
            ActionSystem.Instance.AddReaction(attackPlayerGA);
       }
       yield return null;
    }
    private IEnumerator AttackPlayerPerformer(AttackPlayerGA attackPlayerGA)
    {
        Enemy attacker = attackPlayerGA.Attacker;
        Tween tween = attacker.transform.DOMoveX(attacker.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();
        attacker.transform.DOMoveX(attacker.transform.position.x + 1f, 0.25f);
        //TODO:造成伤害效果
        DealDamageGA dealDamageGA = new(attacker.AttackPower, new() { PlayerSystem.Instance.player }, attacker);
        ActionSystem.Instance.AddReaction(dealDamageGA);
    }
    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        yield return enemyBoardView.RemoveEnemy(killEnemyGA.Enemy);
    }
}
