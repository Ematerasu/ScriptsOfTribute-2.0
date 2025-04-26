using UnityEngine;
using UnityEngine.UI;
using ScriptsOfTribute;
using ScriptsOfTribute.Board;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class EndGameController : MonoBehaviour
{
    [Header("VictoryDefeat References")]
    [SerializeField] private GameObject victoryDefeatRoot;
    [SerializeField] private Image lineImage;
    [SerializeField] private TextMeshProUGUI victoryDefeatText;

    [Header("EndGamePanel References")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private SeedDisplayController seedDisplayController;
    [SerializeField] private CompletedActionHistoryBuilder completedActionHistoryBuilder;
    [SerializeField] private GameSummaryPanelController gameSummaryPanelController;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float victoryDefeatDuration = 2.0f;
    [SerializeField] private Color victoryColor = new Color(0.1683873f, 0.646306f, 0.8301887f);
    [SerializeField] private Color defeatColor = new Color(0.8313726f, 0.3090782f, 0.1686275f);

    private List<Graphic> victoryGraphics = new();

    public void Initialize(EndGameState endState, FullGameState finalState)
    {
        seedDisplayController.SetSeed(finalState.InitialSeed);
        gameSummaryPanelController.SetSummary(
            endState.Winner,
            endState.Reason, 
            finalState.GetPlayer(GameManager.Instance.HumanPlayer).Prestige,
            finalState.GetPlayer(GameManager.Instance.AIPlayer).Prestige
        );
        mainPanel.SetActive(false);

        CollectVictoryGraphics();

        StartCoroutine(PlayEndGameSequence(endState, finalState));
    }

    private void CollectVictoryGraphics()
    {
        victoryGraphics.Clear();
        victoryGraphics.AddRange(victoryDefeatRoot.GetComponentsInChildren<Image>(true));
        victoryGraphics.AddRange(victoryDefeatRoot.GetComponentsInChildren<TextMeshProUGUI>(true));
    }

    private IEnumerator PlayEndGameSequence(EndGameState endState, FullGameState finalState)
    {
        var humanPlayer = GameManager.Instance.HumanPlayer;
        bool victory = endState.Winner == humanPlayer;

        lineImage.color = victory ? victoryColor : defeatColor;
        victoryDefeatText.text = victory ? "VICTORY" : "DEFEAT";

        SetGraphicsAlpha(0f);

        victoryDefeatRoot.SetActive(true);

        yield return StartCoroutine(FadeGraphics(0f, 1f, fadeDuration));

        yield return new WaitForSecondsRealtime(victoryDefeatDuration);

        yield return StartCoroutine(FadeGraphics(1f, 0f, fadeDuration));

        victoryDefeatRoot.SetActive(false);

        mainPanel.SetActive(true);
        completedActionHistoryBuilder.BuildHistory(finalState.CompletedActions);
    }

    private void SetGraphicsAlpha(float alpha)
    {
        foreach (var graphic in victoryGraphics)
        {
            if (graphic == null) continue;
            var color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
    }

    private IEnumerator FadeGraphics(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        SetGraphicsAlpha(startAlpha);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            SetGraphicsAlpha(alpha);
            yield return null;
        }

        SetGraphicsAlpha(endAlpha);
    }
}

