using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TreeEditor.TreeEditorHelper;

public class MapGenerator : MonoBehaviour
{
    [Header("生成配置")]
    [SerializeField] private int totalLayers = 16;
    [SerializeField] private int minWidth = 6;
    [SerializeField] private int maxWidth = 7;
    [SerializeField] private int startNodeCountMin = 3;
    [SerializeField] private int startNodeCountMax = 4;

    [Header("路径校验参数")]
    [SerializeField] private int minShopsPerPath = 2;
    [SerializeField] private int maxShopsPerPath = 3;
    [SerializeField] private int minElitesPerPath = 1;
    [SerializeField] private int maxElitesPerPath = 4;

    //对外接口：生成并返回地图数据
    public List<List<MapNode>> GenerateMap()
    {
        var map = GenerateStructure();

        // 2. 填充类型（规则 + 随机）
        AssignNodeTypes(map);

        // 3. 验证并修正路径（保底机制）
        ValidateAndFixPaths(map);

        return map;
    }

    //生成结构
    private List<List<MapNode>> GenerateStructure()
    {
        List<List<MapNode>> map = new List<List<MapNode>>();

        //生成节点
        for (int y = 0; y < totalLayers; y++)
        {
            List<MapNode> layerNodes = new List<MapNode>();

            int width = 0;
            if (y == 0) width = Random.Range(startNodeCountMin, startNodeCountMax + 1); // 第1层
            else if (y == totalLayers - 1) width = 1; // Boss层
            else width = Random.Range(minWidth, maxWidth + 1); // 中间层

            for (int x = 0; x < width; x++)
            {
                layerNodes.Add(new MapNode(x, y));
            }
            map.Add(layerNodes);
        }

        //生成连线
        for (int y = 0; y < totalLayers - 1; y++)
        {
            ConnectLayers(map[y], map[y + 1]);
        }

        return map;
    }

    private void ConnectLayers(List<MapNode> currentLayer, List<MapNode> nextLayer)
    {
        //策略：
        //确保 CurrentLayer 的每个点都有去处
        //确保 NextLayer 的每个点都有来源
        //连线尽量自然


        float GetNormX(MapNode node, int count) => count == 1 ? 0.5f : (float)node.x / (count - 1);

        //正向连接：每个当前层节点找 1-2 个下层目标
        foreach (var node in currentLayer)
        {
            //找到下层中位置最近的几个点
            var candidates = nextLayer.OrderBy(n => Mathf.Abs(GetNormX(n, nextLayer.Count) - GetNormX(node, currentLayer.Count))).Take(3).ToList();

            //随机选 1-2 个连接，偏向最近的
            int connectCount = Random.Range(1, 3); // 连 1 或 2 条线
            //至少连一个最近的
            Connect(node, candidates[0]);

            if (connectCount > 1 && candidates.Count > 1)
            {
                //50%概率再连一个次近的
                if (Random.value > 0.5f) Connect(node, candidates[1]);
            }
        }

        //反向检查：确保下一层每个点都有父亲
        foreach (var nextNode in nextLayer)
        {
            if (nextNode.parents.Count == 0)
            {
                var parent = currentLayer.OrderBy(n => Mathf.Abs(GetNormX(n, currentLayer.Count) - GetNormX(nextNode, nextLayer.Count))).First();
                Connect(parent, nextNode);
            }
        }
    }

    private void Connect(MapNode parent, MapNode child)
    {
        if (!parent.children.Contains(child)) parent.children.Add(child);
        if (!child.parents.Contains(parent)) child.parents.Add(parent);
    }

    //填充类型
    private void AssignNodeTypes(List<List<MapNode>> map)
    {
        for (int y = 0; y < map.Count; y++)
        {
            foreach (var node in map[y])
            {
                node.nodeType = GetNodeTypeByRules(y, node);
            }
        }
    }

