using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BotFrameDisplay : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Elementy")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI shortDescriptionText;
    [SerializeField] private TextMeshProUGUI longDescriptionText;
    [SerializeField] private Image avatar;
    [SerializeField] private GameObject selectionHighlight;

    public System.Type AIClass { get; private set; }

    private GameSetupPanelController controller;

    public void Setup(BotData data, GameSetupPanelController setupController)
    {
        AIClass = data.AIClass;
        controller = setupController;

        if (nameText != null) nameText.text = data.Name;
        if (shortDescriptionText != null) shortDescriptionText.text = data.ShortDescription;
        if (longDescriptionText != null) longDescriptionText.text = data.LongDescription;
        if (avatar != null)
        {
            avatar.sprite = data.Avatar;
            avatar.SetNativeSize();
        }
    

        Deselect();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
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
}
