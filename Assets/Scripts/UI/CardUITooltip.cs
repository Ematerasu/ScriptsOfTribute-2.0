using UnityEngine;
using UnityEngine.EventSystems;
using ScriptsOfTribute.Board.Cards;

public class CardUITooltip : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    private UniqueCard cardData;
    [SerializeField] private RectTransform thisRect;

    private void Awake()
    {
        if (thisRect == null)
            thisRect = GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && cardData != null)
        {
            UIManager.Instance.ShowCardTooltip(cardData, thisRect);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HideCardTooltip();
    }

    public void SetCardData(UniqueCard card)
    {
        cardData = card;
    }
}
