using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    None,       //未赋值
    Monster,    //普通怪
    Elite,      //精英怪
    Rest,       //安全屋
    Shop,       //商店
    Event,      //未知事件
    Treasure,   //宝箱
    Boss        //Boss
}

[System.Serializable]
public class MapNode
{
    public int x; //在当前层的索引（横向坐标）
    public int y; //层数（0-15）
    public NodeType nodeType;

    // 连线关系
    [System.NonSerialized] public List<MapNode> parents = new List<MapNode>(); //上一层的连接点
    [System.NonSerialized] public List<MapNode> children = new List<MapNode>(); //下一层的连接点

    //运行时对应的UI对象（生成地图时赋值，用于画线和交互）
    public GameObject uiObject;

    public MapNode(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.nodeType = NodeType.None;
    }
}