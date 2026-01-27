using SerializeReferenceEditor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
public enum SuitStyle { Nul, Diamonds, Clubs, Hearts, Spades }
public enum CardType
{
    Attack,//攻击牌
    Skill,//技能牌
    Power,//能力牌
    Status,//状态牌
    Curses//诅咒牌
}
[System.Serializable]
public class Card
{
    public string Title => data.name;
    public string Description;
    public Sprite Image => data.Image;
    public SuitStyle Suit = SuitStyle.Nul;
    public int Num = 0;
    public Effect ManualTargetEffects => data.ManualTargetEffects;
    public List<AutoTargetEffect> OtherEffects => data.OtherEffects;
    [SerializeField] private CardData data;
    //[field: SerializeReference, SR] Power power = null;//能力牌的能力
    public Card(CardData cardData)
    {
        data = cardData;
        Description = GetDynamicDescription(false);
    }

    public string GetDynamicDescription(bool countTotal = true)
    {
        string desc = data.Description; // 获取原始模板

        desc = ResetDamageDes(ManualTargetEffects, desc);
        if (desc != data.Description) return desc;

        foreach (var ATEffect in OtherEffects)
        {
            desc = ResetDamageDes(ATEffect.Effect, desc, countTotal);
            if (desc != data.Description) return desc;
        }
        return desc;
    }

    public string ResetDamageDes(Effect effect, string des, bool countTotal = true)
    {
        string desc = des;
        // 1. 获取基础伤害数值
        if (effect is DealDamegeEffect damageEffect) //
        {
            int baseDamage = damageEffect.damage;

            // 2. 调用战斗系统的算法计算最终伤害
            // 假设你有一个 BuffSystem.CalculateDamage(int baseVal)

            int finalDamage = baseDamage;
            if (BattleSystem.Instance != null && countTotal)
            {
                // 这里接入增益计算逻辑
                finalDamage = MainController.Instance.TotalDamage(baseDamage, null, PlayerSystem.Instance.player);
            }

            // 3. 替换占位符，并根据是否改变颜色
            string colorTag = finalDamage > baseDamage ? "<color=green>" : (finalDamage < baseDamage ? "<color=red>" : "<color=black>");
            desc = desc.Replace("{D}", $"{colorTag}{finalDamage}</color>");
        }
        return desc;
    }
}
