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

        targetColor = color;
        targetGlow = glow;
        this.speed = speed;

        startTimeOffset = Time.time;

        runtimeMat.EnableKeyword("HITEFFECT_ON");
        runtimeMat.SetFloat("_HitEffectBlend", 0.35f);

        runtimeMat.SetColor("_HitEffectColor", Color.black);
        runtimeMat.SetFloat("_HitEffectGlow", 0f);
    }

    private void Update()
    {
        if (runtimeMat == null) return;

        float elapsed = Time.time - startTimeOffset;
        float t = Mathf.PingPong(elapsed * speed, 1f);

        Color lerpedColor = Color.Lerp(Color.black, targetColor, t);
        float lerpedGlow = Mathf.Lerp(0f, targetGlow, t);

        runtimeMat.SetColor("_HitEffectColor", lerpedColor);
        runtimeMat.SetFloat("_HitEffectGlow", lerpedGlow);
    }
}