    private NodeType GetNodeTypeByRules(int layerIndex, MapNode node)
    {
        //固定规则
        if (layerIndex == totalLayers - 1) return NodeType.Boss; //16层
        if (layerIndex == totalLayers - 2) return NodeType.Rest; //15层
        if (layerIndex == 8) return NodeType.Treasure;           //9层 (Index 8)
        if (layerIndex == 0) return NodeType.Monster;            //1层

        //动态规则
        List<NodeType> candidates = new List<NodeType>();

        //基础池子
        candidates.Add(NodeType.Monster);
        candidates.Add(NodeType.Event);

        //商店不能连续遇到
        bool parentHasShop = node.parents.Any(p => p.nodeType == NodeType.Shop);
        //前5层商店概率低(这里通过不加入候选列表或低权重来实现，简单起见前3层不刷)
        if (!parentHasShop && layerIndex > 2) candidates.Add(NodeType.Shop);

        //精英第6层(Index 5)开始生成
        if (layerIndex >= 5) candidates.Add(NodeType.Elite);

        // 安全屋不能连续，且第15层已有固定，中间层可以随机少量
        bool parentHasRest = node.parents.Any(p => p.nodeType == NodeType.Rest);
        if (!parentHasRest && layerIndex > 5 && layerIndex < 13) candidates.Add(NodeType.Rest);

        //简单的权重随机
        return RollWeightedType(layerIndex, candidates);
    }

    private NodeType RollWeightedType(int layerIndex, List<NodeType> candidates)
    {
        List<NodeType> weightedList = new List<NodeType>();
        foreach (var type in candidates)
        {
            int weight = 10;
            if (type == NodeType.Monster) weight = 45;
            if (type == NodeType.Event) weight = 22;
            if (type == NodeType.Elite) weight = 16;
            if (type == NodeType.Shop)
            {
                // 前5层极低
                weight = layerIndex < 5 ? 2 : 5;
            }
            if (type == NodeType.Rest) weight = 12;

            for (int i = 0; i < weight; i++) weightedList.Add(type);
        }

        return weightedList[Random.Range(0, weightedList.Count)];
    }

    //校验修正
    private void ValidateAndFixPaths(List<List<MapNode>> map)
    {
        //模拟100次爬塔路径
        for (int i = 0; i < 100; i++)
        {
            List<MapNode> path = GetRandomPath(map);
            CheckAndFixPath(path);
        }
    }

    private List<MapNode> GetRandomPath(List<List<MapNode>> map)
    {
        List<MapNode> path = new List<MapNode>();
        var current = map[0][Random.Range(0, map[0].Count)];
        path.Add(current);

        while (current.children.Count > 0)
        {
            current = current.children[Random.Range(0, current.children.Count)];
            path.Add(current);
        }
        return path;
    }

    private void CheckAndFixPath(List<MapNode> path)
    {
        int eliteCount = path.Count(n => n.nodeType == NodeType.Elite);
        int shopCount = path.Count(n => n.nodeType == NodeType.Shop);

        //修正精英怪数量
        if (eliteCount < minElitesPerPath)
        {
            //找一个6层以上的普通怪改成精英
            var target = path.FirstOrDefault(n => n.nodeType == NodeType.Monster && n.y >= 5);
            if (target != null) target.nodeType = NodeType.Elite;
        }
        else if (eliteCount > maxElitesPerPath)
        {
            //找一个精英改成普通怪
            var target = path.FirstOrDefault(n => n.nodeType == NodeType.Elite);
            if (target != null) target.nodeType = NodeType.Monster;
        }

        //修正商店数量
        if (shopCount < minShopsPerPath)
        {
            var target = path.FirstOrDefault(n => n.nodeType == NodeType.Monster && n.y > 2);
            if (target != null) target.nodeType = NodeType.Shop;
        }
        else if (shopCount > maxShopsPerPath)
        {
            var target = path.FirstOrDefault(n => n.nodeType == NodeType.Shop);
            if (target != null) target.nodeType = NodeType.Monster;
        }
    }
}