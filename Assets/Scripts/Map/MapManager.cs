using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;

    [Header("UI 预制体")]
    [SerializeField] private GameObject nodePrefab; // 你的地图节点UI Prefab (上面挂 MapNodeView)
    [SerializeField] private Transform mapContainer; // ScrollRect 的 Content
    [SerializeField] private float xSpacing = 200f;
    [SerializeField] private float ySpacing = 300f;

    private void Start()
    {
        GenerateAndDrawMap();
    }

    public void GenerateAndDrawMap()
    {
        // 1. 生成数据
        var mapData = mapGenerator.GenerateMap();

        // 2. 绘制节点
        foreach (var layer in mapData)
        {
            foreach (var node in layer)
            {
                GameObject obj = Instantiate(nodePrefab, mapContainer);
                node.uiObject = obj;

                // 设置位置
                // 简单居中算法：让每一层居中显示
                float layerWidth = (layer.Count - 1) * xSpacing;
                float xPos = node.x * xSpacing - (layerWidth / 2f);
                float yPos = node.y * ySpacing;

                obj.transform.localPosition = new Vector3(xPos, yPos, 0);

                //设置显示内容
                var view = obj.GetComponent<MapNodeView>();
                view.Setup(node);

                //简单逻辑：如果是第1层，则设为可交互；其他层设为不可交互
                if (node.y == 0) view.SetInteractable(true);
                else view.SetInteractable(false);
            }
        }

        //绘制连线
        DrawConnections(mapData);
    }

    private void DrawConnections(List<List<MapNode>> mapData)
    {
        foreach (var layer in mapData)
        {
            foreach (var node in layer)
            {
                foreach (var child in node.children)
                {
                    // rawLine(node.uiObject.transform.position, child.uiObject.transform.position);
                }
            }
        }
    }
}