using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class EndTurnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI hoverText;
    [SerializeField] private float fadeDuration = 0.2f;
    private Coroutine currentFade;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!GameManager.Instance.IsHumanPlayersTurn) return;
        if (hoverText != null)
        {
            hoverText.gameObject.SetActive(true);
            StartFade(0f, 1f);
        }
            
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverText != null)
        {
            StartFade(1f, 0f, () =>
            {
                hoverText.gameObject.SetActive(false);
            });
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!GameManager.Instance.IsHumanPlayersTurn) return;
        if (hoverText != null)
        {
            GameManager.Instance.OnEndTurnButtonClicked();
        }
    }

    private void StartFade(float fromAlpha, float toAlpha, System.Action onComplete = null)
    {
        if (currentFade != null)
        {
            StopCoroutine(currentFade);
        }
        currentFade = StartCoroutine(FadeText(fromAlpha, toAlpha, onComplete));
    }

    private System.Collections.IEnumerator FadeText(float from, float to, System.Action onComplete)
    {
        float t = 0f;
        Color color = hoverText.color;
        while (t < fadeDuration)
        {
            float a = Mathf.Lerp(from, to, t / fadeDuration);
            hoverText.color = new Color(color.r, color.g, color.b, a);
            t += Time.deltaTime;
            yield return null;
        }
        hoverText.color = new Color(color.r, color.g, color.b, to);
        onComplete?.Invoke();
    }
}