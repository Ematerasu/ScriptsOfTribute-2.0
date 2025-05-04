using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScriptsOfTribute.Serializers;
using ScriptsOfTribute.Board.Cards;
using System.Collections.Generic;
using ScriptsOfTribute;
using System.Collections;

public class EffectChoicePanelController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject effectChoicePanel;
    [SerializeField] private TextMeshProUGUI effectChoiceTitle;
    [SerializeField] private Transform effectContentParent;
    [SerializeField] private GameObject effectChoiceButtonPrefab;

    private SerializedChoice currentChoice;
    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;
    private bool spaceHeld = false;

    private void Awake()
    {
        effectChoicePanel.SetActive(false);
        canvasGroup = effectChoicePanel.GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartFade(false);
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            StartFade(true);
        }
    }

    private void StartFade(bool show)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeCoroutine(show));
    }

    private IEnumerator FadeCoroutine(bool show)
    {
        float duration = 0.1f;
        float start = canvasGroup.alpha;
        float end = show ? 1f : 0f;
        float t = 0f;

        canvasGroup.interactable = show;
        canvasGroup.blocksRaycasts = true;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }

        canvasGroup.alpha = end;
    }

    public void ShowEffectChoice(SerializedChoice choice)
    {
        currentChoice = choice;

        effectChoiceTitle.text = ToDisplayString(choice.ChoiceFollowUp);

        foreach (Transform child in effectContentParent)
            Destroy(child.gameObject);

        foreach (var effect in choice.PossibleEffects)
        {
            var btn = Instantiate(effectChoiceButtonPrefab, effectContentParent);
            var controller = btn.GetComponent<EffectChoiceButton>();
            controller.Initialize(effect, OnEffectClicked);
        }

        effectChoicePanel.SetActive(true);
    }

    private void OnEffectClicked(UniqueEffect effect)
    {
        GameManager.Instance.MakeChoice(effect);
        HideEffectChoice();
    }

    public void HideEffectChoice()
    {
        effectChoicePanel.SetActive(false);
    }

    private string ToDisplayString(ChoiceFollowUp followUp)
    {
        return followUp switch
        {
            ChoiceFollowUp.ENACT_CHOSEN_EFFECT => "Choose one effect",
            _ => "Make a choice"
        };
    }
}