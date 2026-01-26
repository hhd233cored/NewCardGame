using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class Character : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TMP_Text BlockText;
    [SerializeField] private GameObject BlockUI;
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public const int MaxBlock = 999;
    public int CurrentBlock { get; private set; }
    public List<Buff> BuffList = new();
    protected void SetupBase(int health,Sprite image)
    {
        MaxHealth = health;
        CurrentHealth = health;
        CurrentBlock = 0;
        spriteRenderer.sprite = image;
        UpdateHealthText();
        UpdateBlockText();
    }
    private void UpdateHealthText()
    {
        healthText.text = "HP:" + CurrentHealth;
    }
    private void UpdateBlockText()
    {
        BlockText.text = CurrentBlock.ToString();
        if (CurrentBlock <= 0) BlockUI.SetActive(false);
        else BlockUI.SetActive(true);
    }
    public void Damage(int damageAmount)
    {
        int loseHp = damageAmount - CurrentBlock;
        CurrentBlock -= damageAmount;
        if (loseHp > 0) CurrentHealth -= loseHp;
        if (CurrentBlock <= 0)
            CurrentBlock = 0;
        if (CurrentHealth <= 0)
            CurrentHealth = 0;
        transform.DOShakePosition(0.2f, 0.5f);
        UpdateBlockText();
        UpdateHealthText();
    }
    public void GainBlock(int blockAmount)
    {
        CurrentBlock += blockAmount;
        if (CurrentBlock > MaxBlock) CurrentBlock = MaxBlock;
        UpdateBlockText();
    }
    public void Recover(int recoverAmount)
    {
        CurrentHealth += recoverAmount;
        if (CurrentHealth>MaxHealth) CurrentHealth=MaxHealth;
        UpdateHealthText();
    }
    public void AddBuff(Buff buff)
    {
        // 1. 检查是否存在同类 Buff
        Buff existing = BuffList.Find(b => b.GetType() == buff.GetType());

        if (existing != null)
        {
            // 2. 叠加层数
            existing.AddStacks(1);
        }
        else
        {
            Buff instance = System.Activator.CreateInstance(buff.GetType()) as Buff;
            //instance.Initialize(this, 1, buff.GetData()); // 建议在 Buff 类里加个 GetData()
            BuffList.Add(instance);
        }
    }
    public void RemoveBuff(Buff buff)
    {
        if (BuffList.Contains(buff))
        {
            buff.OnRemove(); // 执行取消订阅
            BuffList.Remove(buff);
        }
    }
}
