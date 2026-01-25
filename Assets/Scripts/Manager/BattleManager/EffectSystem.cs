using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.RegisterPerformer<PerformEffectGA>(this, PerformEffectPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.UnregisterPerformersByOwner(this);
    }
    private IEnumerator PerformEffectPerformer(PerformEffectGA performEffectGA)
    {
        GameAction effectAction = performEffectGA.Effect.GetGameAction(performEffectGA.Targets);
        ActionSystem.Instance.AddReacyion(effectAction);
        yield return null;
    }
}
