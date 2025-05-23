using UnityEngine;
using TMPro;
using System.Collections;

public class RightClickHintController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI label;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.2f;

    private Coroutine currentRoutine;
    private object currentOwner = null;
    private string currentText = null;

    public void Show(object owner, string text = "[RMB] Tooltip")
    {
        if (owner == currentOwner && text == currentText)
            return;

        currentOwner = owner;
        currentText = text;
        label.text = text;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FadeCanvas(1f, true));
    }

    public void Hide(object owner)
    {
        if (owner != currentOwner)
            return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FadeCanvas(0f, false));
        currentOwner = null;
        currentText = null;
    }

    private IEnumerator FadeCanvas(float targetAlpha, bool enableInteraction)
    {
        canvasGroup.interactable = enableInteraction;
        canvasGroup.blocksRaycasts = enableInteraction;

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
