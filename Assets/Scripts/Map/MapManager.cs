using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;

    [Header("UI 绑定")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private RectTransform mapContainer;
    [SerializeField] private ScrollRect scrollRect;      

    [Header("布局参数")]
    [SerializeField] private float xSpacing = 200f;      //左右间距
    [SerializeField] private float ySpacing = 300f;      //上下间距
    [SerializeField] private float paddingBottom = 400f; //底部留白
    [SerializeField] private float paddingTop = 200f;    //顶部留白

    private void Start()
    {
        StartCoroutine(GenerateMapRoutine());
    }

    private IEnumerator GenerateMapRoutine()
    {
        //生成数据
        var mapData = mapGenerator.GenerateMap();
        mapContainer.anchorMin = new Vector2(0, 0);
        mapContainer.anchorMax = new Vector2(1, 0);
        mapContainer.pivot = new Vector2(0.5f, 0);  // Pivot Y=0 底部对齐

        //设置Content高度
        float contentHeight = ((mapData.Count - 1) * ySpacing) + paddingBottom + paddingTop;
        mapContainer.sizeDelta = new Vector2(0, contentHeight);

        //生成节点
        foreach (var layer in mapData)
        {
            foreach (var node in layer)
            {
                GameObject obj = Instantiate(nodePrefab, mapContainer);
                node.uiObject = obj;

                var view = obj.GetComponent<MapNodeView>();
                if (view != null)
                {
                    view.Setup(node);
                    // 仅第0层可交互
                    view.SetInteractable(node.y == 0);
                }

                float layerWidth = (layer.Count - 1) * xSpacing;
                float xPos = node.x * xSpacing - (layerWidth / 2f);
                float yPos = node.y * ySpacing + paddingBottom;

                obj.transform.localPosition = new Vector3(xPos, yPos, 0);
            }
        }

        DrawConnections(mapData);

        yield return null;

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
            Debug.Log("[Map] Scroll position reset to Bottom (0).");
        }
    }

    private void DrawConnections(List<List<MapNode>> mapData)
    {
        //遍历所有层
        for (int y = 0; y < mapData.Count - 1; y++)
        {
            foreach (var node in mapData[y])
            {
                // 画线连接所有孩子
                foreach (var child in node.children)
                {
                    CreateLine(node.uiObject.transform.localPosition, child.uiObject.transform.localPosition);
                }
            }
        }
    }

    private void CreateLine(Vector3 startPos, Vector3 endPos)
    {
        //实例化线段
        GameObject lineObj = Instantiate(linePrefab, mapContainer);


        lineObj.transform.SetAsFirstSibling();
        RectTransform rt = lineObj.GetComponent<RectTransform>();

        //计算
        Vector3 dir = endPos - startPos;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        //设置位置
        rt.localPosition = startPos;
        rt.localRotation = Quaternion.Euler(0, 0, angle);

        //设置长度
        rt.sizeDelta = new Vector2(distance, rt.sizeDelta.y);
    }
}