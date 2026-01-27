using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField] private List<Transform> slots;
    public List<Enemy> Enemies { get; private set; } = new();
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
        Enemies.Clear();
        foreach(var enemyData in enemyDatas)
        {
            AddEnemy(enemyData);
        }
    }
    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
        //执行buff结算
        foreach (var enemy in Enemies)
        {
            for (int i = enemy.BuffList.Count - 1; i >= 0; i--)
            {
                var buff = enemy.BuffList[i];
                if (!buff.OnTick())
                {
                    enemy.RemoveBuff(buff);
                }
            }
        }
        foreach (var enemy in Enemies)
       {
            if (enemy == null) continue;
            //执行意图
            List<AutoTargetEffect> aTEfects = enemy.IntentionStates[enemy.currentState].ATEffects;
            foreach(var atf in aTEfects)
            {
                List<Character> targets = atf.TargetMode.GetTargets();
                
                if (targets == null) targets = new() { enemy };
                foreach (var item in targets)
                {
                    Debug.Log(item.name);
                }
                
                PerformEffectGA performEffectGA = new(atf.Effect, targets, enemy);
                ActionSystem.Instance.AddReaction(performEffectGA);
            }
            //ActionSystem.Instance.AddReaction(gameAtcion);
            enemy.currentState++;
            if (enemy.currentState > enemy.IntentionStates.Count - 1) enemy.currentState = 0;
            enemy.UpdateIntentionText(enemy.IntentionStates[enemy.currentState].type.ToString());
        }
       yield return null;
    }
    private IEnumerator AttackPlayerPerformer(AttackPlayerGA attackPlayerGA)
    {
        Character attacker = attackPlayerGA.Attacker;
        Tween tween = attacker.transform.DOMoveX(attacker.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();
        attacker.transform.DOMoveX(attacker.transform.position.x + 1f, 0.25f);

        DealDamageGA dealDamageGA = new(attackPlayerGA.Amount, new() { PlayerSystem.Instance.player }, attacker);
        ActionSystem.Instance.AddReaction(dealDamageGA);
    }
    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        yield return RemoveEnemy(killEnemyGA.Enemy);
    }

    public void AddEnemy(EnemyData enemyData)
    {
        Transform slot = slots[Enemies.Count];
        Enemy enemy = MainController.Instance.CreateEnemy(enemyData, slot.position, slot.rotation);
        enemy.transform.parent = slot;
        Enemies.Add(enemy);
    }
    public IEnumerator RemoveEnemy(Enemy enemy)
    {
        Enemies.Remove(enemy);
        Tween tween = enemy.transform.DOScale(Vector3.zero, 0.25f);
        yield return tween.WaitForCompletion();
        Destroy(enemy.gameObject);
    }
}
