using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio")]
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    [Header("Display")]
    public Resolution[] supportedResolutions;
    public FullScreenMode[] supportedModes = new[] {
        FullScreenMode.Windowed,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.ExclusiveFullScreen
    };
    public int resolutionIndex = 0;
    public int screenModeIndex = 0;
    
    [Header("Keybinds")]
    public KeyCode keyAiMove = KeyCode.A;
    public KeyCode keyAiMoveTurn = KeyCode.S;
    public KeyCode keyToggleAutoPlay = KeyCode.T;
    public KeyCode keyEndTurn = KeyCode.Space;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();

        supportedResolutions = Screen.resolutions
            .Where(r =>
                Mathf.Approximately((float)r.width / r.height, 16f / 9f) &&
                r.width >= 1280 && r.height >= 720
            )
            .DistinctBy(r => r.width * 10000 + r.height)
            .OrderByDescending(r => r.width)
            .ToArray();
    }

    public void ApplySettings()
    {
        AudioListener.volume = masterVolume;

        var res = supportedResolutions[resolutionIndex];
        var mode = supportedModes[screenModeIndex];

        Screen.SetResolution(res.width, res.height, mode);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("volume_master", masterVolume);
        PlayerPrefs.SetFloat("volume_music", musicVolume);
        PlayerPrefs.SetFloat("volume_sfx", sfxVolume);
        PlayerPrefs.SetInt("resolution_index", resolutionIndex);
        PlayerPrefs.SetInt("screenmode_index", screenModeIndex);

        PlayerPrefs.SetInt("key_ai_move", (int)keyAiMove);
        PlayerPrefs.SetInt("key_ai_move_turn", (int)keyAiMoveTurn);
        PlayerPrefs.SetInt("key_toggle_autoplay", (int)keyToggleAutoPlay);
        PlayerPrefs.SetInt("key_end_turn", (int)keyEndTurn);

        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("volume_master", 0.5f);
        musicVolume = PlayerPrefs.GetFloat("volume_music", 1f);
        sfxVolume = PlayerPrefs.GetFloat("volume_sfx", 1f);
        resolutionIndex = PlayerPrefs.GetInt("resolution_index", 0);
        screenModeIndex = PlayerPrefs.GetInt("screenmode_index", 0);

        keyAiMove = (KeyCode)PlayerPrefs.GetInt("key_ai_move", (int)KeyCode.A);
        keyAiMoveTurn = (KeyCode)PlayerPrefs.GetInt("key_ai_move_turn", (int)KeyCode.S);
        keyToggleAutoPlay = (KeyCode)PlayerPrefs.GetInt("key_toggle_autoplay", (int)KeyCode.T);
        keyEndTurn = (KeyCode)PlayerPrefs.GetInt("key_end_turn", (int)KeyCode.Space);
    }
}
