using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class Character : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public const int MaxBlock = 999;
    public int CurrentBlock { get; private set; }
    protected void SetupBase(int health,Sprite image)
    {
        MaxHealth = health;
        CurrentHealth = health;
        CurrentBlock = 0;
        spriteRenderer.sprite = image;
        UpdateHealthText();
    }
    private void UpdateHealthText()
    {
        healthText.text = "HP:" + CurrentHealth;
    }
    public void Damage(int damageAmount)
    {
        int loseHp = damageAmount - CurrentBlock;
        CurrentBlock -= damageAmount;
        if (loseHp > 0) CurrentHealth -= damageAmount;
        if (CurrentBlock <= 0)
            CurrentBlock = 0;
        if (CurrentHealth <= 0)
            CurrentHealth = 0;
        transform.DOShakePosition(0.2f, 0.5f);
        UpdateHealthText();
    }
    public void GainBlock(int blockAmount)
    {
        CurrentBlock += blockAmount;
        if (CurrentBlock > MaxBlock) CurrentBlock = MaxBlock;
    }
    public void Recover(int recoverAmount)
    {
        CurrentHealth += recoverAmount;
        if (CurrentHealth>MaxHealth) CurrentHealth=MaxHealth;
        UpdateHealthText();
    }
}
