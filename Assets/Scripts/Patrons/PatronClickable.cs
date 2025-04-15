using UnityEngine;
using UnityEngine.EventSystems;
using ScriptsOfTribute;

public class PatronClickable : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    private PatronId _patronId;
    private string _tooltipText;
    [SerializeField] private GameObject patronCircle;
    public PlayerEnum favoring;

    private Vector3 _neutralRotate = new Vector3(0f, 0f, 90f);
    private Vector3 _player1FavorRotate = new Vector3(0f, 0f, 180f);
    private Vector3 _player2FavorRotate = new Vector3(0f, 0f, 0f);

    public void Initialize(PatronId id)
    {
        _patronId = id;
        _tooltipText = GetTooltipText(id);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            GameManager.Instance.CallPatron(_patronId, ZoneSide.Player1);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            UIManager.Instance.ShowPatronTooltip(CardUtils.GetFullDeckDisplayName(_patronId), _tooltipText);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HidePatronTooltip();
    }

    public void UpdatePatronCircleRotation()
    {
        switch(favoring)
        {
            case PlayerEnum.NO_PLAYER_SELECTED:
                LeanTween.rotate(patronCircle, _neutralRotate, 0.5f);
                break;
            case PlayerEnum.PLAYER1:
                LeanTween.rotate(patronCircle, _player1FavorRotate, 0.5f);
                break;
            case PlayerEnum.PLAYER2:
                LeanTween.rotate(patronCircle, _player2FavorRotate, 0.5f);
                break;
        }
    }

    private string GetTooltipText(PatronId id)
    {
        return id switch
        {
            PatronId.ANSEI => @"FAVORED
    Hold this patron's favor until the start of your turn: <sprite name=""Coin_icon""> Gain 1 Coin at the start of your turn.

    NEUTRAL
    Pay 2 Power: <sprite name=""Coin_icon""> Gain 1 Coin. This Patron now FAVORS you.

    UNFAVORED
    Pay 2 Power: <sprite name=""Coin_icon""> Gain 1 Coin. This Patron is now NEUTRAL.",

            PatronId.DUKE_OF_CROWS => @"FAVORED
    Unusable: No benefit

    NEUTRAL
    Pay all your Coin: <sprite name=""Power_icon""> Gain Power equal to Coin paid minus 1. This Patron now FAVORS you.

    UNFAVORED
    Pay all your Coin: <sprite name=""Power_icon""> Gain Power equal to Coin paid minus 1. This Patron is now NEUTRAL.",

            PatronId.RAJHIN => @"FAVORED
    Pay 3 Coin: <sprite name=""CreateCards_icon""> Create 1 Bewilderment card and place it in your opponent's cooldown pile.

    NEUTRAL
    Pay 3 Coin: <sprite name=""CreateCards_icon""> Create 1 Bewilderment card and place it in your opponent's cooldown pile. This Patron now FAVORS you.

    UNFAVORED
    Pay 3 Coin: <sprite name=""CreateCards_icon""> Create 1 Bewilderment card and place it in your opponent's cooldown pile. This Patron is now NEUTRAL.",

            PatronId.PSIJIC => @"FAVORED
    Opponent has 1 agent card in their active agents, Pay 4 Coin: <sprite name=""Knockout_icon""> Knock Out—Place 1 of your opponent's active agents into their cooldown pile.

    NEUTRAL
    Opponent has 1 agent card in their active agents, Pay 4 Coin: <sprite name=""Knockout_icon""> Knock Out—Place 1 of your opponent's active agents into their cooldown pile. This Patron now FAVORS you.

    UNFAVORED
    Opponent has 1 agent card in their active agents, Pay 4 Coin: <sprite name=""Knockout_icon""> Knock Out—Place 1 of your opponent's active agents into their cooldown pile. This Patron is now NEUTRAL.",

            PatronId.ORGNUM => @"FAVORED
    Pay 3 Coin: <sprite name=""Power_icon""> Gain 1 Power for every 4 cards you own, rounded down. <sprite name=""CreateCards_icon""> Create 1 Summerset Sacking card and place it in your cooldown pile.

    NEUTRAL
    Pay 2 Coin: <sprite name=""Power_icon""> Gain 1 Power for every 6 cards you own, rounded down. This Patron now FAVORS you.

    UNFAVORED
    Pay 1 Coin: <sprite name=""Power_icon""> Gain 2 Power. This Patron is now NEUTRAL.",

            PatronId.HLAALU => @"FAVORED
    Sacrifice 1 card you own in play that cost 1 or more Coin: <sprite name=""Prestige_icon""> Gain Prestige equal to the card's cost minus 1.

    NEUTRAL
    Sacrifice 1 card you own in play that cost 1 or more Coin: <sprite name=""Prestige_icon""> Gain Prestige equal to the card's cost minus 1. This Patron now FAVORS you.

    UNFAVORED
    Sacrifice 1 card you own in play that cost 1 or more Coin: <sprite name=""Prestige_icon""> Gain Prestige equal to the card's cost minus 1. This Patron is now NEUTRAL.",

            PatronId.PELIN => @"FAVORED
    Pay 2 Power, Have 1 agent card in your cooldown pile: <sprite name=""Refresh_icon""> Refresh—Return up to 1 agent card from your cooldown pile to the top of your play deck.

    NEUTRAL
    Pay 2 Power, Have 1 agent card in your cooldown pile: <sprite name=""Refresh_icon""> Refresh—Return up to 1 agent card from your cooldown pile to the top of your play deck. This Patron now FAVORS you.

    UNFAVORED
    Pay 2 Power, Have 1 agent card in your cooldown pile: <sprite name=""Refresh_icon""> Refresh—Return up to 1 agent card from your cooldown pile to the top of your play deck. This Patron is now NEUTRAL.",

            PatronId.RED_EAGLE => @"FAVORED
    Pay 2 Power: <sprite name=""Draw_icon""> Draw 1 card.

    NEUTRAL
    Pay 2 Power: <sprite name=""Draw_icon""> Draw 1 card. This Patron now FAVORS you.

    UNFAVORED
    Pay 2 Power: <sprite name=""Draw_icon""> Draw 1 card. This Patron is now NEUTRAL.",

            PatronId.TREASURY => @"Pay 2 Coin, Sacrifice 1 card from your hand or your played cards: <sprite name=""CreateCards_icon""> Create 1 Writ of Coin card and place it in your cooldown pile.",

            _ => "Unknown Patron"
        };
    }
}
