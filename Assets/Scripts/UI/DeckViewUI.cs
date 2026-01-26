using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DeckBrowserUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject browserPanel;
    [SerializeField] private Transform cardGridContainer;
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text cardCountText;

    [Header("布局设置")]
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private int cardsPerRow = 4;

    private List<CardViewUI> cardUIInstances = new List<CardViewUI>();

    void Start()
    {
        // 设置关闭按钮
        if (closeButton != null)
            closeButton.onClick.AddListener(() => browserPanel.SetActive(false));

        // 初始隐藏浏览器
        browserPanel.SetActive(false);
    }

    /// <summary>
    /// 打开卡组浏览器
    /// </summary>
    public void OpenDeckBrowser()
    {
        if (PlayerSystem.Instance == null)
        {
            Debug.LogError("PlayerSystem实例不存在");
            return;
        }

        browserPanel.SetActive(true);
        RefreshDeckDisplay();
    }

    /// <summary>
    /// 刷新卡组显示
    /// </summary>
    private void RefreshDeckDisplay()
    {
        ClearCardInstances();

        // 获取玩家卡组
        var playerCards = PlayerSystem.Instance.CurrentCards;
        if (playerCards == null || playerCards.Count == 0)
        {
            Debug.LogWarning("玩家卡组为空");
            UpdateCardCountText(0);
            return;
        }

        // 生成卡牌UI
        GenerateCardUIs(playerCards);

        // 更新布局和计数
        UpdateGridLayout();
        UpdateCardCountText(playerCards.Count);
    }

    /// <summary>
    /// 生成卡牌UI实例
    /// </summary>
    private void GenerateCardUIs(List<Card> cards)
    {
        if (cardUIPrefab == null || cardGridContainer == null)
        {
            Debug.LogError("卡牌预制体或容器未设置");
            return;
        }

        foreach (var card in cards)
        {
            GameObject cardObj = Instantiate(cardUIPrefab, cardGridContainer);
            CardViewUI cardUI = cardObj.GetComponent<CardViewUI>();

            if (cardUI != null)
            {
                cardUI.Setup(card);
                cardUIInstances.Add(cardUI);
            }
        }
    }

    /// <summary>
    /// 清理所有卡牌UI实例
    /// </summary>
    private void ClearCardInstances()
    {
        foreach (var cardUI in cardUIInstances)
        {
            if (cardUI != null && cardUI.gameObject != null)
                Destroy(cardUI.gameObject);
        }
        cardUIInstances.Clear();
    }

    /// <summary>
    /// 更新网格布局
    /// </summary>
    private void UpdateGridLayout()
    {
        if (gridLayoutGroup == null) return;

        RectTransform containerRect = cardGridContainer as RectTransform;
        if (containerRect != null)
        {
            float containerWidth = containerRect.rect.width;
            float spacing = containerWidth * 0.02f; // 2%间距
            float cellWidth = (containerWidth - spacing * (cardsPerRow - 1)) / cardsPerRow;

            gridLayoutGroup.cellSize = new Vector2(cellWidth, cellWidth * 1.4f);
            gridLayoutGroup.spacing = new Vector2(spacing, spacing);
        }
    }

    /// <summary>
    /// 更新卡牌数量文本
    /// </summary>
    private void UpdateCardCountText(int count)
    {
        if (cardCountText != null)
            cardCountText.text = $"卡牌数量: {count}";
    }

    /// <summary>
    /// 切换浏览器显示/隐藏
    /// </summary>
    public void ToggleBrowser()
    {
        bool shouldShow = !browserPanel.activeSelf;
        browserPanel.SetActive(shouldShow);

        if (shouldShow)
        {
            RefreshDeckDisplay();
        }
    }

    /// <summary>
    /// 强制刷新显示（当卡组变化时调用）
    /// </summary>
    public void ForceRefresh()
    {
        if (browserPanel.activeSelf)
        {
            RefreshDeckDisplay();
        }
    }
}