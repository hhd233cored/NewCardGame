using SerializeReferenceEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Battle")]
public class BattleData : ScriptableObject
{
    [field: SerializeReference] public List<EnemyData> enemies { get; private set; }
    //TODO:敌怪可能开局携带buff？
}
