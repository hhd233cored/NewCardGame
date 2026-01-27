using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BattleSystem : Singleton<BattleSystem>
{
    [SerializeField] private GameObject damageVFX;
    [SerializeField] private TMP_Text suitNum;
    [SerializeField] private string suit;
    [SerializeField] private int num;


    private SuitStyle currentSuit = SuitStyle.Nul;
    private int currentNum = 0;
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
        suitNum.text = suitStr + numStr;
    }
    public bool HasSameSuitOrNum(SuitStyle suit, int num)
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
        if (setGa.Num != 1) currentNum = setGa.Num;
        currentSuit = setGa.Suit;
        UpdateSuitText(currentSuit, currentNum);
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

            // 这里需要调用角色身上的 Buff 管理逻辑
            target.AddBuff(gainBuffGA.Buff);

            Debug.Log($"给 {target.name} 添加了 {gainBuffGA.Buff.GetType().Name}");
        }
        yield return null;
    }
}
