using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScriptsOfTribute.Serializers;
using ScriptsOfTribute.Board.Cards;
using System.Collections.Generic;
using ScriptsOfTribute;

public class CardChoicePanelController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject cardChoicePanel;
    [SerializeField] private TextMeshProUGUI cardChoiceTitle;
    [SerializeField] private Transform cardContentParent;
    [SerializeField] private GameObject cardChoiceButtonPrefab;
    [SerializeField] private ChoiceProgressSlider progressSlider;
    [SerializeField] private Button confirmButton;

    private SerializedChoice currentChoice;
    private List<UniqueCard> selectedCards = new();

    private void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirmCardChoice);
        cardChoicePanel.SetActive(false);
    }

    public void ShowCardChoice(SerializedChoice choice)
    {
        currentChoice = choice;
        selectedCards.Clear();

        cardChoiceTitle.text = ToDisplayString(choice.ChoiceFollowUp);

        progressSlider.SetBounds(choice.MinChoices, choice.MaxChoices);
        progressSlider.SetSelectedCount(0);

        foreach (Transform child in cardContentParent)
            Destroy(child.gameObject);

        foreach (var card in choice.PossibleCards)
        {
            var btn = Instantiate(cardChoiceButtonPrefab, cardContentParent);
            var controller = btn.GetComponent<CardChoiceButton>();
            controller.Initialize(card, OnCardClicked);
        }

        cardChoicePanel.SetActive(true);
    }

    private void OnCardClicked(UniqueCard card)
    {
        if (selectedCards.Contains(card))
            selectedCards.Remove(card);
        else if (selectedCards.Count < currentChoice.MaxChoices)
            selectedCards.Add(card);

        int count = selectedCards.Count;
        progressSlider.SetSelectedCount(count);
        confirmButton.interactable = count >= currentChoice.MinChoices;
    }

    public void OnConfirmCardChoice()
    {
        HideCardChoice();
        GameManager.Instance.MakeChoice(selectedCards);
    }

    public void HideCardChoice()
    {
        cardChoicePanel.SetActive(false);
    }

    private string ToDisplayString(ChoiceFollowUp followUp)
    {
        return followUp switch
        {
            ChoiceFollowUp.ENACT_CHOSEN_EFFECT => "Choose one effect",
            ChoiceFollowUp.REPLACE_CARDS_IN_TAVERN => "Replace cards in the Tavern",
            ChoiceFollowUp.DESTROY_CARDS => "Choose cards to destroy",
            ChoiceFollowUp.DISCARD_CARDS => "Discard cards from hand",
            ChoiceFollowUp.REFRESH_CARDS => "Return cards to top of draw pile",
            ChoiceFollowUp.TOSS_CARDS => "Toss cards from draw pile to Cooldown pile",
            ChoiceFollowUp.KNOCKOUT_AGENTS => "Knockout enemy agents",
            ChoiceFollowUp.ACQUIRE_CARDS => "Acquire cards from the Tavern",
            ChoiceFollowUp.COMPLETE_HLAALU => "Complete Hlaalu contract",
            ChoiceFollowUp.COMPLETE_PELLIN => "Complete Pellin contract",
            ChoiceFollowUp.COMPLETE_PSIJIC => "Complete Psijic contract",
            ChoiceFollowUp.COMPLETE_TREASURY => "Choose card to pay for Treasury",
            _ => "Make a choice"
        };
    }
}
