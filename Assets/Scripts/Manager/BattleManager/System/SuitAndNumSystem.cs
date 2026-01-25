using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum SuitStyle { Nul, Diamonds, Clubs, Hearts, Spades }
public class SuitAndNumSystem : Singleton<SuitAndNumSystem>
{
    [SerializeField] private TMP_Text suitNum;
    [SerializeField] private string suit;
    [SerializeField] private int num;
   

    private SuitStyle currentSuit = SuitStyle.Nul;
    private int currentNum = 0;
    private void OnEnable()
    {
        ActionSystem.RegisterPerformer<SetSuitAndNumGA>(this, SetSuitAndNumPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.UnregisterPerformersByOwner(this);
    }
    public bool HasSameSuitOrNum(SuitStyle suit,int num)
    {
        if (currentSuit == SuitStyle.Nul || currentNum == 0 || num == 1) return true;
        return currentSuit == suit || currentNum == num;
    }
    public static string SuitToStr(SuitStyle suit)
    {
        string suitStr;
        switch (suit)
        {
            case SuitStyle.Nul:
                suitStr = "";
                break;
            case SuitStyle.Diamonds:
                suitStr = "♦";
                break;
            case SuitStyle.Clubs:
                suitStr = "♣";
                break;
            case SuitStyle.Hearts:
                suitStr = "♥";
                break;
            case SuitStyle.Spades:
                suitStr = "♠";
                break;
            default:
                suitStr = "";
                break;
        }
        return suitStr;
    }
    public static string NumToStr(int num)
    {
        string numStr;
        switch (num)
        {
            case 0:
                numStr = "";
                break;
            case 1:
                numStr = "A";
                break;
            case 11:
                numStr = "J";
                break;
            case 12:
                numStr = "Q";
                break;
            case 13:
                numStr = "K";
                break;
            default:
                numStr = num.ToString();
                break;
        }
        return numStr;
    }
    private IEnumerator SetSuitAndNumPerformer(SetSuitAndNumGA setGa)
    {
        if (setGa.Num != 1) currentNum = setGa.Num;
        currentSuit = setGa.Suit;
        UpdateSuitText(currentSuit, currentNum);
        yield return null;
    }
    public void UpdateSuitText(SuitStyle suit, int num)
    {
        string suitStr = SuitToStr(suit);
        string numStr = NumToStr(num);
        suitNum.text = suitStr + numStr;
    }
}
