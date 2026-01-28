using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
public class Character : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TMP_Text BlockText;
    [SerializeField] private GameObject BlockUI;
    [SerializeField] private CharacterBuffDisplay buffDisplay;
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
        if (BlockUI == null) return;
        BlockText.text = CurrentBlock.ToString();
        if (CurrentBlock <= 0) BlockUI.SetActive(false);
        else BlockUI.SetActive(true);
    }
    public void Damage(int damageAmount)
    {
        int loseHp = damageAmount - CurrentBlock;
        CurrentBlock -= damageAmount;
        
        if (CurrentBlock <= 0)
            CurrentBlock = 0;
        UpdateBlockText();
        if (loseHp > 0) CurrentHealth -= loseHp;
        if (CurrentHealth <= 0)
            CurrentHealth = 0;
        transform.DOShakePosition(0.2f, 0.5f);
        UpdateHealthText();
    }

    public void ClearBlock()
    {
        CurrentBlock = 0;
        UpdateBlockText();
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
        Buff existing = BuffList.Find(b => b.GetType() == buff.GetType());
        
        if (existing != null)
        {
            //重复则叠加层数
            existing.AddStacks(buff.stacks);
        }
        else
        {
            //否则添加buff
            Buff instance = System.Activator.CreateInstance(buff.GetType()) as Buff;

            // 传入当前角色(this)和配置数据(buff.data)
            instance.Initialize(this, buff.stacks, buff.data);
            BuffList.Add(instance);
        }

        if (buffDisplay != null) buffDisplay.RefreshIcons();
        if(this is Enemy enemy)
        {
            enemy.UpdateIntentionText();
        }
    }
    public void RemoveBuff(Buff buff)
    {
        if (BuffList.Contains(buff))
        {
            buff.OnRemove(); // 执行取消订阅
            BuffList.Remove(buff);
            if (buffDisplay != null) buffDisplay.RefreshIcons();
        }
    }
}
