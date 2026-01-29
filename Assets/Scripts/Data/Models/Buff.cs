using SerializeReferenceEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
public enum BuffType { positive, negative }
public abstract class Buff
{
    protected Character owner;//拥有者
    protected List<Character> owner2 => new List<Character>() { owner };
    public int stacks;//层数
    public BuffData data;//ScriptableObject
    public List<BuffData> AddBuffData;//SO，用于Buff中添加Buff

    public virtual void Initialize(Character target, int initialStacks, BuffData buffData, List<BuffData> add)
    {
        owner = target;
        stacks = initialStacks;
        data = buffData;
        AddBuffData = add;
        OnApply();
    }

    //子类必须实现或选择性实现的生命周期
    protected abstract void OnApply();//当 Buff 刚加上时
    public virtual bool OnTick() { return true; }//每回合触发
    public abstract void OnRemove();//当 Buff 消失时

    //层数管理
    public virtual void AddStacks(int amount)
    {
        stacks += amount;
        //可以在这里触发层数变化的 UI 动画动作
    }
}

public class ThornsEffect : Buff
{

    protected override void OnApply()
    {
        //订阅：每当有伤害动作发生后，检查自己
        ActionSystem.SubscribePost<DealDamageGA>(OnDamageOccurred);
    }

    private void OnDamageOccurred(DealDamageGA dealDamageGA)
    {
        foreach (var target in dealDamageGA.Targets)

        {
            // 如果受伤的目标是我自己
            if (target == owner)
            {
                Debug.Log($"{owner.name} 触发了反伤！");
                //向当前动作链追加一个反伤动作
                ActionSystem.Instance.AddReaction(new DealDamageGA(5, new List<Character>() { dealDamageGA.Source }, owner));
            }
        }
    }

    public override void OnRemove()
    {
        //取消订阅
        ActionSystem.UnsubscribePost<DealDamageGA>(OnDamageOccurred);
    }
}

public class PoisonBuff : Buff
{
    protected override void OnApply()
    {
        Debug.Log($"{owner.name} 中毒了！");
    }

    public override bool OnTick()
    {
        // 每一轮开始时，直接调用 ActionSystem 执行扣血
        ActionSystem.Instance.AddReaction(new DealDamageGA(stacks, new List<Character>() { owner }, PlayerSystem.Instance.player));

        // 中毒每回合层数减一
        stacks--;
        return stacks > 0;
    }

    public override void OnRemove()
    {
        Debug.Log($"{owner.name} 的毒解了。");
    }
}

public class StrengthBuff : Buff
{
    public override void OnRemove()
    {
        
    }

    protected override void OnApply()
    {
        Debug.Log($"{owner.name} 获得力量！");
    }
}

public class DexterityBuff : Buff//敏捷
{
    public override void OnRemove()
    {

    }

    protected override void OnApply()
    {
        Debug.Log($"{owner.name} 敏捷！");
    }
}

public class WeakBuff : Buff//虚弱，造成伤害减少25%
{
    protected override void OnApply()
    {

    }

    public override bool OnTick()
    {
        // 每回合层数减一
        stacks--;
        return stacks > 0;
    }

    public override void OnRemove()
    {

    }
}
public class VulnerableBuff : Buff//易伤，受到伤害增加50%
{
    protected override void OnApply()
    {

    }

    public override bool OnTick()
    {
        // 每回合层数减一
        stacks--;
        return stacks > 0;
    }

    public override void OnRemove()
    {

    }
}

public class RitualBuff : Buff//仪式，每回合增加等同于层数的力量
{
    protected override void OnApply()
    {

    }
    
    public override bool OnTick()
    {
        if (AddBuffData.Count > 0)
        {
            StrengthBuff strengthBuff = new();
            strengthBuff.stacks = this.stacks;
            strengthBuff.data = AddBuffData[0];
            ActionSystem.Instance.AddReaction(MainController.AddBuff(owner2, owner, strengthBuff));
        }
        return stacks > 0;
    }

    public override void OnRemove()
    {

    }
}

public class PlatedArmorBuff : Buff//金属化，每回合增加等同于层数的格挡
{
    protected override void OnApply()
    {

    }

    public override bool OnTick()
    {
        ActionSystem.Instance.AddReaction(MainController.Block(stacks, owner2));

        return stacks > 0;
    }

    public override void OnRemove()
    {

    }
}

