using System.Collections.Generic;
using ScriptsOfTribute;
using ScriptsOfTribute.Board.Cards;
using UnityEngine;
using UnityEngine.EventSystems;
public class CardController : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler
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
        UIManager.Instance.HideHint(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.ShowHint(this, "[RMB] Tooltip");
    }


    private bool IsInPileZone(Card card)
    {
        return card.IsInZone(ZoneType.PlayedPile, ZoneSide.HumanPlayer) ||
            card.IsInZone(ZoneType.PlayedPile, ZoneSide.EnemyPlayer) ||
            card.IsInZone(ZoneType.CooldownPile, ZoneSide.HumanPlayer) ||
            card.IsInZone(ZoneType.CooldownPile, ZoneSide.EnemyPlayer) ||
            card.IsInZone(ZoneType.DrawPile, ZoneSide.HumanPlayer) ||
            card.IsInZone(ZoneType.DrawPile, ZoneSide.EnemyPlayer);
    }

    private void HandleLeftClick()
    {
        if (!GameManager.Instance.IsHumanPlayersTurn)
            return;

        var cardData = card.GetCard();

        if (card.IsInZone(ZoneType.Hand, ZoneSide.HumanPlayer))
        {
            GameManager.Instance.PlayCard(cardData, ZoneSide.HumanPlayer);
        }
        else if (card.IsInZone(ZoneType.TavernAvailable, ZoneSide.Neutral))
        {
            GameManager.Instance.BuyCard(cardData, ZoneSide.HumanPlayer);
        }
        else if (card.IsInZone(ZoneType.Agents, ZoneSide.HumanPlayer))
        {
            GameManager.Instance.ActivateAgent(cardData, ZoneSide.HumanPlayer);
        }
        else if (card.IsInZone(ZoneType.Agents, ZoneSide.EnemyPlayer))
        {
            GameManager.Instance.AttackAgent(cardData, ZoneSide.HumanPlayer);
        }
    }

    private void HandleRightClick()
    {
        bool isCardInspectable =
            card.IsInZone(ZoneType.Hand, ZoneSide.HumanPlayer) ||
            card.IsInZone(ZoneType.Agents, ZoneSide.HumanPlayer) ||
            card.IsInZone(ZoneType.Agents, ZoneSide.EnemyPlayer) ||
            card.IsInZone(ZoneType.TavernAvailable, ZoneSide.Neutral);

        bool isBotDebugInspectable =
            GameSetupManager.Instance.IsBotDebugMode &&
            card.IsInZone(ZoneType.Hand, ZoneSide.EnemyPlayer);

        if (!isCardInspectable && !isBotDebugInspectable)
            return;

        UIManager.Instance.ShowCardTooltip(card.GetCard(), transform);
    }


}
