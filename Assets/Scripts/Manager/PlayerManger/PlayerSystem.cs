using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSystem : Singleton<PlayerSystem>
{
    [field: SerializeField] public Player player { get; set; }
    public void Setup(PlayerData data)
    {
        player.Setup(data);
    }
}
