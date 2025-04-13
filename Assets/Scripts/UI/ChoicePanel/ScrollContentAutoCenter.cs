using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollContentAutoCenter : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform viewport;

    private ScrollRect scrollRect;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();

        if (content == null)
            content = scrollRect.content;
        if (viewport == null)
            viewport = scrollRect.viewport;
    }

    private void LateUpdate()
    {
        if (content == null || viewport == null)
            return;

        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        if (contentHeight < viewportHeight)
        {
            float offset = (viewportHeight - contentHeight) / 2f;
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, -offset);
        }
        else
        {
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);
        }
    }
}
