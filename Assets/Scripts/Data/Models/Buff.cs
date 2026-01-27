using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
public enum BuffType { positive, negative }
public abstract class Buff
{
    protected Character owner;//拥有者
    public int stacks;//层数
    public BuffData data;//ScriptableObject
    public BuffType type;

    public virtual void Initialize(Character target, int initialStacks, BuffData buffData)
    {
        type = buffData.Type;
        owner = target;
        stacks = initialStacks;
        data = buffData;
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
                ActionSystem.Instance.AddReaction(new DealDamageGA(5, new List<Character>() { target }, owner));
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

public class PowerBuff : Buff
{
    public override void OnRemove()
    {
        
    }

    protected override void OnApply()
    {
        Debug.Log($"{owner.name} 获得力量！");
    }
}
