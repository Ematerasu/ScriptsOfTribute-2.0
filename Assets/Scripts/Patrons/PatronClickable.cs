using UnityEngine;
using UnityEngine.EventSystems;
using ScriptsOfTribute;

public class PatronClickable : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler
{
    private PatronId _patronId;
    private string _tooltipText;
    [SerializeField] private GameObject patronCircle;
    public PlayerEnum favoring;

    private Vector3 _neutralRotate = new Vector3(0f, 0f, 90f);
    private Vector3 _player1FavorRotate = new Vector3(0f, 0f, 179f);
    private Vector3 _player2FavorRotate = new Vector3(0f, 0f, 1f);

    public void Initialize(PatronId id)
    {
        _patronId = id;
        _tooltipText = GetTooltipText(id);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            GameManager.Instance.CallPatron(_patronId, ZoneSide.HumanPlayer);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            UIManager.Instance.ShowPatronTooltip(CardUtils.GetFullDeckDisplayName(_patronId), _tooltipText);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HidePatronTooltip();
        UIManager.Instance.HideHint(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.ShowHint(this, "[RMB] Tooltip");
    }

    public void UpdatePatronCircleRotation()
    {
        PlayerEnum localPlayer = GameManager.Instance.HumanPlayer;
        switch (favoring)
        {
            case PlayerEnum.NO_PLAYER_SELECTED:
                LeanTween.rotate(patronCircle, _neutralRotate, 0.5f);
                break;
            case var player when player == localPlayer:
                LeanTween.rotate(patronCircle, _player1FavorRotate, 0.5f);
                break;
            case var opponent when opponent != PlayerEnum.NO_PLAYER_SELECTED:
                LeanTween.rotate(patronCircle, _player2FavorRotate, 0.5f);
                break;
        }
    }

    private string GetTooltipText(PatronId id)
{
    return id switch
    {
        PatronId.ANSEI => @"FAVORED
<color=#CCCCCC>Hold this patron's favor until the start of your turn:</color> <b><color=#FFFFFF>Gain 1 Coin at the start of your turn.</color></b>

NEUTRAL
<color=#CCCCCC>Pay 2 Power:</color> <b><color=#FFFFFF>Gain 1 Coin. This Patron now FAVORS you.</color></b>

UNFAVORED
<color=#CCCCCC>Pay 2 Power:</color> <b><color=#FFFFFF>Gain 1 Coin. This Patron now FAVORS you.</color></b>",

        PatronId.DUKE_OF_CROWS => @"FAVORED
<color=#CCCCCC>Unusable:</color> <b><color=#FFFFFF>No benefit</color></b>

NEUTRAL
<color=#CCCCCC>Pay all your Coin:</color> <b><color=#FFFFFF>Gain Power equal to Coin paid minus 1. This Patron now FAVORS you.</color></b>

UNFAVORED
<color=#CCCCCC>Pay all your Coin:</color> <b><color=#FFFFFF>Gain Power equal to Coin paid minus 1. This Patron is now NEUTRAL.</color></b>",

        PatronId.RAJHIN => @"FAVORED
<color=#CCCCCC>Pay 3 Coin:</color> <b><color=#FFFFFF>Create 1 Bewilderment card and place it in your opponent's cooldown pile.</color></b>

NEUTRAL
<color=#CCCCCC>Pay 3 Coin:</color> <b><color=#FFFFFF>Create 1 Bewilderment card and place it in your opponent's cooldown pile. This Patron now FAVORS you.</color></b>

UNFAVORED
<color=#CCCCCC>Pay 3 Coin:</color> <b><color=#FFFFFF>Create 1 Bewilderment card and place it in your opponent's cooldown pile. This Patron is now NEUTRAL.</color></b>",

        PatronId.PSIJIC => @"FAVORED
<color=#CCCCCC>Opponent has 1 agent card in their active agents, Pay 4 Coin:</color> <b><color=#FFFFFF>Knock Out—Place 1 of your opponent's active agents into their cooldown pile.</color></b>

NEUTRAL
<color=#CCCCCC>Opponent has 1 agent card in their active agents, Pay 4 Coin:</color> <b><color=#FFFFFF>Knock Out—Place 1 of your opponent's active agents into their cooldown pile. This Patron now FAVORS you.</color></b>

UNFAVORED
<color=#CCCCCC>Opponent has 1 agent card in their active agents, Pay 4 Coin:</color> <b><color=#FFFFFF>Knock Out—Place 1 of your opponent's active agents into their cooldown pile. This Patron is now NEUTRAL.</color></b>",

        PatronId.ORGNUM => @"FAVORED
<color=#CCCCCC>Pay 3 Coin:</color> <b><color=#FFFFFF>Gain 1 Power for every 4 cards you own, rounded down. Create 1 Summerset Sacking card and place it in your cooldown pile.</color></b>

NEUTRAL
<color=#CCCCCC>Pay 3 Coin:</color> <b><color=#FFFFFF>Gain 1 Power for every 6 cards you own, rounded down. This Patron now FAVORS you.</color></b>

UNFAVORED
<color=#CCCCCC>Pay 3 Coin:</color> <b><color=#FFFFFF>Gain 2 Power. This Patron is now NEUTRAL.</color></b>",

        PatronId.HLAALU => @"FAVORED
<color=#CCCCCC>Sacrifice 1 card you own in play that cost 1 or more Coin:</color> <b><color=#FFFFFF>Gain Prestige equal to the card's cost minus 1.</color></b>

NEUTRAL
<color=#CCCCCC>Sacrifice 1 card you own in play that cost 1 or more Coin:</color> <b><color=#FFFFFF>Gain Prestige equal to the card's cost minus 1. This Patron now FAVORS you.</color></b>

UNFAVORED
<color=#CCCCCC>Sacrifice 1 card you own in play that cost 1 or more Coin:</color> <b><color=#FFFFFF>Gain Prestige equal to the card's cost minus 1. This Patron is now NEUTRAL.</color></b>",

        PatronId.PELIN => @"FAVORED
<color=#CCCCCC>Pay 2 Power, Have 1 agent card in your cooldown pile:</color> <b><color=#FFFFFF>Refresh—Return up to 1 agent card from your cooldown pile to the top of your play deck.</color></b>

NEUTRAL
<color=#CCCCCC>Pay 2 Power, Have 1 agent card in your cooldown pile:</color> <b><color=#FFFFFF>Refresh—Return up to 1 agent card from your cooldown pile to the top of your play deck. This Patron now FAVORS you.</color></b>

UNFAVORED
<color=#CCCCCC>Pay 2 Power, Have 1 agent card in your cooldown pile:</color> <b><color=#FFFFFF>Refresh—Return up to 1 agent card from your cooldown pile to the top of your play deck. This Patron is now NEUTRAL.</color></b>",

        PatronId.RED_EAGLE => @"FAVORED
<color=#CCCCCC>Pay 2 Power:</color> <b><color=#FFFFFF>Draw 1 card.</color></b>

NEUTRAL
<color=#CCCCCC>Pay 2 Power:</color> <b><color=#FFFFFF>Draw 1 card. This Patron now FAVORS you.</color></b>

UNFAVORED
<color=#CCCCCC>Pay 2 Power:</color> <b><color=#FFFFFF>Draw 1 card. This Patron is now NEUTRAL.</color></b>",

        PatronId.SAINT_ALESSIA => @"FAVORED
<color=#CCCCCC>Pay 4 Coin:</color> <b><color=#FFFFFF>Create 1 Chainbreaker Sergeant card and place it in your cooldown pile.</color></b>

NEUTRAL
<color=#CCCCCC>Pay 4 Coin:</color> <b><color=#FFFFFF>Create 1 Soldier of the Empire card and place it in your cooldown pile.</color></b>

UNFAVORED
<color=#CCCCCC>Pay 4 Coin:</color> <b><color=#FFFFFF>Gain 2 Power.</color></b>",

        PatronId.TREASURY => @"<color=#CCCCCC>Pay 2 Coin, Sacrifice 1 card from your hand or your played cards:</color> <b><color=#FFFFFF>Create 1 Writ of Coin card and place it in your cooldown pile.</color></b>",

        _ => "Unknown Patron"
    };
}
}
