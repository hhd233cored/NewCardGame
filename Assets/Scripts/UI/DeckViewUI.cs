using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class DeckViewUI : Singleton<DeckViewUI>
{
    [Header("UI References")]
    [SerializeField] private RectTransform contentFolder;
    [SerializeField] private CardViewUI cardPrefab;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Layout Settings")]
    [SerializeField] private int cardsPerRow = 5;
    [SerializeField] private Vector2 cellSize = new Vector2(200, 300);
    [SerializeField] private Vector2 spacing = new Vector2(40, 50);
    [SerializeField] private float paddingTop = 50f;
    [SerializeField] private float paddingBottom = 100f;
    [SerializeField] private float cardScale = 0.4f;

    public bool active;
    public bool deleteMode;//为true时点击卡牌会删牌
    public bool upgradeMode;//为true时点击卡牌会升级卡牌

    public int counter;//计数器用于牌点击计数，比如删两张牌

    private void Start()
    {
        active = false;
        deleteMode = false;
        upgradeMode = false;
        counter = 0;
    }
    public void TogglePlayerDeckView()
    {
        active = !active;
        scrollRect.gameObject.SetActive(active);
        if (active) RefreshDisplay(PlayerSystem.Instance.CurrentCards);
    }
    public void ToggleDeckView(List<Card> cards)
    {
        active = !active;
        scrollRect.gameObject.SetActive(active);
        if (active) RefreshDisplay(cards);
    }
    public void Refresh(List<Card> c)
    {
        RefreshDisplay(c);
    }

    private void RefreshDisplay(List<Card> c)
    {
        // 1. 清理
        foreach (Transform child in contentFolder)
        {
            Destroy(child.gameObject);
        }

        // 2. 数据获取与排序
        var cards = c;

        // 3. 配置容器
        // 设置容器锚点在顶部中心，Pivot 也在顶部中心
        // 锚点设为：水平居中(0.5)，垂直顶部(1)
        contentFolder.anchorMin = new Vector2(0.5f, 1);
        contentFolder.anchorMax = new Vector2(0.5f, 1);
        // 轴心点设为：水平居中(0.5)，垂直顶部(1)
        contentFolder.pivot = new Vector2(0.5f, 1);

        // 将位置强制归零（确保它贴在 ScrollView 的顶部）
        contentFolder.anchoredPosition = Vector2.zero;


        int totalRows = Mathf.CeilToInt((float)cards.Count / cardsPerRow);

        float calculatedHeight = paddingTop + (totalRows * cellSize.y) + (Mathf.Max(0, totalRows - 1) * spacing.y) + paddingBottom;

        contentFolder.sizeDelta = new Vector2(contentFolder.sizeDelta.x, calculatedHeight);

        // 4. 计算起始 X 坐标（让卡牌居中排列）
        float rowWidth = (cardsPerRow * cellSize.x) + ((cardsPerRow - 1) * spacing.x);
        float startX = -rowWidth / 2f + cellSize.x / 2f;

        // 5. 生成卡牌
        for (int i = 0; i < cards.Count; i++)
        {
            int row = i / cardsPerRow;
            int col = i % cardsPerRow;

            CardViewUI cardUI = Instantiate(cardPrefab, contentFolder);
            cardUI.Setup(cards[i]);

            RectTransform rect = cardUI.GetComponent<RectTransform>();
            if (rect != null)
            {
                // 计算当前容器的半高度（这是从中心到顶边的距离）
                float halfContentHeight = contentFolder.sizeDelta.y / 2f;

                // 计算横向位置
                float xPos = startX + col * (cellSize.x + spacing.x);

                // 【关键修改】：
                // 1. halfContentHeight 将坐标原点“推”到了容器顶边
                // 2. 然后再减去 paddingTop 和 行间距 往下排
                float yPos = halfContentHeight - paddingTop - (row * (cellSize.y + spacing.y)) - (cellSize.y / 2f);

                rect.anchoredPosition = new Vector2(xPos, yPos);
                rect.localScale = Vector3.one * cardScale;

                // 习惯性清空 Z 轴
                rect.anchoredPosition3D = new Vector3(xPos, yPos, 0);
            }
        }

        // 6. 重置滚动条
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    public void Close() => gameObject.SetActive(false);
}