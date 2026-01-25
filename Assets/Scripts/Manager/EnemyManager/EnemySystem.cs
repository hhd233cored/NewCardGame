using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
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
        GameAction gameAtcion = attacker.IntentionStates[attacker.currentState++].GetGameAction(new List<Character>() { PlayerSystem.Instance.player });
        if (attacker.currentState > attacker.IntentionStates.Count - 1) attacker.currentState = 0;
        attacker.UpdateAttackText();
        ActionSystem.Instance.AddReaction(gameAtcion);
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
