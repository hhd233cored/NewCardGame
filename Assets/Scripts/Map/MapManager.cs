using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;

    [Header("UI 绑定")]
    [SerializeField] private GameObject nodePrefab;
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

        //TODO：绘制连线

        yield return null;

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
            Debug.Log("[Map] Scroll position reset to Bottom (0).");
        }
    }
}