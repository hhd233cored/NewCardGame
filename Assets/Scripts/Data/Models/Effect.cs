using System.Collections.Generic;

[System.Serializable]
public abstract class Effect
{
    public abstract GameAction GetGameAction(List<Character> targets);
}
