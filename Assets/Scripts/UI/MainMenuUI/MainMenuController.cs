using UnityEngine;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Panele")]
    [SerializeField] private CanvasGroup mainMenuPanel;
    [SerializeField] private CanvasGroup deckBrowserPanel;
    [SerializeField] private CanvasGroup settingsPanel;
    [SerializeField] private CanvasGroup creditsPanel;
    [SerializeField] private CanvasGroup cardsCarouselPanel;

    [Header("Ustawienia animacji")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("External links")]
    [SerializeField] private string discordUrl = "https://discord.gg/";
    [SerializeField] private string githubUrl = "https://github.com/ScriptsOfTribute";
    [SerializeField] private string websiteUrl = "https://cog2025.inesc-id.pt/tales-of-tribute/";
    [SerializeField] private string esoHubUrl = "https://eso-hub.com/en/tales-of-tribute-card-game";

    private void Start()
    {
        ShowOnly(mainMenuPanel);
        StartCoroutine(FadeIn(cardsCarouselPanel));
    }

    public void OnPlayVsBotClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
    }

    public void OnDeckBrowserClicked()
    {
        Application.OpenURL(esoHubUrl);
    }

    public void OnSettingsClicked()
    {
        StartCoroutine(FadeIn(settingsPanel));
    }

    public void OnCreditsClicked()
    {
        StartCoroutine(FadeIn(creditsPanel));
    }

    public void OnExitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OpenDiscord()
    {
        Application.OpenURL(discordUrl);
    }

    public void OpenGithub()
    {
        Application.OpenURL(githubUrl);
    }

    public void OpenWebsite()
    {
        Application.OpenURL(websiteUrl);
    }

    private IEnumerator FadeIn(CanvasGroup cg)
    {
        cg.gameObject.SetActive(true);
        cg.interactable = true;
        cg.blocksRaycasts = true;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    private void ShowOnly(CanvasGroup toShow)
    {
        CanvasGroup[] all = new[] { mainMenuPanel, settingsPanel };
        foreach (var cg in all)
        {
            bool isActive = cg == toShow;
            cg.gameObject.SetActive(isActive);
            cg.alpha = isActive ? 1f : 0f;
            cg.interactable = isActive;
            cg.blocksRaycasts = isActive;
        }
    }
}
