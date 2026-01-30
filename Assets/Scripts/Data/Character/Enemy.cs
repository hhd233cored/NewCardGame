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
    public int EnemyIntentionRestartpoint;
    public void Setup(EnemyData data)
    {
        IntentionStates = data.Intentions;
        currentState = data.IntentionStart;
        type = data.Type;
        EnemyIntentionRestartpoint = data.IntentionRestart;
        UpdateIntentionText();

        SetupBase(data.Health, data.Image);
    }
    public int GetAttackIntentionDamage()
    {
        int attackDamange = 0;
        int temp = 0;
        if (IntentionStates[currentState].type == IntentionType.Attack)
        {
            foreach (var ate in IntentionStates[currentState].ATEffects)
            {
                if (ate.Effect is AttackPlayerEffect effect)
                {
                    temp = effect.damage;
                    temp = MainController.Instance.TotalDamage(temp, new List<Character>() { PlayerSystem.Instance.player }, this);
                    attackDamange+= temp;

                }

                if (ate.Effect is DealDamegeEffect effect2)
                {
                    temp = effect2.damage;
                    temp = MainController.Instance.TotalDamage(temp, new List<Character>() { PlayerSystem.Instance.player }, this);
                    attackDamange += temp;
                }
            }
        }
        
        return attackDamange;
    }
    public void UpdateIntentionText()
    {
        int damage = GetAttackIntentionDamage();
        string text = IntentionStates[currentState].type.ToString();
        //TODO:显示意图
        attackText.text = damage == 0 ? text : text + ":" + damage;
    }
}
