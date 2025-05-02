using System.Collections.Generic;
using ScriptsOfTribute;
using ScriptsOfTribute.Board.Cards;
using UnityEngine;
using UnityEngine.EventSystems;
public class CardController : MonoBehaviour, IPointerClickHandler
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

    private string GetCardTooltipText(UniqueCard card)
    {
        List<string> parts = new();
        var effects = card.Effects;

        if (effects.Length > 0 && effects[0] != null)
            parts.Add($"<b>Activation:</b> {EffectToString(effects[0])}");

        for (int i = 1; i < effects.Length; i++)
        {
            if (effects[i] != null)
                parts.Add($"<b>Combo {i + 1}:</b> {EffectToString(effects[i])}");
        }

        if (card.CommonId == CardId.MORIHAUS_SACRED_BULL || card.CommonId == CardId.MORIHAUS_THE_ARCHER)
        {
            parts.Add($"<b>Trigger</b> When an agent other than this one is Knocked Out: Gain 1 Coin.");
        }

        return string.Join("\n\n", parts);
    }

    private string EffectToString(UniqueComplexEffect effect)
    {
        if (effect is UniqueEffect ue)
            return ue.ToString();

        if (effect is UniqueEffectOr or)
            return or.ToString();

        if (effect is UniqueEffectComposite comp)
            return comp.ToString();

        return "Unknown effect";
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

        if (UIManager.Instance.IsCardTooltipVisible())
        {
            UIManager.Instance.HideCardTooltip();
            return;
        }

        var uniqueCard = card.GetCard();
        string title = uniqueCard.Name;
        string deck = CardUtils.GetFullDeckDisplayName(uniqueCard.Deck);
        string tooltipText = GetCardTooltipText(uniqueCard);
        Sprite sprite = CardUtils.LoadCardSprite(uniqueCard.Deck, uniqueCard.CommonId);

        UIManager.Instance.ShowCardTooltip(title, deck, tooltipText, sprite);
    }
}
