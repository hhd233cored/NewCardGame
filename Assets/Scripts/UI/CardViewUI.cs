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
    private Material _dynamicMaterial;

    [Header("Card Data")]
    private Card card;

    public void Setup(Card c)
    {
        card = c;

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
        if (DeckViewUI.Instance.deleteMode) DeletCard();
        if (DeckViewUI.Instance.upgradeMode) UpgradeCard();
        if (SelectCardView.Instance.isSelect) SelectCard();
    }
    public void DeletCard()
    {
        PlayerSystem.Instance.CurrentCards.Remove(card);
        DeckViewUI.Instance.Refresh(PlayerSystem.Instance.CurrentCards);
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
            SelectCardView.Instance.TestFill();
            SelectCardView.Instance.FinishSelect();
        }
    }
    public void GainCard()//获得牌、买牌调用
    {
        PlayerSystem.Instance.CurrentCards.Add(card);
    }
    public Card GetCardData() => card;
}