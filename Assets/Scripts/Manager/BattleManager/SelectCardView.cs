using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;

public class SelectCardView : Singleton<SelectCardView>
{
    [SerializeField] private List<Transform> slots;
    public List<CardViewUI> cardUIs;
    [SerializeField] private CardViewUI perfab;
    public List<CardData> CardDataList;
    public bool isSelect;
    private void Start()
    {
        isSelect = false;
    }
    public void TestFill()
    {
        isSelect = !isSelect;
        foreach (var slot in slots)
        {
            slot.gameObject.SetActive(isSelect);
        }
        if (isSelect) SetCardToSlots(CardDataList);
    }
    public void SetCardToSlots(List<CardData> cards)
    {
        // 先清理旧的 UI（防止多次点击 TestFill 导致卡牌重叠）
        foreach (var oldUI in cardUIs)
        {
            if (oldUI != null) Destroy(oldUI.gameObject);
        }
        cardUIs.Clear();

        int i = 0;
        foreach (var cardData in cards)
        {
            // 防止数据多于格子时报错
            if (i >= slots.Count) break;

            Transform slot = slots[i++];
            Card card = new(cardData);

            // 1. 实例化时直接指定父物体，这是最高效且不容易出错的做法
            CardViewUI cardViewUI = Instantiate(perfab, slot);

            // 2. 如果一定要在实例化后修改父物体，请使用 SetParent 如下：
            // cardViewUI.transform.SetParent(slot, false); 

            // 3. 既然是 UI，位置通常要重置为 0，确保它在格子的中心
            RectTransform rect = cardViewUI.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one; // 确保缩放是 1
            }

            cardViewUI.Setup(card);
            cardUIs.Add(cardViewUI);
        }
    }
}
