using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCardGA : GameAction
{
    public Enemy Target { get; private set; }
    public CardView CardView { get; }
    public PlayCardGA(CardView cardView)
    {
        CardView = cardView;
        Target = null;
    }
    public PlayCardGA(CardView cardView,Enemy target)
    {
        CardView = cardView;
        Target = target;
    }
}
