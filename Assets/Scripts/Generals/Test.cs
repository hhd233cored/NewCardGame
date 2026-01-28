using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : Singleton<Test>
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private BattleData battleData;
    [SerializeField] private GameObject PlayerView;
    private void Start()
    {
        PlayerSystem.Instance.Setup(playerData);
        PlayerView.SetActive(true);
        /*
        EnemySystem.Instance.Setup(battleData.enemies);
        CardSystem.Instance.Setup(PlayerSystem.Instance.player.CurrentCards);
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
        */
    }
}
