using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System;
using TMPro;
using ScriptsOfTribute.Board.Cards;

public class CardLookupPanelController : MonoBehaviour
{

    [Header("UI Elements")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button playedPileButton;
    [SerializeField] private Button cooldownPileButton;
    [SerializeField] private Button drawPileButton;
    [SerializeField] private Transform cardContentParent;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Button closePanelButton;

    [Header("Button Scale Settings")]
    [SerializeField] private Vector3 selectedScale = Vector3.one;
    [SerializeField] private Vector3 unselectedScale = new Vector3(0.8f, 0.8f, 0.8f);

    [Header("Player 1 Zones")]
    [SerializeField] private Transform player1DrawPile;
    [SerializeField] private Transform player1PlayedPile;
    [SerializeField] private Transform player1CooldownPile;

    [Header("Player 2 Zones")]
    [SerializeField] private Transform player2DrawPile;
    [SerializeField] private Transform player2PlayedPile;
    [SerializeField] private Transform player2CooldownPile;
    [SerializeField] private Transform player2Hand;

    private Coroutine loadingCoroutine;
    private ZoneSide currentSide;
    private PileType currentPile;
    private bool isClosing = false;

    private void Awake()
    {
        closePanelButton.onClick.AddListener(Hide);

        drawPileButton.onClick.AddListener(() => SwitchPile(PileType.Draw));
        playedPileButton.onClick.AddListener(() => SwitchPile(PileType.Played));
        cooldownPileButton.onClick.AddListener(() => SwitchPile(PileType.Cooldown));
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Hide();
        }
    }

    public void Show(ZoneSide side, PileType startingPile)
    {
        currentSide = side;
        currentPile = startingPile;
        if (currentSide == ZoneSide.EnemyPlayer && !GameSetupManager.Instance.IsBotDebugMode)
        {
            var drawPileText = drawPileButton.GetComponentInChildren<TextMeshProUGUI>();
            drawPileText.text = "Draw pile & Hand";
        }
        else
        {
            var drawPileText = drawPileButton.GetComponentInChildren<TextMeshProUGUI>();
            drawPileText.text = "Draw pile";
        }
        panelRoot.SetActive(true);
        UpdateView();
    }

    public void Hide()
    {
        if (isClosing) return;

        isClosing = true;
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        StartCoroutine(FadeOutAndDisable());
    }

    private void SwitchPile(PileType newPile)
    {
        if (currentPile == newPile)
            return;

        currentPile = newPile;
        UpdateView();
    }

    private void UpdateView()
    {
        ClearContent();
        
        List<Card> cardObjects;

        if (currentSide == ZoneSide.EnemyPlayer && currentPile == PileType.Draw)
        {
            var drawTransform = GetPileTransform(ZoneSide.EnemyPlayer, PileType.Draw);

            if (GameSetupManager.Instance.IsBotDebugMode)
            {
                cardObjects = drawTransform.GetComponentsInChildren<Card>().ToList();
            }
            else
            {
                var handTransform = GetPileTransform(ZoneSide.EnemyPlayer, PileType.Hand);
                cardObjects = drawTransform.GetComponentsInChildren<Card>()
                    .Concat(handTransform.GetComponentsInChildren<Card>())
                    .ToList();
            }
        }
        else
        {
            var pileTransform = GetPileTransform(currentSide, currentPile);
            cardObjects = pileTransform.GetComponentsInChildren<Card>().ToList();
        }

        var entries = cardObjects
            .Select(c => (c.GetCard(), c.GetOriginalSprite(), c.hpText.text))
            .OrderBy(entry => entry.Item1.Name)
            .ToList();
        loadingCoroutine = StartCoroutine(AddToContentCoroutine(entries));
        UpdateButtonScales();
    }

    private void UpdateButtonScales()
    {
        drawPileButton.transform.localScale = (currentPile == PileType.Draw) ? selectedScale : unselectedScale;
        playedPileButton.transform.localScale = (currentPile == PileType.Played) ? selectedScale : unselectedScale;
        cooldownPileButton.transform.localScale = (currentPile == PileType.Cooldown) ? selectedScale : unselectedScale;
    }

    private Transform GetPileTransform(ZoneSide side, PileType pileType)
    {
        if (side == ZoneSide.HumanPlayer)
        {
            return pileType switch
            {
                PileType.Draw => player1DrawPile,
                PileType.Played => player1PlayedPile,
                PileType.Cooldown => player1CooldownPile,
                _ => null
            };
        }
        else
        {
            return pileType switch
            {
                PileType.Draw => player2DrawPile,
                PileType.Played => player2PlayedPile,
                PileType.Cooldown => player2CooldownPile,
                PileType.Hand => player2Hand,
                _ => null
            };
        }
    }

    private void ClearContent()
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        foreach (Transform child in cardContentParent)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator AddToContentCoroutine(List<(UniqueCard card, Sprite sprite, string hpText)> sprites)
    {
        foreach (var (card, sprite, hpText) in sprites)
        {
            var cardUI = Instantiate(cardPrefab, cardContentParent);
            cardUI.GetComponent<Image>().sprite = sprite;
            cardUI.GetComponentInChildren<TextMeshProUGUI>().SetText(hpText);
            cardUI.GetComponent<CardUITooltip>().SetCardData(card);
            yield return null;
        }

        StartCoroutine(FadeInContent());
    }

    private IEnumerator FadeInContent()
    {
        CanvasGroup canvasGroup = cardContentParent.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is missing on cardContentParent!");
            yield break;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        cardContentParent.gameObject.SetActive(true);

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private IEnumerator FadeOutAndDisable()
    {
        CanvasGroup canvasGroup = cardContentParent.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is missing on cardContentParent!");
            yield break;
        }

        float duration = 0.3f;
        float elapsed = 0f;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / duration));
            yield return null;
        }

        canvasGroup.alpha = 0f;

        panelRoot.SetActive(false);

        ClearContent();
        isClosing = false;
    }
}
