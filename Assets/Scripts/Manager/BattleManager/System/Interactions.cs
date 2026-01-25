using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactions : Singleton<Interactions>
{
    public bool PlayerIsDragging { get; set; } = false;
    public bool AllowInteractWhilePerforming { get; set; } = false;

    public bool PlayerCanInteract()
    {
        if (AllowInteractWhilePerforming) return true;
        return !ActionSystem.Instance.IsPerforming;
    }
    public bool PlayerCanHover()
    {
        if (PlayerIsDragging) return false;
        return true;
    }
}
