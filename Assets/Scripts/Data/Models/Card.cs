using SerializeReferenceEditor;
using System.Collections.Generic;
using UnityEngine;
public enum SuitStyle { Nul, Diamonds, Clubs, Hearts, Spades }
public enum CardType
{
    Attack,//攻击牌
    Skill,//技能牌
    Power,//能力牌
    Status,//状态牌
    Curses//诅咒牌
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
    //[field: SerializeReference, SR] Power power = null;//能力牌的能力
    public Card(CardData cardData)
    {
        data = cardData;
    }
}
