using SerializeReferenceEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Card")]
public class CardData : ScriptableObject
{
    [field: SerializeField] public string Title { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public int Rarity { get; private set; }
    [field: SerializeField] public int Num { get; private set; }//新增：数字
    [field: SerializeField] public SuitStyle Suit { get; private set; }//新增：花色
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public CardType CardType { get; private set; }
    [field: SerializeField] public bool Exhaust { get; private set; }//消耗
    [field: SerializeField] public bool Ethereal { get; private set; }//虚无
    [field: SerializeField] public bool CantPlay { get; private set; }//不能打出
    [field: SerializeReference, SR] public List<Effect> ManualTargetEffects { get; private set; } = null;
    [field: SerializeReference, SR] public List<AutoTargetEffect> OtherEffects { get; private set; } = null;
}
