using UnityEngine;
using UnityEngine.UI;

public class PatronGlowPulsing : MonoBehaviour
{
    private Material runtimeMat;
    private float speed = 1f;
    private Color targetColor;
    private float targetGlow;
    private float startTimeOffset;

    public void Initialize(Color color, float glow, float speed = 1f)
    {
        var img = GetComponentInChildren<Image>();
        runtimeMat = new Material(img.material);
        img.material = runtimeMat;

        runtimeMat.EnableKeyword("OVERLAY_ON");

        runtimeMat.SetColor("_OverlayColor", color);
    }
}
