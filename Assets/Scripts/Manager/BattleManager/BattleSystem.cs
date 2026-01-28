using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.GraphicsBuffer;

public class BattleSystem : Singleton<BattleSystem>
{
    [SerializeField] private GameObject damageVFX;
    [SerializeField] private TMP_Text suitNum;
    [SerializeField] private string suit;
    [SerializeField] private int num;

    [SerializeField] private HandView handView;


    private SuitStyle currentSuit = SuitStyle.Nul;
    private int currentNum = 0;
    private int currentDirection;
    private int currentScore = 1;

    public int direction => currentDirection;
    private void OnEnable()
    {
        ActionSystem.RegisterPerformer<DealDamageGA>(this, DealDamagePerformer);
        ActionSystem.RegisterPerformer<PerformEffectGA>(this, PerformEffectPerformer);
        ActionSystem.RegisterPerformer<SetSuitAndNumGA>(this, SetSuitAndNumPerformer);
        ActionSystem.RegisterPerformer<GainBlockGA>(this, GainBlockPerformer);//该动作的使用对象①对于敌怪将会在执行意图时设置②对于玩家则是在Effect上实现
        ActionSystem.RegisterPerformer<RecoverGA>(this, RecoverPerformer);
        ActionSystem.RegisterPerformer<GainBuffGA>(this, GainBuffPerformer);
    }
    private void OnDisable()
    {
        int removed = ActionSystem.UnregisterPerformersByOwner(this);
    }
    public void UpdateSuitText(SuitStyle suit, int num)
    {
        string suitStr = SuitToStr(suit);
        string numStr = NumToStr(num);
        //新增方向
        string dir="";
        switch (currentDirection)
        {
            case -1:
                dir = "↓";
                break;
            case 1:
                dir = "↑";
                break;
            default:
                dir = "";
                break;
        }
        //end
        suitNum.text = suitStr + numStr + dir;
    }
    public void ResetDir()
    {
        currentDirection = 0;
    }
    public bool HasSameSuitOrNum(SuitStyle suit, int num)
    {
        if (currentSuit == SuitStyle.Nul || currentNum == 0 || num == 1) return true;
        return currentSuit == suit || currentNum == num;
    }

    public bool StrictCheckSuitOrNum(SuitStyle suit, int num)
    {
        if (currentSuit == SuitStyle.Nul || currentNum == 0) return true;

        if(currentSuit == suit)
        {
            if ((num - currentNum) * direction >= 0)//检测惯性是否与方向相同，比如（5-4）*1>=0，(4-6)*-1.=0则代表相同
            {
                return Mathf.Abs(num - currentNum) <= 1;//检查是否同点数或者相邻
            }
        }
        return currentNum == num;
    }

    public bool StrictCheckSuitOrNum2(SuitStyle suit, int num,SuitStyle hoverCardSuit,int hoverCardNum)
    {
        if (hoverCardSuit == SuitStyle.Nul || hoverCardNum == 0) return true;

        int dir = 0;

        if (hoverCardSuit == currentSuit)
        {
            if (num - hoverCardNum < 0) dir = -1;//下降
            if (num - hoverCardNum > 0) dir = 1;//上升
            if (hoverCardNum == 13) dir = -1;
            if (hoverCardNum == 1) dir = 1;
            if (hoverCardNum == currentNum) dir = 0;
        }
        else
        {
            if (hoverCardNum == currentNum) dir = 0;//断连照样重置方向？
        }

        if (hoverCardSuit == suit)
        {
            if ((num - hoverCardNum) * dir >= 0)//检测惯性是否与方向相同，比如（5-4）*1>=0，(4-6)*-1.=0则代表相同
            {
                return Mathf.Abs(num - hoverCardNum) <= 1;//检查是否同点数或者相邻
            }
        }
        return hoverCardNum == num;
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


    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        foreach(var target in dealDamageGA.Targets)
        {
            target.Damage(MainController.Instance.TotalDamage(dealDamageGA.Amount, dealDamageGA.Targets, dealDamageGA.Source));
            Instantiate(damageVFX, target.transform.position, Quaternion.identity,this.transform);
            yield return new WaitForSeconds(0.15f);
            if (target.CurrentHealth <= 0)
            {
                if(target is Enemy enemy)
                {
                    KillEnemyGA killEnemyGA = new(enemy);
                    ActionSystem.Instance.AddReaction(killEnemyGA);
                }
                else
                {
                    //TODO:执行死亡逻辑
                }
            }
        }
    }
    private IEnumerator PerformEffectPerformer(PerformEffectGA performEffectGA)
    {
        GameAction effectAction = performEffectGA.Effect.GetGameAction(performEffectGA.Targets,performEffectGA.Source);
        ActionSystem.Instance.AddReaction(effectAction);
        yield return null;
    }
   
    private IEnumerator SetSuitAndNumPerformer(SetSuitAndNumGA setGa)
    {
        
        
        
        if (setGa.Suit == currentSuit)
        {
            //新增设置方向
            if (num - setGa.Num < 0) currentDirection = -1;//下降
            if (num - setGa.Num > 0) currentDirection = 1;//上升
            if (setGa.Num == 13) currentDirection = -1;
            if (setGa.Num == 1) currentDirection = 1;
            if (setGa.Num == currentNum) currentDirection = 0;

            //新增设置分数
            //同花色加分
            if (currentScore < 4) currentScore++;
        }
        else
        {
            if (setGa.Num == currentNum) currentDirection = 0;//断连照样重置方向？
            currentScore = 1;
        }
        //

        //新增：A不再是万能牌
        //if (setGa.Num != 1) currentNum = setGa.Num;

        currentNum = setGa.Num;
        currentSuit = setGa.Suit;
        UpdateSuitText(currentSuit, currentNum);
        //handView.ResetOutLine();
        yield return null;
    }
   
    private IEnumerator GainBlockPerformer(GainBlockGA gainBlockGA)
    {
        foreach(var target in gainBlockGA.Target)
        {
            target.GainBlock(gainBlockGA.Amount);
        }
        yield return null;
    }

    private IEnumerator RecoverPerformer(RecoverGA recoverGA)
    {
        foreach (var target in recoverGA.Target)
        {
            target.Recover(recoverGA.Amount);
        }
        yield return null;
    }
    private IEnumerator GainBuffPerformer(GainBuffGA gainBuffGA)
    {
        foreach (var target in gainBuffGA.Targets)
        {
            if (target == null) continue;

            target.AddBuff(gainBuffGA.Buff);
        }
        yield return null;
    }
}
