using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScriptsOfTribute.Board.Cards;
using System;
using UnityEngine.EventSystems;

public class EffectChoiceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI effectLabel;
    [SerializeField] private Button button;
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;

    private UniqueEffect effect;
    private Action<UniqueEffect> onClickCallback;

    public void Initialize(UniqueEffect effect, Action<UniqueEffect> onClick)
    {
        this.effect = effect;
        this.onClickCallback = onClick;

        effectLabel.text = GetShortEffectLabel(effect);
        if (tooltipText != null)
        {
            tooltipText.text = GetLongEffectDescription(effect);
        }

        button.onClick.AddListener(OnClick);
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private void OnClick()
    {
        onClickCallback?.Invoke(effect);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private string GetShortEffectLabel(UniqueEffect effect)
    {
        return effect.Type switch
        {
            EffectType.GAIN_COIN => $"+{effect.Amount} Coin",
            EffectType.GAIN_POWER => $"+{effect.Amount} Power",
            EffectType.GAIN_PRESTIGE => $"+{effect.Amount} Prestige",
            EffectType.OPP_LOSE_PRESTIGE => $"Enemy -{effect.Amount} Prestige",
            EffectType.REPLACE_TAVERN => $"Replace {effect.Amount} Tavern cards",
            EffectType.ACQUIRE_TAVERN => $"Acquire card from Tavern ({effect.Amount})",
            EffectType.DESTROY_CARD => $"Destroy {effect.Amount} cards in play",
            EffectType.DRAW => $"Draw {effect.Amount} cards",
            EffectType.OPP_DISCARD => $"Opponent Discards {effect.Amount} cards",
            EffectType.RETURN_TOP => $"Return {effect.Amount} to Draw Pile",
            EffectType.TOSS => $"Toss {effect.Amount} to Cooldown",
            EffectType.KNOCKOUT => $"Knockout {effect.Amount} Agents",
            EffectType.PATRON_CALL => $"+{effect.Amount} Patron Call",
            EffectType.CREATE_SUMMERSET_SACKING => "Add Summerset Sacking",
            EffectType.DONATE => $"Swap {effect.Amount} cards",
            EffectType.KNOCKOUT_ALL => "Knockout all agents",
            EffectType.RETURN_AGENT_TOP => $"Return {effect.Amount} agents to Draw Pile",
            EffectType.HEAL => $"Heal {effect.Amount}",
            _ => effect.Type.ToString()
        };
    }

    private string GetLongEffectDescription(UniqueEffect effect)
    {
        return effect.Type switch
        {
            EffectType.GAIN_COIN => $"Gain {effect.Amount} Coin(s).",
            EffectType.GAIN_POWER => $"Gain {effect.Amount} Power.",
            EffectType.GAIN_PRESTIGE => $"Gain {effect.Amount} Prestige directly.",
            EffectType.OPP_LOSE_PRESTIGE => $"Opponent loses {effect.Amount} Prestige.",
            EffectType.REPLACE_TAVERN => $"Replace {effect.Amount} cards in the Tavern with new ones from the pile.",
            EffectType.ACQUIRE_TAVERN => $"Acquire 1 card from the Tavern costing up to {effect.Amount}.",
            EffectType.DESTROY_CARD => $"Destroy a card in play (from your hand, played pile or Agents).",
            EffectType.DRAW => $"Draw {effect.Amount} card(s) from your draw pile.",
            EffectType.OPP_DISCARD => $"Opponent must discard {effect.Amount} card(s) from hand at the start of his turn.",
            EffectType.RETURN_TOP => $"Return {effect.Amount} card(s) from cooldown pile to top of draw pile.",
            EffectType.TOSS => $"Send {effect.Amount} card(s) from draw pile to cooldown.",
            EffectType.KNOCKOUT => $"Knockout up to {effect.Amount} enemy agent(s).",
            EffectType.PATRON_CALL => $"Gain {effect.Amount} Patron Call(s).",
            EffectType.CREATE_SUMMERSET_SACKING => $"Add {effect.Amount} Summerset Sacking card(s) to your cooldown pile.",
            EffectType.DONATE => $"Discard {effect.Amount} cards, then draw the same amount.",
            EffectType.KNOCKOUT_ALL => "Knockout all agents from board.",
            EffectType.RETURN_AGENT_TOP => $"Return {effect.Amount} agents to top of the Draw Pile.",
            EffectType.HEAL => $"Heal this agent by {effect.Amount} HP.",
            _ => effect.Type.ToString()
        };
    }
}
