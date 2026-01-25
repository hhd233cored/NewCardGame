using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SuitAndNumUI : MonoBehaviour
{
    [SerializeField] private TMP_Text suitNum;
    [SerializeField] private string suit;
    [SerializeField] private int num;
    public void UpdateSuitText(SuitStyle suit,int num)
    {
        string suitStr = SuitAndNumSystem.SuitToStr(suit);
        string numStr = SuitAndNumSystem.NumToStr(num);
        
        suitNum.text = suitStr + numStr;
    }
}
