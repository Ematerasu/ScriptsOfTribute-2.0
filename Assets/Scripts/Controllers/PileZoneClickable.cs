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
        bool IsProhibitedZone = zoneSide == ZoneSide.EnemyPlayer && pileType == PileType.Draw;
        if (IsProhibitedZone && !GameSetupManager.Instance.IsBotDebugMode)
            return;
        UIManager.Instance.CardLookup(zoneSide, pileType);
    }
}

public enum PileType
{
    Draw,
    Played,
    Cooldown
}