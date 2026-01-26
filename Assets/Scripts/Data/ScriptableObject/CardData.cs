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
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public CardType CardType { get; private set; }
    [field: SerializeReference, SR] public Effect ManualTargetEffects { get; private set; } = null;
    [field: SerializeReference, SR] public List<AutoTargetEffect> OtherEffects { get; private set; } = null;
}
