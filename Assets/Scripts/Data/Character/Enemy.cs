using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enemy : Character
{
    [SerializeField] private TMP_Text attackText;
    public List<Intention> IntentionStates;
    public int currentState;
    public void Setup(EnemyData data)
    {
        IntentionStates = data.Intention;
        currentState = 0;
        UpdateAttackText();
        SetupBase(data.Health, data.Image);
    }
    public void UpdateAttackText()
    {
        //TODO:œ‘ æ“‚Õº
        attackText.text = "Intention:" + currentState;
    }
}
