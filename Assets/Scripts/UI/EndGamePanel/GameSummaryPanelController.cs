using TMPro;
using UnityEngine;
using ScriptsOfTribute.Board;
using ScriptsOfTribute;
using UnityEngine.UI;

public class GameSummaryPanelController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI resultText; // "You won!" / "You lost!"
    [SerializeField] private TextMeshProUGUI reasonText;
    [SerializeField] private TextMeshProUGUI prestigeComparisonText;
    [SerializeField] private Image resultIcon;

    [Header("Settings")]
    [SerializeField] private Color victoryColor = new Color(0.1683873f, 0.646306f, 0.8301887f);
    [SerializeField] private Color defeatColor = new Color(0.8313726f, 0.3090782f, 0.1686275f);
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Sprite opponentSprite;

    public void SetSummary(PlayerEnum winner, GameEndReason reason, int prestigePlayer, int prestigeOpponent)
    {
        var humanPlayer = GameManager.Instance.HumanPlayer;
        bool victory = winner == humanPlayer;

        resultText.text = victory ? "You won!" : "You lost!";
        resultText.color = victory ? victoryColor : defeatColor;

        resultIcon.sprite = victory ? playerSprite : opponentSprite;

        reasonText.text = FormatReason(reason);

        prestigeComparisonText.text = $"Your Prestige: {prestigePlayer}\nOpponent Prestige: {prestigeOpponent}";
    }

    private string FormatReason(GameEndReason reason)
    {
        return reason switch
        {
            GameEndReason.INCORRECT_MOVE => "Incorrect move made",
            GameEndReason.PRESTIGE_OVER_40_NOT_MATCHED => "Prestige over 40 not matched",
            GameEndReason.PRESTIGE_OVER_80 => "Reached over 80 Prestige",
            GameEndReason.PATRON_FAVOR => "All Patrons favored",
            GameEndReason.TURN_TIMEOUT => "Turn timeout exceeded",
            GameEndReason.PATRON_SELECTION_TIMEOUT => "Patron selection timeout",
            GameEndReason.PATRON_SELECTION_FAILURE => "Patron selection failure",
            GameEndReason.TURN_LIMIT_EXCEEDED => "Turn limit exceeded",
            GameEndReason.INTERNAL_ERROR => "Internal game error",
            GameEndReason.BOT_EXCEPTION => "Bot crashed",
            GameEndReason.PREPARE_TIME_EXCEEDED => "Prepare phase timeout",
            _ => "Unknown reason"
        };
    }
}
