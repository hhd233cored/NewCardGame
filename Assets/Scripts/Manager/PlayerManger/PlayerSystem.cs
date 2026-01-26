using SerializeReferenceEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSystem : Singleton<PlayerSystem>
{
    [field: SerializeField] public Player player { get; set; }
    public List<Card> CurrentCards;
    public void Setup(PlayerData data)
    {
        //基础设置
        player.Setup(data);
        //设置初始卡组
        CurrentCards = new();
        foreach (var cardData in data.Deck)
        {
            Card card = new(cardData);
            switch ((int)UnityEngine.Random.Range(1, 5))
            {
                case 1:
                    card.Suit = SuitStyle.Diamonds;
                    break;
                case 2:
                    card.Suit = SuitStyle.Clubs;
                    break;
                case 3:
                    card.Suit = SuitStyle.Hearts;
                    break;
                case 4:
                    card.Suit = SuitStyle.Spades;
                    break;
                default:
                    card.Suit = SuitStyle.Nul;
                    break;
            }
            card.Num = UnityEngine.Random.Range(1, 14);
            CurrentCards.Add(card);
        }
    }
}
