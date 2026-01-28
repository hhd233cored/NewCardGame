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
    public int GetAttackIntentionDamage()
    {
        int attackDamange = 0;
        if (IntentionStates[0].type == IntentionType.Attack)
        {
            foreach (var ate in IntentionStates[currentState].ATEffects)
            {
                if (ate.Effect is AttackPlayerEffect effect)
                {
                    attackDamange = effect.damage;
                    attackDamange = MainController.Instance.TotalDamage(attackDamange, new List<Character>() { PlayerSystem.Instance.player }, this);

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
