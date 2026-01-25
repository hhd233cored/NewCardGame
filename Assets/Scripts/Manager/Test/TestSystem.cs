using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestSystem : MonoBehaviour
{
    [SerializeField] private List<CardData> deckData;
    private void Start()
    {
        CardSystem.Instance.Setup(deckData);
    }
}
