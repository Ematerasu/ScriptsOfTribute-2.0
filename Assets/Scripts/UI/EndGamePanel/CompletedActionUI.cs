using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute;

public class CompletedActionUI : MonoBehaviour
{
    [SerializeField] private Image playerIcon;
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI subText;

    public void Initialize(CompletedAction action, Sprite player1Sprite, Sprite player2Sprite, string main, string sub)
    {
        playerIcon.sprite = action.Player == PlayerEnum.PLAYER1 ? player1Sprite : player2Sprite;
        mainText.text = main;
        subText.text = sub;
    }
}
