using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScriptsOfTribute.Serializers;
using ScriptsOfTribute.Board.Cards;
using System.Collections.Generic;
using ScriptsOfTribute;

public class EffectChoicePanelController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject effectChoicePanel;
    [SerializeField] private TextMeshProUGUI effectChoiceTitle;
    [SerializeField] private Transform effectContentParent;
    [SerializeField] private GameObject effectChoiceButtonPrefab;

    private SerializedChoice currentChoice;

    private void Awake()
    {
        effectChoicePanel.SetActive(false);
    }

    public void ShowEffectChoice(SerializedChoice choice)
    {
        currentChoice = choice;

        effectChoiceTitle.text = ToDisplayString(choice.ChoiceFollowUp);

        foreach (Transform child in effectContentParent)
            Destroy(child.gameObject);

        foreach (var effect in choice.PossibleEffects)
        {
            var btn = Instantiate(effectChoiceButtonPrefab, effectContentParent);
            var controller = btn.GetComponent<EffectChoiceButton>();
            controller.Initialize(effect, OnEffectClicked);
        }

        effectChoicePanel.SetActive(true);
    }

    private void OnEffectClicked(UniqueEffect effect)
    {
        GameManager.Instance.MakeChoice(effect);
        HideEffectChoice();
    }

    public void HideEffectChoice()
    {
        effectChoicePanel.SetActive(false);
    }

    private string ToDisplayString(ChoiceFollowUp followUp)
    {
        return followUp switch
        {
            ChoiceFollowUp.ENACT_CHOSEN_EFFECT => "Choose one effect",
            _ => "Make a choice"
        };
    }
}