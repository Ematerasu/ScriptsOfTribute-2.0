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

    [Header("Player Zones")]
    [SerializeField] private Transform playerDrawPile;
    [SerializeField] private Transform playerCooldown;
    [SerializeField] private Transform playerHand;
    [SerializeField] private Transform playerPlayed;
    [SerializeField] private Transform playerAgents;

    [Header("Enemy Zones")]
    [SerializeField] private Transform enemyDrawPile;
    [SerializeField] private Transform enemyCooldown;
    [SerializeField] private Transform enemyHand;
    [SerializeField] private Transform enemyPlayed;
    [SerializeField] private Transform enemyAgents;

    [Header("Tavern")]
    [SerializeField] private Transform tavernPile;
    [SerializeField] private List<Transform> tavernCardSpots;
        
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI playerGold;
    [SerializeField] private TextMeshProUGUI playerPrestige;
    [SerializeField] private TextMeshProUGUI playerPower;
    [SerializeField] private TextMeshProUGUI playerPatronCalls;
    [SerializeField] private TextMeshProUGUI enemyGold;
    [SerializeField] private TextMeshProUGUI enemyPrestige;
    [SerializeField] private TextMeshProUGUI enemyPower;
    [SerializeField] private TextMeshProUGUI enemyPatronCalls;
    
    [Header("Prefabs / Pool")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform offscreenCardContainer;
    [SerializeField] private Transform powerOriginPointPlayer1;
    [SerializeField] private Transform powerOriginPointPlayer2;
    [SerializeField] private GameObject powerProjectilePrefab;

    // Tracking which UniqueCard -> the instantiated card object
    private Dictionary<UniqueId, GameObject> cardObjects = new Dictionary<UniqueId, GameObject>();
    public bool HasCardObject(UniqueId id) => cardObjects.ContainsKey(id);
    public GameObject GetCardObject(UniqueId id) => cardObjects[id];
    private Dictionary<PatronId, Transform> patronTransforms = new();

    private HashSet<Transform> reservedTavernSpots = new();


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterPatronTransform(PatronId patronId, Transform transform)
    {
        patronTransforms[patronId] = transform;
    }

    public void ClearBoard()
    {
        foreach (var kvp in cardObjects)
        {
            Destroy(kvp.Value);
        }

        cardObjects.Clear();
    }

    public void InitializeBoard(FullGameState state)
    {
        ClearBoard();
        // 1) Setup Player 1
        SetupPlayer(state.CurrentPlayer, isPlayer: state.CurrentPlayer.PlayerID == GameManager.Instance.HumanPlayer);

        // 2) Setup Player 2
        SetupPlayer(state.EnemyPlayer, isPlayer: state.EnemyPlayer.PlayerID == GameManager.Instance.HumanPlayer);

        // 3) Setup Tavern
        SetupTavern(state);

        // 4) UI
        SetUpUI(state);

    }

    public void UpdateBoard(FullGameState state, UpdateReason reason)
    {
        StartCoroutine(UpdateAgentHealthCoroutine(state));
        SetUpUI(state);
        UIManager.Instance.UpdateCombosPanel(state.ComboStates);
    }

    private void SetupPlayer(SerializedPlayer playerData, bool isPlayer)
    {
        // Decide which transforms to use
        Transform drawPile   = isPlayer ? playerDrawPile   : enemyDrawPile;
        Transform cooldown   = isPlayer ? playerCooldown   : enemyCooldown;
        Transform hand       = isPlayer ? playerHand       : enemyHand;
        Transform played     = isPlayer ? playerPlayed     : enemyPlayed;
        Transform agents     = isPlayer ? playerAgents     : enemyAgents;

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
        CardLayoutManager.Instance.ScheduleLayout(ZoneType.Hand, isPlayer ? ZoneSide.HumanPlayer : ZoneSide.EnemyPlayer);
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

        foreach (var agent in playerData.Agents)
        {
            CreateCardObject(agent.RepresentingCard, agents);
        }
        CardLayoutManager.Instance.ScheduleLayout(ZoneType.Agents, isPlayer ? ZoneSide.HumanPlayer : ZoneSide.EnemyPlayer);

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
        SerializedPlayer player1 = state.CurrentPlayer.PlayerID == GameManager.Instance.HumanPlayer ? state.CurrentPlayer : state.EnemyPlayer;
        SerializedPlayer player2 = state.CurrentPlayer.PlayerID == GameManager.Instance.AIPlayer ? state.CurrentPlayer : state.EnemyPlayer;
        playerGold.SetText(player1.Coins.ToString());
        playerPrestige.SetText(player1.Prestige.ToString());
        playerPower.SetText(player1.Power.ToString());
        playerPatronCalls.SetText(player1.PatronCalls.ToString());
        enemyGold.SetText(player2.Coins.ToString());
        enemyPrestige.SetText(player2.Prestige.ToString());
        enemyPower.SetText(player2.Power.ToString());
        enemyPatronCalls.SetText(player2.PatronCalls.ToString());
    }

    private GameObject CreateCardObject(UniqueCard cardData, Transform parentTransform)
    {
        GameObject cardObj = CardManager.Instance.CreateCardVisual(cardData);
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
        CreateCardObject(card, parentTransform);
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
            LeanTween.move(cardObj, drawTransform.position, 0.35f)
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
            
            int sortingIndex = -1;
            for (int i = 0; i < targetZone.childCount; i++)
            {
                if (targetZone.GetChild(i) == cardObj.transform)
                {
                    sortingIndex = i;
                    break;
                }
            }

            var card = cardObj.GetComponent<Card>();
            if (card != null)
            {
                card.SetZoneInfo(zoneType, zoneSide);
                card.SetAnimating(false);
                card.SetVisible(card.ShouldBeVisible());
            }
            var layout = cardObj.GetComponent<CardLayoutBehaviour>();
            if (layout != null)
            {
                layout.ApplyLayout();
                layout.SetSortingOrder(sortingIndex >= 0 ? sortingIndex : 0);
            }

            onComplete?.Invoke();
        });
    }

    public Transform GetZoneTransform(ZoneType zone, ZoneSide side)
    {
        if (zone == ZoneType.RAJHIN && patronTransforms.TryGetValue(PatronId.RAJHIN, out var rajhinTransform))
            return rajhinTransform;
        if (zone == ZoneType.SAINT_ALESSIA && patronTransforms.TryGetValue(PatronId.SAINT_ALESSIA, out var alessiaTransform))
            return alessiaTransform;
        if (zone == ZoneType.ORGNUM && patronTransforms.TryGetValue(PatronId.ORGNUM, out var orgnumTransform))
            return orgnumTransform;
        if (zone == ZoneType.TREASURY && patronTransforms.TryGetValue(PatronId.TREASURY, out var treasuryTransform))
            return treasuryTransform;

        return (zone, side) switch
        {
            (ZoneType.Hand, ZoneSide.HumanPlayer) => playerHand,
            (ZoneType.Hand, ZoneSide.EnemyPlayer) => enemyHand,
            (ZoneType.PlayedPile, ZoneSide.HumanPlayer) => playerPlayed,
            (ZoneType.PlayedPile, ZoneSide.EnemyPlayer) => enemyPlayed,
            (ZoneType.CooldownPile, ZoneSide.HumanPlayer) => playerCooldown,
            (ZoneType.CooldownPile, ZoneSide.EnemyPlayer) => enemyCooldown,
            (ZoneType.DrawPile, ZoneSide.HumanPlayer) => playerDrawPile,
            (ZoneType.DrawPile, ZoneSide.EnemyPlayer) => enemyDrawPile,
            (ZoneType.Agents, ZoneSide.HumanPlayer) => playerAgents,
            (ZoneType.Agents, ZoneSide.EnemyPlayer) => enemyAgents,
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

        foreach (Transform child in enemyAgents)
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

        foreach (Transform child in enemyAgents)
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

    public void DebugCheckHandSync(FullGameState state)
    {
        var correctHand = state.EnemyPlayer.Hand.Select(c => c.UniqueId.Value).OrderBy(x => x).ToList();
        var visualHand = GetCardsInZone(ZoneType.Hand, ZoneSide.EnemyPlayer).Select(c => c.GetCard().UniqueId.Value).OrderBy(x => x).ToList();

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