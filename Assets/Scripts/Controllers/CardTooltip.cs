using System.Collections.Generic;
using ScriptsOfTribute;
using ScriptsOfTribute.Board.Cards;
using TMPro;
using UnityEngine;

public class CardTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI deckText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI effectText;

    public void Setup(UniqueCard card)
    {
        cardNameText.text = card.Name;
        deckText.text = CardUtils.GetDeckDisplayName(card.Deck);
        typeText.text = CardUtils.GetTypeString(card.Type);
        effectText.text = BuildEffectText(card);
    }
    private string BuildEffectText(UniqueCard card)
    {
        List<string> parts = new();
        var effects = card.Effects;

        if (effects.Length > 0 && effects[0] != null)
            parts.Add($"<b>PLAY EFFECT</b>\n{EffectToString(effects[0])}");

        for (int i = 1; i < effects.Length; i++)
        {
            if (effects[i] != null)
                parts.Add($"<b>COMBO {i + 1}</b>\n{EffectToString(effects[i])}");
        }

        if (card.CommonId == CardId.MORIHAUS_SACRED_BULL || card.CommonId == CardId.MORIHAUS_THE_ARCHER)
        {
            parts.Add($"<b>TRIGGER</b>\nWhen an agent other than this one is Knocked Out: Gain 1 Coin.");
        }

        return string.Join("\n\n", parts);
    }

    private string EffectToString(UniqueComplexEffect effect)
    {
        if (effect is UniqueEffect ue)
        {
            return EffectDescription(ue.Type, ue.Amount);
        }

        if (effect is UniqueEffectOr or)
        {
            return $"{EffectToString(or.GetLeft())} <b>OR</b> {EffectToString(or.GetRight())}";
        }

        if (effect is UniqueEffectComposite comp)
        {
            return $"{EffectToString(comp.GetLeft())} <b>AND</b> {EffectToString(comp.GetRight())}";
        }

        return "Unknown effect";
    }

    public void SetPositionRelativeTo(Transform cardTransform)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(cardTransform.position);
        bool showRight = screenPos.x < Screen.width / 2f;
        float offsetX = showRight ? 200f : -200f;
        Vector2 adjustedScreenPos = new Vector2(screenPos.x + offsetX, screenPos.y);

        RectTransform canvasRect = transform.parent.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            adjustedScreenPos,
            Camera.main,
            out localPoint
        );

        RectTransform rt = GetComponent<RectTransform>();
        rt.pivot = new Vector2(showRight ? 0f : 1f, 0.5f);
        localPoint = AdjustYToStayInCanvas(localPoint, rt, canvasRect);
        rt.anchoredPosition = localPoint;
    }

    private string EffectDescription(EffectType type, int value)
    {
        string baseColor = "#89857C";
        string valueStr = $"<color=#FFFFFF><b>{value}</b></color>";
        string Sized(string text) => $"<size=90%>{text}</size>";
        switch (type)
        {
            case EffectType.GAIN_COIN:
                return Sized($"<color={baseColor}>Gain {valueStr} coin(s)</color>");
            case EffectType.GAIN_POWER:
                return Sized($"<color={baseColor}>Gain {valueStr} power</color>");
            case EffectType.GAIN_PRESTIGE:
                return Sized($"<color={baseColor}>Gain {valueStr} prestige</color>");
            case EffectType.OPP_LOSE_PRESTIGE:
                return Sized($"<color={baseColor}>Opponent loses {valueStr} prestige</color>");
            case EffectType.REPLACE_TAVERN:
                return Sized($"<color={baseColor}>Replace up to {valueStr} Tavern card(s)</color>");
            case EffectType.ACQUIRE_TAVERN:
                return Sized($"<color={baseColor}>Acquire a Tavern card costing up to {valueStr}</color>");
            case EffectType.DESTROY_CARD:
                return Sized($"<color={baseColor}>Destroy a card from hand or played pile</color>");
            case EffectType.DRAW:
                return Sized($"<color={baseColor}>Draw {valueStr} card(s)</color>");
            case EffectType.OPP_DISCARD:
                return Sized($"<color={baseColor}>Opponent discards {valueStr} card(s)</color>");
            case EffectType.RETURN_TOP:
                return Sized($"<color={baseColor}>Put a card from your cooldown on top of your draw pile</color>");
            case EffectType.RETURN_AGENT_TOP:
                return Sized($"<color={baseColor}>Put an agent from your cooldown on top of your draw pile</color>");
            case EffectType.TOSS:
                return Sized($"<color={baseColor}>Toss {valueStr} card(s)</color>");
            case EffectType.KNOCKOUT:
                return Sized($"<color={baseColor}>Knock out {valueStr} enemy agent(s)</color>");
            case EffectType.PATRON_CALL:
                return Sized($"<color={baseColor}>Gain {valueStr} more patron call(s)</color>");
            case EffectType.CREATE_SUMMERSET_SACKING:
                return Sized($"<color={baseColor}>Create a <i>Summerset Sacking</i> card</color>");
            case EffectType.HEAL:
                return Sized($"<color={baseColor}>Heal this agent for {valueStr} health</color>");
            case EffectType.KNOCKOUT_ALL:
                return Sized($"<color={baseColor}>Knock out all agents on board</color>");
            case EffectType.DONATE:
                return Sized($"<color={baseColor}>Swap {valueStr} of your cards in hand</color>");
            default:
                return Sized($"<color={baseColor}>[Unknown effect: {type}, value={value}]</color>");
        }
    }

    private Vector2 AdjustYToStayInCanvas(Vector2 anchoredPos, RectTransform tooltipRT, RectTransform canvasRT)
    {
        float tooltipHeight = tooltipRT.rect.height;
        float padding = 10f;

        float halfTooltip = tooltipHeight * tooltipRT.pivot.y;
        float canvasMinY = canvasRT.rect.yMin + padding;
        float canvasMaxY = canvasRT.rect.yMax - padding;

        float bottom = anchoredPos.y - halfTooltip;
        float top = anchoredPos.y + (tooltipHeight - halfTooltip);

        if (bottom < canvasMinY)
            anchoredPos.y += canvasMinY - bottom;
        else if (top > canvasMaxY)
            anchoredPos.y -= top - canvasMaxY;

        return anchoredPos;
    }
}
