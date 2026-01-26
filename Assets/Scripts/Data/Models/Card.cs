using System.Collections.Generic;
using UnityEngine;
public enum SuitStyle { Nul, Diamonds, Clubs, Hearts, Spades }
public enum CardType
{
    Attack,//¹¥»÷ÅÆ
    Skill,//¼¼ÄÜÅÆ
    Power,//ÄÜÁ¦ÅÆ
    Status,//×´Ì¬ÅÆ
    Curses//×çÖäÅÆ
}
[System.Serializable]
public class Card
{
    public string Title => data.name;
    public string Description => data.Description;
    public Sprite Image => data.Image;
    public SuitStyle Suit = SuitStyle.Nul;
    public int Num = 0;
    public Effect ManualTargetEffects => data.ManualTargetEffects;
    public List<AutoTargetEffect> OtherEffects => data.OtherEffects;
    [SerializeField] private CardData data;
    public Card(CardData cardData)
    {
        data = cardData;
    }
}
