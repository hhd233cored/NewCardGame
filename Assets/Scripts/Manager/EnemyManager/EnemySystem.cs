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
       foreach(var enemy in Enemies)
       {
            //÷¥––“‚Õº
            GameAction gameAtcion = enemy.IntentionStates[enemy.currentState++].GetGameAction(new List<Character>() { PlayerSystem.Instance.player }, enemy);
            if (enemy.currentState > enemy.IntentionStates.Count - 1) enemy.currentState = 0;
            ActionSystem.Instance.AddReaction(gameAtcion);
            enemy.UpdateIntentionText();
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
