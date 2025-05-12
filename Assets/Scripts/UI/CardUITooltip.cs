using UnityEngine;
using UnityEngine.EventSystems;
using ScriptsOfTribute.Board.Cards;
using TMPro;
using ScriptsOfTribute;

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
        var nameText = transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        var costText = transform.Find("Cost")?.GetComponent<TextMeshProUGUI>();
        var hpText   = transform.Find("HP")  ?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = card.Name;

        if (costText != null)
        {
            bool notFree = card.Cost > 0;
            costText.gameObject.SetActive(notFree);
            costText.text = card.Cost.ToString();
        }

        if (hpText != null)
        {
            bool hasHp = card.HP > 0;
            hpText.gameObject.SetActive(hasHp);
            if (hasHp) hpText.text = card.HP.ToString();
        }

        // Ugly but have no time to create different prefabs
        if (nameText != null)
        {
            float offsetY = 0f;

            switch (card.Deck)
            {
                case PatronId.TREASURY:
                    offsetY = +8f;
                    break;

                case PatronId.SAINT_ALESSIA:
                    offsetY = +15f; 
                    break;
                case PatronId.ORGNUM:
                    offsetY = +3f; 
                    break;
            }

            if (Mathf.Abs(offsetY) > 0.01f)
            {
                RectTransform rt = nameText.rectTransform;
                rt.anchoredPosition += new Vector2(0f, offsetY);
            }
        }
    }
}
