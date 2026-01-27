using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public List<Card> CurrentCards => PlayerSystem.Instance.CurrentCards;
    public int CurrentGold { get; private set; }
    public void Setup(PlayerData data)
    {
        SetupBase(data.Health, data.Image);
        CurrentGold = data.StartingGold;
    }

    //ÐÞ¸Ä½ð±Ò
    public void ChangeGold(int amount)
    {
        CurrentGold += amount;
        if (CurrentGold < 0) CurrentGold = 0;
    }
}
