using UnityEngine;
using UnityEngine.UI;

public class PatronGlowPulsing : MonoBehaviour
{
    private Material runtimeMat;
    public void Initialize(Color color)
    {
        var img = GetComponentInChildren<Image>();
        if (img == null)
        {
            Debug.LogWarning("No Image component found in children.");
            return;
        }
        runtimeMat = new Material(img.material);
        img.material = runtimeMat;

        runtimeMat.SetColor("_OverlayColor", color);
        runtimeMat.SetFloat("_OverlayBlend", 0.714f);
    }
}
