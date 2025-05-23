using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Collections;

public class BotFrameDisplay : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Elementy")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI shortDescriptionText;
    [SerializeField] private TextMeshProUGUI longDescriptionText;
    [SerializeField] private Image avatar;
    [SerializeField] private GameObject selectionHighlight;
    [SerializeField] private GameObject ErrorGameObject;
    [SerializeField] private TextMeshProUGUI errorText;

    public System.Type AIClass { get; private set; }

    private GameSetupPanelController controller;
    private Coroutine hideErrorCoroutine;

    private bool isGrpcBot => AIClass == typeof(UnityBots.GrpcBotAI);
    private string grpcHostPath => Path.Combine(Application.streamingAssetsPath, "GrpcHost.exe");

    public void Setup(BotData data, GameSetupPanelController setupController)
    {
        AIClass = data.AIClass;
        controller = setupController;

        if (nameText != null) nameText.text = data.Name;
        if (shortDescriptionText != null) shortDescriptionText.text = data.ShortDescription;
        if (longDescriptionText != null) longDescriptionText.text = data.LongDescription;
        if (avatar != null && data.Avatar != null)
        {
            avatar.sprite = data.Avatar;
            avatar.SetNativeSize();
        }
        else
        {
            avatar.enabled = false;
        }


        Deselect();
        HideError();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isGrpcBot && !File.Exists(grpcHostPath))
        {
            Debug.LogWarning($"Nie znaleziono {grpcHostPath}. Nie można wybrać GrpcBot.");
            ShowError("Missing GrpcHost.exe in StreamingAssets folder.\nThis bot requires an external process to function.");
            return;
        }

        controller.SelectBot(this);
    }

    public void Select()
    {
        if (selectionHighlight != null)
            selectionHighlight.SetActive(true);
    }

    public void Deselect()
    {
        if (selectionHighlight != null)
            selectionHighlight.SetActive(false);
    }
    
    private void ShowError(string message)
    {
        if (hideErrorCoroutine != null)
            StopCoroutine(hideErrorCoroutine);

        if (ErrorGameObject != null)
            ErrorGameObject.SetActive(true);

        if (errorText != null)
            errorText.text = message;

        hideErrorCoroutine = StartCoroutine(HideErrorAfterDelay(4f));
    }

    private void HideError()
    {
        if (ErrorGameObject != null)
            ErrorGameObject.SetActive(false);
        if (errorText != null)
            errorText.text = string.Empty;
    }

    private IEnumerator HideErrorAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        HideError();
    }
}
