using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardCardsGA : GameAction
{
    public int Amount { get; set; }
    public bool IsChoose { get; set; }
    public DiscardCardsGA(int amount, bool isChoose)
    {
        Amount = amount;
        IsChoose = isChoose;
    }
}
