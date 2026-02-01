using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public List<List<MapNode>> RuntimeMapData;

    public void StartNewGame()
    {
        StartCoroutine(GenerateMapRoutine());
    }

    private IEnumerator GenerateMapRoutine()
    {
        //生成数据
        List<List<MapNode>> mapData;

        if (GameManager.Instance.CurrentMapData != null && GameManager.Instance.CurrentMapData.Count > 0)
        {
            mapData = new List<List<MapNode>>();
            foreach (var layer in GameManager.Instance.CurrentMapData)
            {
                mapData.Add(layer.nodes);
            }

            //重建连线
            RestoreConnections(mapData);
        }
        else
        {
            //生成新图
            mapData = mapGenerator.GenerateMap();
            GameManager.Instance.SaveMapState(mapData);
        }
        RuntimeMapData = mapData;

        mapContainer.anchorMin = new Vector2(0, 0);
        mapContainer.anchorMax = new Vector2(1, 0);
        mapContainer.pivot = new Vector2(0.5f, 0);
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
                    view.SetInteractable(false);
                }

                float layerWidth = (layer.Count - 1) * xSpacing;
                float xPos = node.x * xSpacing - (layerWidth / 2f);
                float yPos = node.y * ySpacing + paddingBottom;

                obj.transform.localPosition = new Vector3(xPos, yPos, 0);
            }
        }

        DrawConnections(mapData);
        UpdateNodeStates();
        yield return null;

        if (scrollRect != null)
        {
            //智能滚动
            if (GameManager.Instance.CurrentNode != null)
            {
                float targetPos = (float)GameManager.Instance.CurrentNode.y / (mapData.Count - 1);
                targetPos += 0.05f;
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(targetPos);

                Debug.Log($"[Map] Scroll to layer {GameManager.Instance.CurrentNode.y} ({targetPos})");
            }
            else
            {
                //刚开始游戏滚到底部
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }

    public void SaveMap()
    {
        if (RuntimeMapData != null && RuntimeMapData.Count > 0)
        {
            GameManager.Instance.SaveMapState(RuntimeMapData);
        }
        else
        {
            Debug.LogWarning("[MapManager]试图保存，但RuntimeMapData为空！");
        }
    }

    ///<summary>
    ///供非战斗节点调用：当交互完成后，刷新地图状态以解锁下一层
    ///</summary>
    public void UnlockNextLayer()
    {
        UpdateNodeStates();
    }

    ///<summary>
    ///刷新全图节点的交互状态
    ///</summary>
    private void UpdateNodeStates()
    {
        var currentNode = GameManager.Instance.CurrentNode;

        //先把所有节点都锁住
        if (GameManager.Instance.CurrentMapData != null)
        {
            foreach (var layer in GameManager.Instance.CurrentMapData)
            {
                foreach (var node in layer.nodes)
                {
                    SetNodeInteractable(node, false);
                }
            }
        }

        //根据进度解锁
        if (currentNode == null)
        {
            if (GameManager.Instance.CurrentMapData != null && GameManager.Instance.CurrentMapData.Count > 0)
            {
                foreach (var node in GameManager.Instance.CurrentMapData[0].nodes)
                {
                    SetNodeInteractable(node, true);
                }
            }
        }

        else
        {
            var realCurrentNode = GetNodeFromData(currentNode.x, currentNode.y);
            if (realCurrentNode != null && realCurrentNode.children != null)
            {
                foreach (var child in realCurrentNode.children)
                {
                    var realChild = GetNodeFromData(child.x, child.y);
                    if (realChild != null)
                    {
                        SetNodeInteractable(realChild, true);
                    }
                }
            }
        }

        ScrollToCurrentLayer();
    }

    ///<summary>
    ///自动滚动ScrollView到当前进度
    ///</summary>
    private void ScrollToCurrentLayer()
    {
        if (scrollRect == null || GameManager.Instance.CurrentMapData == null) return;

        float targetPos = 0f;

        if (GameManager.Instance.CurrentNode != null)
        {
            float currentY = GameManager.Instance.CurrentNode.y;
            float totalLayers = GameManager.Instance.CurrentMapData.Count;
            //相机移动的目标位置，如果需要修改 改CurrentY
            targetPos = (currentY + 1.5f) / (totalLayers - 1);
        }

        targetPos = Mathf.Clamp01(targetPos);
        StartCoroutine(SmoothScroll(targetPos));
    }

    private IEnumerator SmoothScroll(float targetV)
    {
        float startV = scrollRect.verticalNormalizedPosition;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(startV, targetV, elapsed / duration);
            yield return null;
        }
        scrollRect.verticalNormalizedPosition = targetV;
    }


    private MapNode GetNodeFromData(int x, int y)
    {
        var map = GameManager.Instance.CurrentMapData;
        if (map == null || y >= map.Count || y < 0) return null;

        return map[y].nodes.Find(n => n.x == x);
    }

    private void RestoreConnections(List<List<MapNode>> mapData)
    {
        foreach (var layer in mapData)
        {
            foreach (var node in layer)
            {
                //确保运行时列表已初始化
                if (node.children == null) node.children = new List<MapNode>();

                //遍历保存的坐标，找回对象
                foreach (var coord in node.childrenCoordinates)
                {
                    var childNode = GetNodeByCoord(mapData, coord.x, coord.y);
                    if (childNode != null)
                    {
                        // 重新建立引用
                        if (!node.children.Contains(childNode))
                        {
                            node.children.Add(childNode);
                        }
                        // 顺便把反向引用(parents)也连上，虽然UI没用到，但逻辑可能需要
                        if (childNode.parents == null) childNode.parents = new List<MapNode>();
                        if (!childNode.parents.Contains(node))
                        {
                            childNode.parents.Add(node);
                        }
                    }
                }
            }
        }
    }

    private MapNode GetNodeByCoord(List<List<MapNode>> mapData, int x, int y)
    {
        if (y >= 0 && y < mapData.Count)
        {
            return mapData[y].Find(n => n.x == x);
        }
        return null;
    }

    private void SetNodeInteractable(MapNode node, bool interactable)
    {
        if (node.uiObject == null)
        {
            Debug.LogError($"[Map] 试图设置节点 {node.x},{node.y} 但 uiObject 为空！");
            return;
        }

        var view = node.uiObject.GetComponent<MapNodeView>();
        if (view != null)
        {
            view.SetInteractable(interactable);
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