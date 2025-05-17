using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScriptsOfTribute.Serializers;
using ScriptsOfTribute.Board.Cards;
using System.Collections.Generic;
using ScriptsOfTribute;
using System.Collections;

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
    private int minChoices;
    private int maxChoices;

    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;
    private Coroutine confirmEnableRoutine;

    private void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirmCardChoice);
        cardChoicePanel.SetActive(false);
        confirmButton.interactable = false;
        canvasGroup = cardChoicePanel.GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            StartFade(false);
        }
        else if (Input.GetKeyUp(KeyCode.H))
        {
            StartFade(true);
        }
    }

    private void StartFade(bool show)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeCoroutine(show));
    }

    private IEnumerator FadeCoroutine(bool show)
    {
        float duration = 0.1f;
        float start = canvasGroup.alpha;
        float end = show ? 1f : 0f;
        float t = 0f;

        canvasGroup.interactable = show;
        canvasGroup.blocksRaycasts = true;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }

        canvasGroup.alpha = end;
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
        minChoices = choice.MinChoices;
        maxChoices = choice.MaxChoices;
        cardChoicePanel.SetActive(true);
        if (confirmEnableRoutine != null)
            StopCoroutine(confirmEnableRoutine);
        confirmEnableRoutine = StartCoroutine(EnableConfirmButtonAfterDelay(0.5f));
        UIManager.Instance.ShowHint("Hold [H] to hide");
    }

    private void OnCardClicked(UniqueCard card)
    {
        bool wasSelected = selectedCards.Contains(card);
        bool selectionChanged = false;

        if (wasSelected)
        {
            selectedCards.Remove(card);
            selectionChanged = true;
        }
        else if (selectedCards.Count < maxChoices)
        {
            selectedCards.Add(card);
            selectionChanged = true;
        }
        else
        {
            // Max reached
            var allButtons = cardContentParent.GetComponentsInChildren<CardChoiceButton>();
            foreach (var btn in allButtons)
            {
                if (btn.Card == card)
                {
                    btn.FlashRedOutline();
                    break;
                }
            }
            return;
        }

        if (selectionChanged)
        {
            int count = selectedCards.Count;
            progressSlider.SetSelectedCount(count);
            confirmButton.interactable = count >= minChoices;
        }
        confirmButton.interactable = minChoices <= selectedCards.Count;
    }

    public void OnConfirmCardChoice()
    {
        HideCardChoice();
        GameManager.Instance.MakeChoice(selectedCards);
        confirmButton.interactable = false;
    }

    public void HideCardChoice()
    {
        UIManager.Instance.HideHint();
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

    private IEnumerator EnableConfirmButtonAfterDelay(float delay)
    {
        confirmButton.interactable = false;
        yield return new WaitForSecondsRealtime(delay);
        confirmButton.interactable = minChoices <= selectedCards.Count;
    }
}
