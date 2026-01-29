using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopManager : Singleton<ShopManager>
{
    [SerializeField] private List<Transform> slots;
    public List<CardData> CardDataList;
    public List<CardViewUI> cardUIs;
    public TMP_Text deleteCardUITitle;

    [SerializeField] private CardViewUI perfab;
    [SerializeField] private GameObject shop;

    public bool hasDel;
    public bool isShop;
    private void Start()
    {
        isShop = false;
        shop.SetActive(false);

    }
    public void EnterShop()
    {
        isShop = true;
        shop.SetActive(isShop);
        if (isShop)
        {
            hasDel = false;
            SetCardToSlots(CardDataList);
        }
        deleteCardUITitle.text = "Delete Card";
    }
    public void SetCardToSlots(List<CardData> cards)
    {
        // 先清理旧的 UI（防止多次点击 TestFill 导致卡牌重叠）
        foreach (var oldUI in cardUIs)
        {
            if (oldUI != null) Destroy(oldUI.gameObject);
        }
        cardUIs.Clear();

        int sale = Random.Range(0, slots.Count);

        for (int i = 0; i < slots.Count; i++)
        {
            // 防止数据多于格子时报错
            if (i >= slots.Count) break;
            int rand = Random.Range(0, CardDataList.Count);
            CardData cardData = CardDataList[rand];

            Transform slot = slots[i];
            int cost = Random.Range(45, 56);
            if (sale == i) cost /= 2;
            TMP_Text price = slot?.GetComponentInChildren<TextMeshProUGUI>();
            if (sale == i) price.text = "Sale: " + cost.ToString();
            price.gameObject.SetActive(true);
            Card card = new(cardData);
            /*
            switch ((int)UnityEngine.Random.Range(1, 5))
            {
                case 1:
                    card.Suit = SuitStyle.Diamonds;
                    break;
                case 2:
                    card.Suit = SuitStyle.Clubs;
                    break;
                case 3:
                    card.Suit = SuitStyle.Hearts;
                    break;
                case 4:
                    card.Suit = SuitStyle.Spades;
                    break;
                default:
                    card.Suit = SuitStyle.Nul;
                    break;
            }
            card.Num = UnityEngine.Random.Range(1, 14);
            */
            // 1. 实例化时直接指定父物体，这是最高效且不容易出错的做法
            CardViewUI cardViewUI = Instantiate(perfab, slot);
            
            // 2. 如果一定要在实例化后修改父物体，请使用 SetParent 如下：
            // cardViewUI.transform.SetParent(slot, false); 

            // 3. 既然是 UI，位置通常要重置为 0，确保它在格子的中心
            RectTransform rect = cardViewUI.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
            }

            cardViewUI.Setup(card);
            cardViewUI.cost = cost;
            cardViewUI.priceText = price.gameObject;
            cardUIs.Add(cardViewUI);
        }
    }
    public void HasDeleteCard()
    {
        hasDel = true;
        deleteCardUITitle.text = "Disable";
    }
    public void DeleteCard()
    {
        if (PlayerSystem.Instance.player.CurrentGold > 75 && !hasDel)
        {
            DeckViewUI.Instance.counter = 1;
            DeckViewUI.Instance.deleteMode = true;
            DeckViewUI.Instance.TogglePlayerDeckView();
        }
    }
    public void ResetCardSuitOrNum()
    {

    }
    public void Leave()
    {
        isShop = false;
        foreach (var oldUI in cardUIs)
        {
            if (oldUI != null) Destroy(oldUI.gameObject);
        }
        cardUIs.Clear();
        shop.SetActive(isShop);
        var mapManager = FindObjectOfType<MapManager>();
        mapManager?.UnlockNextLayer();
    }
}
