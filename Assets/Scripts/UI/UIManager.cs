using System.Collections;
using System.Collections.Generic;
using ScriptsOfTribute;
using ScriptsOfTribute.Serializers;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
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
    [SerializeField] private PatronSelectionPanel patronSelectionPanel;

    [Header("Text objects")]
    [SerializeField] private GameObject YourTurnImage;

    [Header("Patrons")]
    [SerializeField] private GameObject PatronTooltipPanel;
    [SerializeField] private TextMeshProUGUI PatronTooltipTitle;
    [SerializeField] private TextMeshProUGUI PatronTooltipText;

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

    public void ShowPatronDraft(List<PatronId> patrons)
    {
        patronSelectionPanel.ShowPatronDraft(patrons);
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

    public IEnumerator ShowYourTurnMessage()
    {
        CanvasGroup group = YourTurnImage.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = YourTurnImage.AddComponent<CanvasGroup>();
        }

        YourTurnImage.SetActive(true);
        group.alpha = 0;

        float duration = 0.5f;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);

        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = 1.0f - Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        YourTurnImage.SetActive(false);
    }

    public void ShowPatronTooltip(string name, string text)
    {
        PatronTooltipText.SetText(text);
        PatronTooltipTitle.SetText(name);
        PatronTooltipPanel.SetActive(true);
    }

    public void HidePatronTooltip()
    {
        PatronTooltipPanel.SetActive(false);
    }
}
