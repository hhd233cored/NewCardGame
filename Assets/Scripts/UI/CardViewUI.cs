using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardViewUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text suitNumText;
    [SerializeField] private Image cardImage;
    [SerializeField] private Image backgroundImage;

    public int cost;
    public GameObject priceText;

    private Material _dynamicMaterial;
    
    [Header("Card Data")]
    private Card card;

    public void Setup(Card c)
    {
        card = c;
        cost = 0;
        priceText = null;
        // 更新UI显示
        if (titleText != null) titleText.text = card.Title;
        if (descriptionText != null) descriptionText.text = card.Description;
        if (suitNumText != null) suitNumText.text =
            BattleSystem.SuitToStr(card.Suit) + BattleSystem.NumToStr(card.Num);
        if (cardImage != null && card.Image != null)
            cardImage.sprite = card.Image;
        if (_dynamicMaterial == null)
        {
            _dynamicMaterial = Instantiate(backgroundImage.material);
            backgroundImage.material = _dynamicMaterial;
            _dynamicMaterial.SetFloat("_ShowOutline", 0f);
        }
    }
    public void ClickCard()
    {
        if (DeckViewUI.Instance.deleteMode)
        {
            DeletCard();
            return;
        }
        if (DeckViewUI.Instance.upgradeMode)
        {
            UpgradeCard();
            return;
        }
        if (SelectCardView.Instance.isSelect)
        {
            SelectCard();
            return;
        }
        if (ShopManager.Instance.isShop) 
        {
            GainCard(cost);
        } 
    }
    public void DeletCard()
    {
        PlayerSystem.Instance.CurrentCards.Remove(card);
        DeckViewUI.Instance.Refresh(PlayerSystem.Instance.CurrentCards);
        DeckViewUI.Instance.counter--;
        if (DeckViewUI.Instance.counter <= 0)
        {
            if (ShopManager.Instance.isShop) PlayerSystem.Instance.player.ChangeGold(-75);
            DeckViewUI.Instance.deleteMode = false;
            DeckViewUI.Instance.TogglePlayerDeckView();
            ShopManager.Instance.HasDeleteCard();
        }
    }
    public void UpgradeCard()
    {
        Debug.Log("Upgrade " + card.Title);
    }
    public void SelectCard()
    {
        if (SelectCardView.Instance.cardUIs.Contains(this))
        {
            PlayerSystem.Instance.CurrentCards.Add(card);
            SelectCardView.Instance.cardUIs.Remove(this);
            DeckViewUI.Instance.counter--;
            if (DeckViewUI.Instance.counter <= 0)
            {
                SelectCardView.Instance.TestFill();
                SelectCardView.Instance.FinishSelect();
            }
        }
    }
    public void GainCard(int cost = 0)//获得牌、买牌调用
    {
        if (PlayerSystem.Instance.player.CurrentGold > cost)
        {
            PlayerSystem.Instance.CurrentCards.Add(card);
            PlayerSystem.Instance.player.ChangeGold(-cost);
            ShopManager.Instance.cardUIs.Remove(this);
            Destroy(this.gameObject);
            priceText.SetActive(false);
        }
    }
    public Card GetCardData() => card;
}