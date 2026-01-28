using System.Collections.Generic;
using UnityEngine;

public enum EventOptionType
{
    Leave,      //离开
    GainGold,   //获得金币
    LoseHealth, //扣血
    RecoverHealth, //回血
    Combat,     //触发战斗
    GainCard    //获得卡牌
}

[System.Serializable]
public class EventOption
{
    public string Description; //按钮上显示的文字
    public EventOptionType Type;

    [Header("参数")]
    public int IntValue; //金币数或回血量
    public BattleData BattleData; //如果是战斗选项，填入敌怪配置
    public CardData CardReward; //如果是卡牌奖励
}

[CreateAssetMenu(menuName = "Data/Event")]
public class EventData : ScriptableObject
{
    public string Title; //事件标题
    [TextArea] public string Content; //事件剧情文本
    public Sprite Image; //左侧显示的图片

    public List<EventOption> Options; //2-4 个选项
}