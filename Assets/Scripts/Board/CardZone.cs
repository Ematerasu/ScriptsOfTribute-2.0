using UnityEngine;

public enum ZoneType
{
    Tavern,
    TavernAvailable,
    RAJHIN,
    SAINT_ALESSIA,
    TREASURY,
    ORGNUM,
    Hand,
    DrawPile,
    CooldownPile,
    PlayedPile,
    Agents,
}

public enum ZoneSide
{
    Neutral,
    HumanPlayer,
    EnemyPlayer
}

public class CardZone : MonoBehaviour
{
    public ZoneType zoneType;
    public ZoneSide zoneSide;
}