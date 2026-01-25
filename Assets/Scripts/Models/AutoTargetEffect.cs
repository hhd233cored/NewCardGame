using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using SerializeReferenceEditor;

[System.Serializable]
public class AutoTargetEffect
{
    [field: SerializeReference, SR] public TargetMode TargetMode { get; private set; }
    [field: SerializeReference, SR] public Effect Effect { get; private set; }
}
