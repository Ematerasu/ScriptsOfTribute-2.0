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
    [SerializeField] private TextMeshProUGUI Player1PatronCalls;
    [SerializeField] private TextMeshProUGUI Player2Gold;
    [SerializeField] private TextMeshProUGUI Player2Prestige;
    [SerializeField] private TextMeshProUGUI Player2Power;
    [SerializeField] private TextMeshProUGUI Player2PatronCalls;
    
    [Header("Prefabs / Pool")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform offscreenCardContainer;
    [SerializeField] private Transform powerOriginPointPlayer1;
    [SerializeField] private Transform powerOriginPointPlayer2;
    [SerializeField] private GameObject powerProjectilePrefab;

    // Tracking which UniqueCard -> the instantiated card object
    private Dictionary<UniqueId, GameObject> cardObjects = new Dictionary<UniqueId, GameObject>();
    public bool HasCardObject(UniqueId id) => cardObjects.ContainsKey(id);

    private HashSet<Transform> reservedTavernSpots = new();


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void InitializeBoard(FullGameState state)
    {
        foreach (var kvp in cardObjects)
        {
            Destroy(kvp.Value);
        }
        cardObjects.Clear();

        // 1) Setup Player 1
        SetupPlayer(state.CurrentPlayer, isPlayer1: state.CurrentPlayer.PlayerID == PlayerEnum.PLAYER1);

        // 2) Setup Player 2
        SetupPlayer(state.EnemyPlayer, isPlayer1: state.EnemyPlayer.PlayerID == PlayerEnum.PLAYER1);

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
        Player1PatronCalls.SetText(player1.PatronCalls.ToString());
        Player2Gold.SetText(player2.Coins.ToString());
        Player2Prestige.SetText(player2.Prestige.ToString());
        Player2Power.SetText(player2.Power.ToString());
        Player2PatronCalls.SetText(player2.PatronCalls.ToString());
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

    public void CreateCardObjectFromRuntime(UniqueCard card, ZoneType initialZone, ZoneSide side)
    {
        if (cardObjects.ContainsKey(card.UniqueId))
        {
            Debug.LogWarning($"[BoardManager] Tried to create duplicate card object for {card.UniqueId.Value}");
            return;
        }

        var parentTransform = GetZoneTransform(initialZone, side);
        var obj = CreateCardObject(card, parentTransform);
        Debug.Log($"[BoardManager] [RUNTIME] Created card object: {card.Name} ({card.UniqueId.Value}) in {initialZone} ({side})");
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

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public void MoveCardToZone(UniqueId cardId, Transform targetZone, ZoneType zoneType, ZoneSide zoneSide, System.Action? onComplete = null)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    {
        if (!cardObjects.TryGetValue(cardId, out var cardObj))
            return;

        var card = cardObj.GetComponent<Card>();
        if (card != null)
            card.SetAnimating(true);

        if (zoneType == ZoneType.Hand && (card.ZoneType == ZoneType.PlayedPile || card.ZoneType == ZoneType.CooldownPile))
        {
            Transform drawTransform = GetZoneTransform(ZoneType.DrawPile, zoneSide);
            LeanTween.move(cardObj, drawTransform.position, 0.25f)
                .setEase(LeanTweenType.easeInOutSine)
                .setOnComplete(() =>
                {
                    cardObj.transform.SetParent(drawTransform, false);
                    cardObj.transform.localPosition = Vector3.zero;
                    card.SetZoneInfo(ZoneType.DrawPile, zoneSide);
                    MoveCardToZone(cardId, targetZone, zoneType, zoneSide, onComplete);
                });
            return;
        }
        LeanTween.move(cardObj, targetZone.position, 0.4f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
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
            bool isEmpty = spot.GetComponentInChildren<Card>() == null;
            if (isEmpty && !reservedTavernSpots.Contains(spot))
            {
                return spot;
            }
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
        reservedTavernSpots.Add(freeSpot);
        MoveCardToZone(card, freeSpot, ZoneType.TavernAvailable, ZoneSide.Neutral, () => reservedTavernSpots.Remove(freeSpot));
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
        yield return new WaitForSeconds(0.7f);

        SerializedPlayer p1 = GameManager.Instance.IsHumanPlayersTurn ? state.CurrentPlayer : state.EnemyPlayer;
        SerializedPlayer p2 = GameManager.Instance.IsHumanPlayersTurn ? state.EnemyPlayer : state.CurrentPlayer;

        foreach (Transform child in p1Agents)
        {
            Card card = child.GetComponent<Card>();
            if (card != null)
            {
                var matching = p1.Agents.FirstOrDefault(agent => agent.RepresentingCard == card.GetCard());
                if (matching != null)
                {
                    card.UpdateAgentHealth(matching);
                    if (matching.Activated)
                        card.ShowActivationEffect();
                    else
                        card.RemoveActivationEffect();
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
                    if (matching.Activated)
                        card.ShowActivationEffect();
                    else
                        card.RemoveActivationEffect();
                }
            }
            yield return null;
        }
    }

    public void DebugCheckHandSync(FullGameState state)
    {
        var correctHand = state.EnemyPlayer.Hand.Select(c => c.UniqueId.Value).OrderBy(x => x).ToList();
        var visualHand = GetCardsInZone(ZoneType.Hand, ZoneSide.Player2).Select(c => c.GetCard().UniqueId.Value).OrderBy(x => x).ToList();

        Debug.Log($"[SYNC] Engine hand (P2): {string.Join(", ", correctHand)}");
        Debug.Log($"[SYNC] Unity  hand (P2): {string.Join(", ", visualHand)}");

        var extras = visualHand.Except(correctHand).ToList();
        if (extras.Count > 0)
        {
            Debug.LogWarning($"[DESYNC] Extra card(s) in Unity hand: {string.Join(", ", extras)}");
        }

        var missing = correctHand.Except(visualHand).ToList();
        if (missing.Count > 0)
        {
            Debug.LogWarning($"[DESYNC] Missing card(s) in Unity hand: {string.Join(", ", missing)}");
        }
    }

    public void PlayPowerAttackEffect(UniqueId cardId, ZoneSide side, System.Action? onComplete = null)
    {
        Vector3 targetPosition = cardObjects[cardId].transform.position;
        Transform powerOrigin = side == ZoneSide.Player1 ? powerOriginPointPlayer1 : powerOriginPointPlayer2;
        GameObject projectile = Instantiate(powerProjectilePrefab, powerOrigin.position, Quaternion.identity, transform);

        float travelTime = 0.5f;

        Vector3 midPoint = Vector3.Lerp(powerOrigin.position, targetPosition, 0.5f);

        Vector3 curveOffset = new Vector3(-2f, 0, 0);
        if (side == ZoneSide.Player2) curveOffset *= -1f;

        midPoint += curveOffset;

        Vector3[] path = new Vector3[] {
            powerOrigin.position,
            powerOrigin.position,
            midPoint,
            targetPosition,
            targetPosition
        };

        LeanTween.moveSpline(projectile, path, travelTime)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(() =>
            {
                LeanTween.scale(projectile, Vector3.zero, 0.1f).setEase(LeanTweenType.easeInQuad).setOnComplete(() =>
                {
                    Destroy(projectile);
                    onComplete?.Invoke();
                });
            });
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