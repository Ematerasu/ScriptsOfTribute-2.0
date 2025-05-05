using System.Collections;
using System.Collections.Generic;
using ScriptsOfTribute;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("AI Buttons")]
    [SerializeField] private Button aiMoveButton;
    [SerializeField] private Button aiMoveFullTurnButton;

    [Header("Misc buttons")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button homeButton;

    [Header("Choice Panels")]
    [SerializeField] private CardChoicePanelController cardChoicePanel;
    [SerializeField] private EffectChoicePanelController effectChoicePanel;
    [SerializeField] private PatronSelectionPanel patronSelectionPanel;

    [Header("Normal panels")]
    [SerializeField] private CardLookupPanelController cardLookupPanel;
    [SerializeField] private CombosPanelController combosPanel;
    [SerializeField] private BotLogPanelController botLogPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Text objects")]
    [SerializeField] private GameObject YourTurnImage;

    [Header("Patrons")]
    [SerializeField] private GameObject PatronTooltipPanel;
    [SerializeField] private TextMeshProUGUI PatronTooltipTitle;
    [SerializeField] private TextMeshProUGUI PatronTooltipText;

    [Header("End game")]
    [SerializeField] private EndGameController endGameController;

    [Header("Tooltips")]
    [SerializeField] private CardTooltip cardTooltip;
    private Coroutine fadeCoroutine;

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

        settingsButton.onClick.AddListener(ShowSettingsPanel);
        homeButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Mouse1))
        {
            HidePatronTooltip();
        }
    }

    public void ShowSettingsPanel()
    {
        StartCoroutine(FadeIn(settingsPanel, 0.3f));
    }

    public void ShowPatronDraft(List<PatronId> patrons)
    {
        if (!GameSetupManager.Instance.IsBotDebugMode)
            botLogPanel.gameObject.SetActive(false);
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
        AudioManager.Instance.PlayYourTurnSfx();
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

    public void ShowCardTooltip(UniqueCard card, Transform cardWorldTransform)
    {
        if (fadeCoroutine != null)
        StopCoroutine(fadeCoroutine);

        cardTooltip.Setup(card);
        cardTooltip.SetPositionRelativeTo(cardWorldTransform);
        fadeCoroutine = StartCoroutine(FadeTooltip(1f, 0.1f));
    }

    public void HideCardTooltip()
    {
        if (fadeCoroutine != null)
        StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeTooltip(0f, 0.1f));
    }

    public void HandleEndGame(EndGameState endGameState, FullGameState finalState)
    {
        endGameController.Initialize(endGameState, finalState);
    }

    public void CardLookup(ZoneSide side, PileType startingPile)
    {
        cardLookupPanel.Show(side, startingPile);
    }

    public void InitializeCombosPanel(PatronId[] patrons)
    {
        combosPanel.InitializeExpandedView(patrons);
    }

    public void UpdateCombosPanel(ComboStates comboStates)
    {
        combosPanel.UpdateCombos(comboStates);
    }

    private IEnumerator FadeTooltip(float targetAlpha, float duration)
    {
        CanvasGroup cg = cardTooltip.gameObject.GetComponent<CanvasGroup>();
        float startAlpha = cg.alpha;
        float time = 0f;

        if (targetAlpha > 0f)
            cardTooltip.gameObject.SetActive(true);

        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        cg.alpha = targetAlpha;

        if (targetAlpha == 0f)
            cardTooltip.gameObject.SetActive(false);
    }

    private IEnumerator FadeIn(GameObject panel, float fadeDuration)
    {
        var cg = panel.GetComponent<CanvasGroup>();
        cg.gameObject.SetActive(true);
        cg.interactable = true;
        cg.blocksRaycasts = true;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }
}
