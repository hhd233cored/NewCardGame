using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoardView : MonoBehaviour
{
    [SerializeField] private List<Transform> slots;
    public List<Enemy> Enemies { get; private set; } = new();
    public void AddEnemy(EnemyData enemyData)
    {
        Transform slot = slots[Enemies.Count];
        Enemy enemy = EnemyViewCreator.Instance.CreateEnemy(enemyData, slot.position, slot.rotation);
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
