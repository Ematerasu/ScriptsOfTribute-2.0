using UnityEngine;

public enum ZoneType
{
    Tavern,
    TavernAvailable,
    Hand,
    DrawPile,
    CooldownPile,
    PlayedPile,
    Agents,
}

public enum ZoneSide
{
    Neutral,
    Player1,
    Player2
}

public class CardZone : MonoBehaviour
{
    public ZoneType zoneType;
    public ZoneSide zoneSide;
}