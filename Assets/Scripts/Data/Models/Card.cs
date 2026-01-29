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
    public string Title => data.Title;
    public string Description;
    public Sprite Image => data.Image;
    public SuitStyle Suit = SuitStyle.Nul;
    public int Num = 0;
    public List<Effect> ManualTargetEffects => data.ManualTargetEffects;
    public List<AutoTargetEffect> OtherEffects => data.OtherEffects;
    public CardData data { get; private set; }
    //[field: SerializeReference, SR] Power power = null;//能力牌的能力
    public Card(CardData cardData)
    {
        data = cardData;
        Suit = data.Suit;
        Num = data.Num;
        Description = GetDynamicDescription(false);
    }

    public string GetDynamicDescription(bool countTotal = true)
    {
        string desc = data.Description; // 获取原始模板
        string tempDes = desc;
        foreach (Effect effect in ManualTargetEffects)
        {
            tempDes = ResetDes(effect, desc);
            if (tempDes == "Nul")
                continue;
            else
                desc = tempDes;
        }

        foreach (var ATEffect in OtherEffects)
        {
            tempDes = ResetDes(ATEffect.Effect, desc);
            if (tempDes == "Nul")
                continue;
            else
                desc = tempDes;
        }
        return desc;
    }

    public string ResetDes(Effect effect, string des, bool countTotal = true)
    {
        string desc = des;
        // 1. 获取基础伤害数值
        if (effect is DealDamegeEffect damageEffect) //改伤害文本
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

        if (effect is GainBlockEffect gainBlockEffect) //改格挡文本
        {
            int baseBlock = gainBlockEffect.block;

            // 2. 调用战斗系统的算法计算最终伤害
            // 假设你有一个 BuffSystem.CalculateDamage(int baseVal)

            int finalBlock = baseBlock;
            if (BattleSystem.Instance != null && countTotal)
            {
                // 这里接入增益计算逻辑
                finalBlock = MainController.Instance.TotalBlock(baseBlock, null, PlayerSystem.Instance.player);
            }

            // 3. 替换占位符，并根据是否改变颜色
            string colorTag = finalBlock > baseBlock ? "<color=green>" : (finalBlock < baseBlock ? "<color=red>" : "<color=black>");
            desc = desc.Replace("{B}", $"{colorTag}{finalBlock}</color>");
        }
        return desc == des ? "Nul" : desc;
    }
}
