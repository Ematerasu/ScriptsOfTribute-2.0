using System.Collections.Generic;
using UnityEngine;
using ScriptsOfTribute;
using ScriptsOfTribute.Serializers;
using ScriptsOfTribute.Board.Cards;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Unity.VisualScripting;
using System.Collections;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("Player 1 Zones")]
    [SerializeField] private Transform p1DrawPile;
    [SerializeField] private Transform p1Cooldown;
    [SerializeField] private Transform p1Hand;
    [SerializeField] private Transform p1Played;
    [SerializeField] private Transform p1Agents;

    [Header("Player 2 Zones")]
    [SerializeField] private Transform p2DrawPile;
    [SerializeField] private Transform p2Cooldown;
    [SerializeField] private Transform p2Hand;
    [SerializeField] private Transform p2Played;
    [SerializeField] private Transform p2Agents;

    [Header("Tavern")]
    [SerializeField] private Transform tavernPile;
    [SerializeField] private List<Transform> tavernCardSpots;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI Player1Gold;
    [SerializeField] private TextMeshProUGUI Player1Prestige;
    [SerializeField] private TextMeshProUGUI Player1Power;
    [SerializeField] private TextMeshProUGUI Player2Gold;
    [SerializeField] private TextMeshProUGUI Player2Prestige;
    [SerializeField] private TextMeshProUGUI Player2Power;
    
    [Header("Prefabs / Pool")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform offscreenCardContainer;

    // Tracking which UniqueCard -> the instantiated card object
    private Dictionary<UniqueId, GameObject> cardObjects = new Dictionary<UniqueId, GameObject>();

    private List<UniqueId> currentTavernCards;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void InitializeBoard(FullGameState state)
    {
        currentTavernCards = state.TavernAvailableCards.Select(c => c.UniqueId).ToList();
        foreach (var kvp in cardObjects)
        {
            Destroy(kvp.Value);
        }
        cardObjects.Clear();

        // 1) Setup Player 1
        SetupPlayer(state.CurrentPlayer, isPlayer1: true);

        // 2) Setup Player 2
        SetupPlayer(state.EnemyPlayer, isPlayer1: false);

        // 3) Setup Tavern
        SetupTavern(state);

        // 4) UI
        SetUpUI(state);

    }

    public void UpdateBoard(FullGameState state, UpdateReason reason)
    {
        StartCoroutine(UpdateAgentHealthCoroutine(state));
        SetUpUI(state);
    }

    private void SetupPlayer(SerializedPlayer playerData, bool isPlayer1)
    {
        // Decide which transforms to use
        Transform drawPile   = isPlayer1 ? p1DrawPile   : p2DrawPile;
        Transform cooldown   = isPlayer1 ? p1Cooldown   : p2Cooldown;
        Transform hand       = isPlayer1 ? p1Hand       : p2Hand;
        Transform played     = isPlayer1 ? p1Played     : p2Played;

        // Draw Pile
        foreach (var card in playerData.DrawPile)
        {
            CreateCardObject(card, drawPile);
        }
        // Hand
        foreach (var card in playerData.Hand)
        {
            CreateCardObject(card, hand);
        }
        ArrangeCardsInHand(hand);
        // Cooldown
        foreach (var card in playerData.CooldownPile)
        {
            CreateCardObject(card, cooldown);
        }
        // Played
        foreach (var card in playerData.Played)
        {
            CreateCardObject(card, played);
        }

    }

    private void SetupTavern(FullGameState state)
    {
        foreach (var card in state.TavernCards)
        {
            CreateCardObject(card, tavernPile);
        }

        for (int i = 0; i < state.TavernAvailableCards.Count; i++)
        {
            if (i < tavernCardSpots.Count)
            {
                var card = state.TavernAvailableCards[i];
                CreateCardObject(card, tavernCardSpots[i]);
            }
        }

    }

    private void SetUpUI(FullGameState state)
    {
        SerializedPlayer player1 = state.CurrentPlayer.PlayerID == PlayerEnum.PLAYER1 ? state.CurrentPlayer : state.EnemyPlayer;
        SerializedPlayer player2 = state.CurrentPlayer.PlayerID == PlayerEnum.PLAYER2 ? state.CurrentPlayer : state.EnemyPlayer;
        Player1Gold.SetText(player1.Coins.ToString());
        Player1Prestige.SetText(player1.Prestige.ToString());
        Player1Power.SetText(player1.Power.ToString());
        Player2Gold.SetText(player2.Coins.ToString());
        Player2Prestige.SetText(player2.Prestige.ToString());
        Player2Power.SetText(player2.Power.ToString());
    }

    private GameObject CreateCardObject(UniqueCard cardData, Transform parentTransform)
    {
        GameObject cardObj = Instantiate(cardPrefab);
        cardObj.transform.SetParent(parentTransform, true);
        cardObj.transform.localPosition = Vector3.zero;

        var controller = cardObj.GetComponent<Card>();
        if (controller != null)
        {
            controller.InitializeCard(cardData);
            
            var zone = parentTransform.GetComponent<CardZone>();
            if (zone != null)
            {
                controller.SetZoneInfo(zone.zoneType, zone.zoneSide);
                controller.SetVisible(controller.ShouldBeVisible());
                // if (visible)
                //     Debug.Log($"Card {cardData.Name} ({cardData.UniqueId.Value}) is in {zone.zoneType} {zone.zoneSide}");
            }
        }
        var layout = cardObj.GetComponent<CardLayoutBehaviour>();
        if (layout != null)
            layout.ApplyLayout();
        cardObjects[cardData.UniqueId] = cardObj;
        return cardObj;
    }

    public void ArrangeCardsInHand(Transform handTransform)
    {
        float cardSpacing = 0.8f;
        int count = handTransform.childCount;

        float totalWidth = (count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            Transform card = handTransform.GetChild(i);
            var cardScript = card.GetComponent<Card>();
            cardScript.SetAnimating(true);
            Vector3 targetPos = new Vector3(startX + i * cardSpacing, 0, 0);
            card.localPosition = targetPos;
            cardScript.SetLayoutPosition(targetPos);

            SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = i+1;
            }
            cardScript.SetAnimating(false);
        }
    }

    public void ArrangeAgentsInZone(Transform zoneTransform)
    {
        float cardWidth = 1.2f;
        int count = zoneTransform.childCount;
        if (count == 0) return;

        float totalWidth = (count - 1) * cardWidth;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            Transform card = zoneTransform.GetChild(i);
            Vector3 targetPos = new Vector3(startX + i * cardWidth, 0, 0);
            card.localPosition = targetPos;

            Card logic = card.GetComponent<Card>();
            logic?.SetLayoutPosition(targetPos);
        }
    }

    public void MoveCardToZone(UniqueId cardId, Transform targetZone, ZoneType zoneType, ZoneSide zoneSide, System.Action? onComplete = null)
    {
        if (!cardObjects.TryGetValue(cardId, out var cardObj))
            return;

        var card = cardObj.GetComponent<Card>();
        if (card != null)
            card.SetAnimating(true);
        LeanTween.move(cardObj, targetZone.position, 0.2f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            cardObj.transform.SetParent(targetZone, false);
            cardObj.transform.localPosition = Vector3.zero;
            
            var cardsInZoneCnt = GetCardsInZone(zoneType, zoneSide).Count + 1;
            SpriteRenderer sr = cardObj.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sortingOrder = cardsInZoneCnt;

            var card = cardObj.GetComponent<Card>();
            if (card != null)
            {
                card.SetZoneInfo(zoneType, zoneSide);
                card.SetAnimating(false);
                card.SetVisible(card.ShouldBeVisible());
            }
            var layout = cardObj.GetComponent<CardLayoutBehaviour>();
            if (layout != null)
                layout.ApplyLayout();

            onComplete?.Invoke();
        });
    }

    public Transform GetZoneTransform(ZoneType zone, ZoneSide side)
    {
        return (zone, side) switch
        {
            (ZoneType.Hand, ZoneSide.Player1) => p1Hand,
            (ZoneType.Hand, ZoneSide.Player2) => p2Hand,
            (ZoneType.PlayedPile, ZoneSide.Player1) => p1Played,
            (ZoneType.PlayedPile, ZoneSide.Player2) => p2Played,
            (ZoneType.CooldownPile, ZoneSide.Player1) => p1Cooldown,
            (ZoneType.CooldownPile, ZoneSide.Player2) => p2Cooldown,
            (ZoneType.DrawPile, ZoneSide.Player1) => p1DrawPile,
            (ZoneType.DrawPile, ZoneSide.Player2) => p2DrawPile,
            (ZoneType.Agents, ZoneSide.Player1) => p1Agents,
            (ZoneType.Agents, ZoneSide.Player2) => p2Agents,
            (ZoneType.Tavern, ZoneSide.Neutral) => tavernPile,
            _ => offscreenCardContainer
        };
    }
    public List<Card> GetCardsInZone(ZoneType zone, ZoneSide side)
    {
        List<Card> result = new List<Card>();

        foreach (var go in cardObjects.Values)
        {
            var card = go.GetComponent<Card>();
            if (card != null && card.IsInZone(zone, side))
            {
                result.Add(card);
            }
        }

        return result;
    }

    private Transform FindFirstFreeTavernSpot()
    {
        foreach (var spot in tavernCardSpots)
        {
            if (spot.childCount == 0) return spot;
        }

        Debug.LogWarning("Nie znaleziono wolnego slotu w tawernie!");
        return null;
    }

    public IEnumerator AnimateAddCardToTavernDelayed(UniqueId card)
    {
        yield return new WaitUntil(() => FindFirstFreeTavernSpot() != null);

        Transform freeSpot = FindFirstFreeTavernSpot();
        if (freeSpot == null)
        {
            Debug.LogError("Slot powinien być wolny, ale nie jest!");
            yield break;
        }

        MoveCardToZone(card, freeSpot, ZoneType.TavernAvailable, ZoneSide.Neutral);
    }

    public void DestroyCards(List<UniqueCard> cardsToDestroy)
    {
        foreach(var card in cardsToDestroy)
        {
            Destroy(cardObjects[card.UniqueId]);
            cardObjects.Remove(card.UniqueId);
        }
    }

    private IEnumerator UpdateAgentHealthCoroutine(FullGameState state)
    {
        yield return null;

        SerializedPlayer p1 = GameManager.Instance.HumanPlayer == PlayerEnum.PLAYER1 ? state.CurrentPlayer : state.EnemyPlayer;
        SerializedPlayer p2 = GameManager.Instance.HumanPlayer == PlayerEnum.PLAYER2 ? state.CurrentPlayer : state.EnemyPlayer;

        foreach (Transform child in p1Agents)
        {
            Card card = child.GetComponent<Card>();
            if (card != null)
            {
                var matching = p1.Agents.FirstOrDefault(agent => agent.RepresentingCard == card.GetCard());
                if (matching != null)
                {
                    card.UpdateAgentHealth(matching);
                }
            }
            yield return null;
        }

        foreach (Transform child in p2Agents)
        {
            Card card = child.GetComponent<Card>();
            if (card != null)
            {
                var matching = p2.Agents.FirstOrDefault(agent => agent.RepresentingCard == card.GetCard());
                if (matching != null)
                {
                    card.UpdateAgentHealth(matching);
                }
            }
            yield return null;
        }
    }

}

public enum UpdateReason
{
    PLAY_CARD,
    BUY_CARD,
    PATRON_ACTIVATION,
    CHOICE_MADE,
    AGENT_ACTIVATION,
    AGENT_ATTACK,
    END_TURN
}