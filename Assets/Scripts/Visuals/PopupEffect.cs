using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private CanvasGroup canvasGroup;

    public void Setup(string text)
    {
        popupText.text = text;
        canvasGroup.alpha = 0f;
    }

    public IEnumerator PlayQueuedAnimation(float lifetime)
    {
        float fadeInTime = 0.2f;
        float fadeOutTime = 0.5f;
        float stayTime = lifetime - fadeInTime - fadeOutTime;

        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInTime);
            yield return null;
        }

        yield return new WaitForSeconds(stayTime);

        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutTime);
            yield return null;
        }

        Destroy(gameObject);
    }
}
