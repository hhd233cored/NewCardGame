using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;

public class SelectCardView : Singleton<SelectCardView>
{
    [SerializeField] private List<Transform> slots;
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject skipButton;
    [SerializeField] private TMP_Text gainGold;
    public List<CardViewUI> cardUIs;
    [SerializeField] private CardViewUI perfab;
    public List<CardData> CardDataList=>GameManager.Instance.CardDataList;
    public bool isSelect;
    private void Start()
    {
        isSelect = false;
        skipButton.SetActive(false);
        background.SetActive(false);
        gainGold.gameObject.SetActive(false);
    }
    public void TestFill(int gain = 0)
    {
        isSelect = !isSelect;
        foreach (var slot in slots)
        {
            slot.gameObject.SetActive(isSelect);
        }
        skipButton.SetActive(isSelect);
        background.SetActive(isSelect);
        gainGold.gameObject.SetActive(false);
        if (isSelect) SetCardToSlots(CardDataList);
        if (gain != 0)
        {
            gainGold.gameObject.SetActive(true);
            gainGold.text = "Victory, You Gain " + gain + " Gold";
        }
    }
    public void SetCardToSlots(List<CardData> cards)
    {
        // 先清理旧的 UI（防止多次点击 TestFill 导致卡牌重叠）
        foreach (var oldUI in cardUIs)
        {
            if (oldUI != null) Destroy(oldUI.gameObject);
        }
        cardUIs.Clear();


        for (int i = 0; i < slots.Count; i++)
        {
            // 防止数据多于格子时报错
            if (i >= slots.Count) break;
            int rand = Random.Range(0, CardDataList.Count);
            CardData cardData = CardDataList[rand];

            Transform slot = slots[i];
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
                rect.localScale = Vector3.one * 0.7f; // 确保缩放是 1
            }

            cardViewUI.Setup(card);
            cardUIs.Add(cardViewUI);
        }
        // 1. 实例化时直接指定父物体，这是最高效且不容易出错的做法
    }
    public void SkipSelect()
    {
        TestFill();
        FinishSelect();
    }
    public void FinishSelect()
    {
        StartCoroutine(GameManager.Instance.EnterMapScene());
    }
}
