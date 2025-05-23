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

    [Header("Keybindings")]
    [SerializeField] private KeybindButton aiMoveButton;
    [SerializeField] private KeybindButton aiMoveTurnButton;
    [SerializeField] private KeybindButton toggleAutoButton;
    [SerializeField] private KeybindButton endTurnButton;
    private KeyCode keyAiMoveTemp;
    private KeyCode keyAiMoveTurnTemp;
    private KeyCode keyToggleAutoPlayTemp;
    private KeyCode keyEndTurnTemp;

    [Header("Panels")]
    public GameObject settingsPanel;
    [SerializeField] private ScrollRect settingsScrollRect;
    private const string BounceShownKey = "settings_scroll_bounce_shown";

    private float originalMaster;
    private float originalMusic;
    private float originalSFX;

    private void Start()
    {
        masterSlider.value = SettingsManager.Instance.masterVolume;
        musicSlider.value = SettingsManager.Instance.musicVolume;
        sfxSlider.value = SettingsManager.Instance.sfxVolume;
        AudioManager.Instance.ApplyVolumes();
        UpdateResolutionLabel();
        UpdateScreenModeLabel();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            OnExitClicked();
        }
    }

    private void OnEnable()
    {
        originalMaster = SettingsManager.Instance.masterVolume;
        originalMusic = SettingsManager.Instance.musicVolume;
        originalSFX = SettingsManager.Instance.sfxVolume;

        keyAiMoveTemp = SettingsManager.Instance.keyAiMove;
        keyAiMoveTurnTemp = SettingsManager.Instance.keyAiMoveTurn;
        keyToggleAutoPlayTemp = SettingsManager.Instance.keyToggleAutoPlay;
        keyEndTurnTemp = SettingsManager.Instance.keyEndTurn;

        aiMoveButton.SetKey(keyAiMoveTemp);
        aiMoveTurnButton.SetKey(keyAiMoveTurnTemp);
        toggleAutoButton.SetKey(keyToggleAutoPlayTemp);
        endTurnButton.SetKey(keyEndTurnTemp);

        aiMoveButton.OnKeyAssigned = k => keyAiMoveTemp = k;
        aiMoveTurnButton.OnKeyAssigned = k => keyAiMoveTurnTemp = k;
        toggleAutoButton.OnKeyAssigned = k => keyToggleAutoPlayTemp = k;
        endTurnButton.OnKeyAssigned = k => keyEndTurnTemp = k;

        if (!PlayerPrefs.HasKey(BounceShownKey))
        {
            StartCoroutine(HintScrollBounce());
            PlayerPrefs.SetInt(BounceShownKey, 1);
            PlayerPrefs.Save();
        }
    }

    private IEnumerator HintScrollBounce()
    {
        yield return null;
        yield return new WaitForSeconds(0.2f);
        float scrollTime = 0.25f;
        float downTo = 0.85f;

        yield return AnimateScrollTo(downTo, scrollTime);

        yield return new WaitForSeconds(0.3f);

        yield return AnimateScrollTo(1f, scrollTime);
    }

    private IEnumerator AnimateScrollTo(float target, float duration)
    {
        float start = settingsScrollRect.verticalNormalizedPosition;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float value = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            settingsScrollRect.verticalNormalizedPosition = value;
            yield return null;
        }

        settingsScrollRect.verticalNormalizedPosition = target;
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
        var sm = SettingsManager.Instance;

        sm.keyAiMove = keyAiMoveTemp;
        sm.keyAiMoveTurn = keyAiMoveTurnTemp;
        sm.keyToggleAutoPlay = keyToggleAutoPlayTemp;
        sm.keyEndTurn = keyEndTurnTemp;

        sm.ApplySettings();
        sm.SaveSettings();
        StartCoroutine(FadeOut(settingsPanel));
    }

    public void OnExitClicked()
    {
        var sm = SettingsManager.Instance;

        sm.masterVolume = originalMaster;
        sm.musicVolume = originalMusic;
        sm.sfxVolume = originalSFX;

        masterSlider.value = originalMaster;
        musicSlider.value = originalMusic;
        sfxSlider.value = originalSFX;
        AudioManager.Instance.ApplyVolumes();

        keyAiMoveTemp = sm.keyAiMove;
        keyAiMoveTurnTemp = sm.keyAiMoveTurn;
        keyToggleAutoPlayTemp = sm.keyToggleAutoPlay;
        keyEndTurnTemp = sm.keyEndTurn;

        aiMoveButton.SetKey(keyAiMoveTemp);
        aiMoveTurnButton.SetKey(keyAiMoveTurnTemp);
        toggleAutoButton.SetKey(keyToggleAutoPlayTemp);
        endTurnButton.SetKey(keyEndTurnTemp);

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
        settingsScrollRect.verticalNormalizedPosition = 1f;
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

    public bool IsKeyAlreadyUsed(KeyCode key, KeybindButton.KeybindType except)
    {
        return
            (key == keyAiMoveTemp && except != KeybindButton.KeybindType.AiMove) ||
            (key == keyAiMoveTurnTemp && except != KeybindButton.KeybindType.AiMoveTurn) ||
            (key == keyToggleAutoPlayTemp && except != KeybindButton.KeybindType.ToggleAutoPlay) ||
            (key == keyEndTurnTemp && except != KeybindButton.KeybindType.EndTurn);
    }
}
