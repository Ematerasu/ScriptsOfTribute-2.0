using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeedDisplayController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI seedText;
    [SerializeField] private Button copyButton;

    private ulong currentSeed;

    private void Awake()
    {
        if (copyButton != null)
        {
            copyButton.onClick.AddListener(OnCopyButtonClicked);
        }
    }

    public void SetSeed(ulong seed)
    {
        currentSeed = seed;
        if (seedText != null)
        {
            seedText.text = seed.ToString();
        }
    }

    private void OnCopyButtonClicked()
    {
        GUIUtility.systemCopyBuffer = currentSeed.ToString();
        Debug.Log($"Seed {currentSeed} skopiowany do schowka.");
    }
}
