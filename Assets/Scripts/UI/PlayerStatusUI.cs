using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStatusUI : MonoBehaviour
{
    [SerializeField] private TMP_Text Name;
    [SerializeField] private TMP_Text Health;
    [SerializeField] private TMP_Text Gold;
    private void Update()
    {
        Player player = PlayerSystem.Instance?.player;
        if(player != null)
        {
            Health.text = "HP: " + player.CurrentHealth + "/" + player.MaxHealth;
            Gold.text = "Gold: " + player.CurrentGold;
        }
    }
}
