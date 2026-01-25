using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : Character
{
    public void Setup(PlayerData data)
    {
        SetupBase(data.Health, data.Image);
    }
}
