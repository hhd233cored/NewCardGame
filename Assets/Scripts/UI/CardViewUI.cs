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

    [Header("Card Data")]
    private Card cardData;

    public void Setup(Card card)
    {
        cardData = card;

        // 更新UI显示
        if (titleText != null) titleText.text = card.Title;
        if (descriptionText != null) descriptionText.text = card.Description;
        if (suitNumText != null) suitNumText.text =
            BattleSystem.SuitToStr(card.Suit) + BattleSystem.NumToStr(card.Num);
        if (cardImage != null && card.Image != null)
            cardImage.sprite = card.Image;
    }
    public void ClickCard()
    {
        if (DeckViewUI.Instance.deleteMode) DeletCard();
        if (DeckViewUI.Instance.upgradeMode) UpgradeCard();
        if (SelectCardView.Instance.isSelect) SelectCard();
    }
    public void DeletCard()
    {
        PlayerSystem.Instance.CurrentCards.Remove(cardData);
        DeckViewUI.Instance.Refresh(PlayerSystem.Instance.CurrentCards);
    }
    public void UpgradeCard()
    {
        Debug.Log("Upgrade " + cardData.Title);
    }
    public void SelectCard()
    {
        if (SelectCardView.Instance.cardUIs.Contains(this))
        {
            PlayerSystem.Instance.CurrentCards.Add(cardData);
            SelectCardView.Instance.cardUIs.Remove(this);
            SelectCardView.Instance.TestFill();
        }
    }
    public Card GetCardData() => cardData;
}