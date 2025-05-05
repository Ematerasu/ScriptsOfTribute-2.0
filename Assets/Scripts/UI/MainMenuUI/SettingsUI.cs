using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class SettingsUI : MonoBehaviour
{
    [Header("Audio Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Resolution Selector")]
    public TextMeshProUGUI resolutionText;

    [Header("Screen Mode Selector")]
    public TextMeshProUGUI screenModeText;

    [Header("Panels")]
    public GameObject settingsPanel;

    private float originalMaster;
    private float originalMusic;
    private float originalSFX;

    private void Start()
    {
        SettingsManager.Instance.LoadSettings();

        masterSlider.value = SettingsManager.Instance.masterVolume;
        musicSlider.value = SettingsManager.Instance.musicVolume;
        sfxSlider.value = SettingsManager.Instance.sfxVolume;
        AudioManager.Instance.ApplyVolumes();
        UpdateResolutionLabel();
        UpdateScreenModeLabel();
    }

    private void OnEnable()
    {
        originalMaster = SettingsManager.Instance.masterVolume;
        originalMusic = SettingsManager.Instance.musicVolume;
        originalSFX = SettingsManager.Instance.sfxVolume;
    }

    public void OnMasterVolumeChanged(float value)
    {
        SettingsManager.Instance.masterVolume = value;
        AudioManager.Instance.ApplyVolumes();
    }

    public void OnMusicVolumeChanged(float value)
    {
        SettingsManager.Instance.musicVolume = value;
        AudioManager.Instance.ApplyVolumes();
    }

    public void OnSFXVolumeChanged(float value)
    {
        SettingsManager.Instance.sfxVolume = value;
        AudioManager.Instance.ApplyVolumes();
    }

    public void OnResolutionLeft() => ShiftResolution(-1);
    public void OnResolutionRight() => ShiftResolution(+1);

    private void ShiftResolution(int dir)
    {
        var sm = SettingsManager.Instance;
        sm.resolutionIndex = (sm.resolutionIndex + dir + sm.supportedResolutions.Length) % sm.supportedResolutions.Length;
        UpdateResolutionLabel();
    }

    public void OnScreenModeLeft() => ShiftScreenMode(-1);
    public void OnScreenModeRight() => ShiftScreenMode(+1);

    private void ShiftScreenMode(int dir)
    {
        var sm = SettingsManager.Instance;
        sm.screenModeIndex = (sm.screenModeIndex + dir + sm.supportedModes.Length) % sm.supportedModes.Length;
        UpdateScreenModeLabel();
    }

    private void UpdateResolutionLabel()
    {
        var r = SettingsManager.Instance.supportedResolutions[SettingsManager.Instance.resolutionIndex];
        resolutionText.text = $"{r.width}x{r.height}";
    }

    private void UpdateScreenModeLabel()
    {
        var mode = SettingsManager.Instance.supportedModes[SettingsManager.Instance.screenModeIndex];
        screenModeText.text = mode.ToString();
    }

    public void OnApplyClicked()
    {
        SettingsManager.Instance.ApplySettings();
        SettingsManager.Instance.SaveSettings();
        StartCoroutine(FadeOut(settingsPanel));
    }

    public void OnExitClicked()
    {
        SettingsManager.Instance.masterVolume = originalMaster;
        SettingsManager.Instance.musicVolume = originalMusic;
        SettingsManager.Instance.sfxVolume = originalSFX;

        masterSlider.value = originalMaster;
        musicSlider.value = originalMusic;
        sfxSlider.value = originalSFX;
        AudioManager.Instance.ApplyVolumes();

        StartCoroutine(FadeOut(settingsPanel));
    }

    private IEnumerator FadeOut(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;

        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            cg.alpha = 1f - Mathf.Clamp01(t / 0.3f);
            yield return null;
        }

        cg.alpha = 0f;
        cg.gameObject.SetActive(false);
    }
    private IEnumerator FadeIn(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;

        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            cg.alpha = 1f - Mathf.Clamp01(t / 0.3f);
            yield return null;
        }

        cg.alpha = 0f;
        cg.gameObject.SetActive(false);
    }
}
