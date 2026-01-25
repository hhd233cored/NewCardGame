using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : Singleton<DamageSystem>
{
    [SerializeField] private GameObject damageVFX;
    private void OnEnable()
    {
        ActionSystem.RegisterPerformer<DealDamageGA>(this, DealDamagePerformer);
    }
    private void OnDisable()
    {
        int removed = ActionSystem.UnregisterPerformersByOwner(this);
    }
    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        foreach(var target in dealDamageGA.Targets)
        {
            target.Damage(dealDamageGA.Amount);
            Instantiate(damageVFX, target.transform.position, Quaternion.identity);
            yield return new WaitForSeconds(0.15f);
            if (target.CurrentHealth <= 0)
            {
                if(target is Enemy enemy)
                {
                    KillEnemyGA killEnemyGA = new(enemy);
                    ActionSystem.Instance.AddReacyion(killEnemyGA);
                }
                else
                {
                    //TODO:Ö´ÐÐËÀÍöÂß¼­
                }
            }
        }
    }
}
