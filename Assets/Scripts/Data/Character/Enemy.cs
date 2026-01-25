using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enemy : Character
{
    [SerializeField] private TMP_Text attackText;
    public int AttackPower { get; set; }
    public void Setup(EnemyData data)
    {
        AttackPower = data.AttackPower;
        UpdateAttackText();
        SetupBase(data.Health, data.Image);
    }
    private void UpdateAttackText()
    {
        attackText.text = "ATK:" + AttackPower;
    }
}
