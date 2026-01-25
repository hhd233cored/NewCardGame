using System.Collections.Generic;
using UnityEngine;
public enum SuitStyle { Nul, Diamonds, Clubs, Hearts, Spades }
public class Card
{
    public string Title => data.name;
    public string Description => data.Description;
    public Sprite Image => data.Image;
    public SuitStyle Suit = SuitStyle.Nul;
    public int Num = 0;
    public Effect ManualTargetEffects => data.ManualTargetEffects;
    public List<AutoTargetEffect> OtherEffects => data.OtherEffects;
    private readonly CardData data;
    public Card(CardData cardData)
    {
        data = cardData;
    }
}
