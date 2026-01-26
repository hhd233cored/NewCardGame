using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>
/// 普通、精英、boss
/// </summary>
public enum EnemyType { Normal, Elite, Boss }
public class Enemy : Character
{
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private EnemyType type;
    public List<Intention> IntentionStates;
    public int currentState;
    public void Setup(EnemyData data)
    {
        IntentionStates = data.Intentions;
        currentState = 0;
        type = data.Type;
        UpdateIntentionText();
        SetupBase(data.Health, data.Image);
    }
    public void UpdateIntentionText()
    {
        //TODO:显示意图
        attackText.text = "Intention:" + currentState;
    }
}
