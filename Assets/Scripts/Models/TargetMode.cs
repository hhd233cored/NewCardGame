using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

[System.Serializable]
public abstract class TargetMode
{
    public abstract List<Character> GetTargets();
}

