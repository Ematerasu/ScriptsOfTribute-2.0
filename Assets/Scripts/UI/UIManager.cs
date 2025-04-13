using ScriptsOfTribute;
using ScriptsOfTribute.Serializers;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("AI Buttons")]
    [SerializeField] private Button aiMoveButton;
    [SerializeField] private Button aiMoveFullTurnButton;

    [Header("Choice Panels")]
    [SerializeField] private CardChoicePanelController cardChoicePanel;
    [SerializeField] private EffectChoicePanelController effectChoicePanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        aiMoveButton.gameObject.SetActive(false);
        aiMoveFullTurnButton.gameObject.SetActive(false);

        aiMoveButton.onClick.AddListener(OnAiMoveClicked);
        aiMoveFullTurnButton.onClick.AddListener(OnAiFullTurnClicked);
    }

    public void ShowAiButtons(bool show)
    {
        aiMoveButton.gameObject.SetActive(show);
        aiMoveFullTurnButton.gameObject.SetActive(show);
    }

    private void OnAiMoveClicked()
    {
        if (!AIManager.Instance.BotIsPlaying)
            GameManager.Instance.PlaySingleAIMove();
        else
            Debug.Log("Bot is playing!!");
    }

    private void OnAiFullTurnClicked()
    {
        if (!AIManager.Instance.BotIsPlaying)
            GameManager.Instance.PlayAiTurn();
        else
            Debug.Log("Bot is playing!!");
    }

    public void ShowChoice(SerializedChoice choice)
    {
        switch (choice.Type)
        {
            case Choice.DataType.CARD:
                cardChoicePanel.ShowCardChoice(choice);
                break;

            case Choice.DataType.EFFECT:
                effectChoicePanel.ShowEffectChoice(choice);
                break;

            default:
                Debug.LogError($"Unsupported choice type: {choice.Type}");
                break;
        }
    }
}
