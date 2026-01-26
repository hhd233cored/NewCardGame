using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public abstract class Power
{
    protected abstract void OnApply();//当 Buff 刚加上时
    public virtual void OnTick() { }//每回合触发
    public abstract void OnRemove();//当 Buff 消失时
}
