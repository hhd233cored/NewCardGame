using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private List<EnemyData> enemyDatas;
    private void Start()
    {
        PlayerSystem.Instance.Setup(playerData);
        EnemySystem.Instance.Setup(enemyDatas);
        CardSystem.Instance.Setup(playerData.Deck);
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
}
