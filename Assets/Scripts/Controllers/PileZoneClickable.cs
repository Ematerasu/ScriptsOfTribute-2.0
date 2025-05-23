using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PileZoneClickable : MonoBehaviour
{
    [SerializeField] private ZoneSide zoneSide;
    [SerializeField] private PileType pileType;

    public void OnClickedFromCard()
    {
        UIManager.Instance.CardLookup(zoneSide, pileType);
    }
}

public enum PileType
{
    Draw,
    Played,
    Cooldown,
    Hand,
}