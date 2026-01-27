using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName ="Data/Player")]
public class PlayerData : ScriptableObject
{
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public int Health { get; private set; }
    [field:SerializeField]public List<CardData> Deck { get; private set; }

    [field: SerializeField] public int StartingGold { get; private set; } = 99; //³õÊ¼½ð±Ò

}
