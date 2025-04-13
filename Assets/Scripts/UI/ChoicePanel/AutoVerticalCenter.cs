using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class AutoVerticalCenter : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private bool centerWhenTooSmall = true;

    private RectTransform _wrapper;

    private void Awake()
    {
        _wrapper = GetComponent<RectTransform>();
        if (content == null || viewport == null)
        {
            Debug.LogError("Content lub Viewport nie zostały przypisane!");
        }
        if (scrollRect == null)
            Debug.LogWarning("ScrollRect nie został przypisany! Przeciągnij obiekt ScrollView z komponentem ScrollRect.");
    }

    private void LateUpdate()
    {
        if (content == null || viewport == null || scrollRect == null)
            return;

        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

#if UNITY_EDITOR
        if (!Application.isPlaying && !UnityEditor.SceneView.currentDrawingSceneView) return;
#endif

        if (centerWhenTooSmall && contentHeight < viewportHeight)
        {
            float offset = (viewportHeight - contentHeight) / 2f;
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, -offset);
            scrollRect.vertical = false;
        }
        else
        {
            // content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);
            scrollRect.vertical = true;
        }
    }
}
