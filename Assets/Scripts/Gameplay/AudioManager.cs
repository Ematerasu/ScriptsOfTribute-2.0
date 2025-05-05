using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Clips")]
    public AudioClip cardPlay;
    public AudioClip cardDraw;
    public AudioClip cardBuy;
    public AudioClip patronActivate;
    public AudioClip mainMenuMusic;
    public AudioClip gameMusic;
    public AudioClip projectileSound;
    public AudioClip endTurnSound;
    public AudioClip defeatSound;
    public AudioClip victorySound;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        PlayMusic(mainMenuMusic);
        ApplyVolumes();
    }

    public void ApplyVolumes()
    {
        AudioListener.volume = SettingsManager.Instance.masterVolume;
        musicSource.volume = SettingsManager.Instance.musicVolume;
        sfxSource.volume = SettingsManager.Instance.sfxVolume;
    }

    public void PlaySound(AudioClip clip, float volume=1f)
    {
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayCardPlaySfx()
    {
        PlaySound(cardPlay, 0.2f);
    }
    public void PlayCardDrawSfx()
    {
        PlaySound(cardDraw, 0.5f);
    }
    public void PlayCardBuySfx()
    {
        PlaySound(cardBuy, 0.7f);
    }

    public void PlayPatronActivateSfx()
    {
        PlaySound(patronActivate, 0.2f);
    }
    public void PlayYourTurnSfx()
    {
        PlaySound(endTurnSound, 1f);
    }
    
    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic);
    }
    public void PlayGameMusic()
    {
        PlayMusic(gameMusic);
    }
    public void PlayProjectileSound()
    {
        PlaySound(projectileSound, 0.6f);
    }
    
    public void SwapMusic(AudioClip newClip, float fadeDuration = 0.5f)
    {
        StartCoroutine(SwapMusicCoroutine(newClip, fadeDuration));
    }

    private IEnumerator SwapMusicCoroutine(AudioClip newClip, float fadeDuration)
    {
        if (musicSource.clip == newClip) yield break;

        float startVolume = musicSource.volume;

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, SettingsManager.Instance.musicVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = SettingsManager.Instance.musicVolume;
    }
}
