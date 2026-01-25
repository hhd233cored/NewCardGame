using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyViewCreator : Singleton<EnemyViewCreator>
{
    [SerializeField] private Enemy enemyPrefab;
    public Enemy CreateEnemy(EnemyData enemyData, Vector3 position, Quaternion rotation)
    {
        Enemy enemy = Instantiate(enemyPrefab, position, rotation);
        enemy.Setup(enemyData);
        return enemy;
    }
}
