using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public List<Card> CurrentCards => PlayerSystem.Instance.CurrentCards;
    public void Setup(PlayerData data)
    {
        SetupBase(data.Health, data.Image);
    }
}
