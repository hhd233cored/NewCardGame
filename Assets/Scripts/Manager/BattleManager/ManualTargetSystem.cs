using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualTargetSystem : Singleton<ManualTargetSystem>
{
    [SerializeField] private ArrowView arrowView;
    [SerializeField] private LayerMask targetLayerMask;

    public void StartTargeting(Vector3 startPosition)
    {
        arrowView.gameObject.SetActive(true);
        arrowView.SetupArrow(startPosition);
    }

    public Enemy EndTargeting(Vector3 mouseWorld)
    {
        arrowView.gameObject.SetActive(false);

        Collider2D col = Physics2D.OverlapPoint(mouseWorld, targetLayerMask);
        if (col != null && col.TryGetComponent(out Enemy enemy))
            return enemy;

        return null;
    }
}