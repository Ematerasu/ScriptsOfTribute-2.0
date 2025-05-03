using System.Collections.Generic;
using ScriptsOfTribute;
using ScriptsOfTribute.Board.Cards;
using UnityEngine;
using UnityEngine.EventSystems;
public class CardController : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{    
    public Card card;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsInPileZone(card))
        {
            var pileClickable = transform.parent.GetComponent<PileZoneClickable>();
            pileClickable?.OnClickedFromCard();
            return;
        }

        if (card.IsAnimating() || !card.IsVisible())
            return;

        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                HandleLeftClick();
                break;

            case PointerEventData.InputButton.Right:
                HandleRightClick();
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HideCardTooltip();
    }


    private bool IsInPileZone(Card card)
    {
        return card.IsInZone(ZoneType.PlayedPile, ZoneSide.Player1) ||
            card.IsInZone(ZoneType.PlayedPile, ZoneSide.Player2) ||
            card.IsInZone(ZoneType.CooldownPile, ZoneSide.Player1) ||
            card.IsInZone(ZoneType.CooldownPile, ZoneSide.Player2) ||
            card.IsInZone(ZoneType.DrawPile, ZoneSide.Player1) ||
            card.IsInZone(ZoneType.DrawPile, ZoneSide.Player2);
    }

    private void HandleLeftClick()
    {
        if (!GameManager.Instance.IsHumanPlayersTurn)
            return;

        var cardData = card.GetCard();

        if (card.IsInZone(ZoneType.Hand, ZoneSide.Player1))
        {
            GameManager.Instance.PlayCard(cardData, ZoneSide.Player1);
        }
        else if (card.IsInZone(ZoneType.TavernAvailable, ZoneSide.Neutral))
        {
            GameManager.Instance.BuyCard(cardData, ZoneSide.Player1);
        }
        else if (card.IsInZone(ZoneType.Agents, ZoneSide.Player1))
        {
            GameManager.Instance.ActivateAgent(cardData, ZoneSide.Player1);
        }
        else if (card.IsInZone(ZoneType.Agents, ZoneSide.Player2))
        {
            GameManager.Instance.AttackAgent(cardData, ZoneSide.Player1);
        }
    }

    private void HandleRightClick()
    {
        bool isCardInspectable =
            card.IsInZone(ZoneType.Hand, ZoneSide.Player1) ||
            card.IsInZone(ZoneType.Agents, ZoneSide.Player1) ||
            card.IsInZone(ZoneType.Agents, ZoneSide.Player2) ||
            card.IsInZone(ZoneType.TavernAvailable, ZoneSide.Neutral);

        bool isBotDebugInspectable =
            GameSetupManager.Instance.IsBotDebugMode &&
            card.IsInZone(ZoneType.Hand, ZoneSide.Player2);

        if (!isCardInspectable && !isBotDebugInspectable)
            return;

        UIManager.Instance.ShowCardTooltip(card.GetCard(), transform);
    }


}
