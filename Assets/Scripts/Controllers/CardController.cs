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
        if (card.IsInZone(ZoneType.PlayedPile, ZoneSide.Player1) ||
            card.IsInZone(ZoneType.PlayedPile, ZoneSide.Player2) ||
            card.IsInZone(ZoneType.CooldownPile, ZoneSide.Player1) ||
            card.IsInZone(ZoneType.CooldownPile, ZoneSide.Player2) ||
            card.IsInZone(ZoneType.DrawPile, ZoneSide.Player1) ||
            card.IsInZone(ZoneType.DrawPile, ZoneSide.Player2))
        {
            PileZoneClickable pileClickable = transform.parent.GetComponent<PileZoneClickable>();
            if (pileClickable != null)
            {
                pileClickable.OnClickedFromCard();
                return;
            }
        }

        if (card.IsAnimating() || !card.IsVisible()) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!GameManager.Instance.IsHumanPlayersTurn) return;
            //Debug.Log($"Click on {card.GetCard().CommonId} {card.GetCard().UniqueId.Value}");
            if (card.IsInZone(ZoneType.Hand, ZoneSide.Player1))
            {   
                GameManager.Instance.PlayCard(card.GetCard(), ZoneSide.Player1);
            }
            else if (card.IsInZone(ZoneType.TavernAvailable, ZoneSide.Neutral))
            { 
                GameManager.Instance.BuyCard(card.GetCard(), ZoneSide.Player1);
            }
            else if (card.IsInZone(ZoneType.Agents, ZoneSide.Player1))
            { 
                GameManager.Instance.ActivateAgent(card.GetCard(), ZoneSide.Player1);
            }
            else if (card.IsInZone(ZoneType.Agents, ZoneSide.Player2))
            {
                GameManager.Instance.AttackAgent(card.GetCard(), ZoneSide.Player1);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right && (
                card.IsInZone(ZoneType.Hand, ZoneSide.Player1) ||
                card.IsInZone(ZoneType.Agents, ZoneSide.Player1) ||
                card.IsInZone(ZoneType.Agents, ZoneSide.Player2) ||
                card.IsInZone(ZoneType.TavernAvailable, ZoneSide.Neutral)
            ))
        {
            if (UIManager.Instance.IsCardTooltipVisible())
            {
                UIManager.Instance.HideCardTooltip();
                return;
            }
            var uniqueCard = card.GetCard();
            string title = $"{uniqueCard.Name}";
            string deck = CardUtils.GetFullDeckDisplayName(uniqueCard.Deck);
            string tooltipText = GetCardTooltipText(uniqueCard);
            Sprite sprite = CardUtils.LoadCardSprite(uniqueCard.Deck, uniqueCard.CommonId);
            UIManager.Instance.ShowCardTooltip(title, deck, tooltipText, sprite);
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
}
