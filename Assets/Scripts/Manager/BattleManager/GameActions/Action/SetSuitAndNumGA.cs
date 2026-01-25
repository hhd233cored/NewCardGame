using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSuitAndNumGA : GameAction
{
    public SuitStyle Suit { get; set; }
    public int Num { get; set; }
    public SetSuitAndNumGA(SuitStyle suit,int num)
    {
        Suit = suit;
        Num = num;
    }
}
