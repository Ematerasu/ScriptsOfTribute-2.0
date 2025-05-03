using System.Collections;
using System.Collections.Generic;
using ScriptsOfTribute;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.Cards;
using UnityEngine;
using System.Linq;
using ScriptsOfTribute.Serializers;

public class CompletedActionProcessor : MonoBehaviour
{
    private Queue<VisualCommand> _visualQueue = new();

    private HashSet<UniqueId> _prevPlayer1Hand = new();
    private HashSet<UniqueId> _prevPlayer1Played = new();
    private HashSet<UniqueId> _prevPlayer1Cooldown = new();
    private HashSet<UniqueId> _prevPlayer1Draw = new();
    private HashSet<(UniqueId, bool, int)> _prevPlayer1Agents = new();
    private int _prevPlayer1Power = 0;

    private HashSet<UniqueId> _prevPlayer2Hand = new();
    private HashSet<UniqueId> _prevPlayer2Played = new();
    private HashSet<UniqueId> _prevPlayer2Cooldown = new();
    private HashSet<UniqueId> _prevPlayer2Draw = new();
    private HashSet<(UniqueId, bool, int)> _prevPlayer2Agents = new();
    private int _prevPlayer2Power = 0;

    private HashSet<UniqueId> _prevTavernAvailable = new();
    private HashSet<UniqueId> _prevTavernPile = new();

    public bool IsBusy => _visualQueue.Count > 0;
    public int ElementsInQueue => _visualQueue.Count;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log($"Na kolejce jest aktualnie {ElementsInQueue} elementów");
        }
    }

    private void ClearProcessor()
    {
        _visualQueue.Clear();
    }

    public void SetInitialSnapshot(FullGameState state)
    {
        ClearProcessor();
        var humanPlayer = state.GetPlayer(GameManager.Instance.HumanPlayer);
        var aiPlayer = state.GetPlayer(GameManager.Instance.AIPlayer);
        
        _prevPlayer1Hand = humanPlayer.Hand.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer1Played = humanPlayer.Played.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer1Cooldown = humanPlayer.CooldownPile.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer1Draw = humanPlayer.DrawPile.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer1Agents = humanPlayer.Agents.Select(agent => (agent.RepresentingCard.UniqueId, agent.Activated, agent.CurrentHp)).ToHashSet();
        _prevPlayer1Power = humanPlayer.Power;

        _prevPlayer2Hand = aiPlayer.Hand.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer2Played = aiPlayer.Played.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer2Cooldown = aiPlayer.CooldownPile.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer2Draw = aiPlayer.DrawPile.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer2Agents = aiPlayer.Agents.Select(agent => (agent.RepresentingCard.UniqueId, agent.Activated, agent.CurrentHp)).ToHashSet();
        _prevPlayer2Power = aiPlayer.Power;

        _prevTavernAvailable = state.TavernAvailableCards.Select(c => c.UniqueId).ToHashSet();
        _prevTavernPile = state.TavernCards.Select(c => c.UniqueId).ToHashSet();

        //Debug.Log("[Snapshot] Initial snapshot set.");
    }

    public void CompareAndQueueChanges(FullGameState newState)
    {
        var humanPlayer = newState.GetPlayer(GameManager.Instance.HumanPlayer);
        var aiPlayer = newState.GetPlayer(GameManager.Instance.AIPlayer);

        CompareAndQueueAgents(_prevPlayer1Agents, humanPlayer.Agents, aiPlayer.Power - _prevPlayer2Power, ZoneSide.Player1, _prevPlayer1Agents);
        CompareAndQueue(_prevPlayer1Draw, humanPlayer.DrawPile, ZoneType.DrawPile, ZoneSide.Player1, _prevPlayer1Draw);
        CompareAndQueue(_prevPlayer1Cooldown, humanPlayer.CooldownPile, ZoneType.CooldownPile, ZoneSide.Player1, _prevPlayer1Cooldown);
        CompareAndQueue(_prevPlayer1Played, humanPlayer.Played, ZoneType.PlayedPile, ZoneSide.Player1, _prevPlayer1Played);
        CompareAndQueue(_prevPlayer1Hand, humanPlayer.Hand, ZoneType.Hand, ZoneSide.Player1, _prevPlayer1Hand);
        
        CompareAndQueueAgents(_prevPlayer2Agents, aiPlayer.Agents, humanPlayer.Power - _prevPlayer1Power, ZoneSide.Player2, _prevPlayer2Agents);
        CompareAndQueue(_prevPlayer2Draw, aiPlayer.DrawPile, ZoneType.DrawPile, ZoneSide.Player2, _prevPlayer2Draw);
        CompareAndQueue(_prevPlayer2Cooldown, aiPlayer.CooldownPile, ZoneType.CooldownPile, ZoneSide.Player2, _prevPlayer2Cooldown);
        CompareAndQueue(_prevPlayer2Played, aiPlayer.Played, ZoneType.PlayedPile, ZoneSide.Player2, _prevPlayer2Played);
        CompareAndQueue(_prevPlayer2Hand, aiPlayer.Hand, ZoneType.Hand, ZoneSide.Player2, _prevPlayer2Hand);

        CompareAndQueue(_prevTavernPile, newState.TavernCards, ZoneType.Tavern, ZoneSide.Neutral, _prevTavernPile);
        CompareAndQueue(_prevTavernAvailable, newState.TavernAvailableCards, ZoneType.TavernAvailable, ZoneSide.Neutral, _prevTavernAvailable);

        _prevPlayer1Power = humanPlayer.Power;
        _prevPlayer2Power = aiPlayer.Power;
    }

    private void CompareAndQueue(HashSet<UniqueId> previousSet, List<UniqueCard> currentList, ZoneType zoneType, ZoneSide zoneSide, HashSet<UniqueId> storage)
    {
        var currentIds = currentList.Select(c => c.UniqueId).ToHashSet();
        var added = currentIds.Except(previousSet);

        foreach (var id in added)
        {
            if (!BoardManager.Instance.HasCardObject(id))
            {
                var card = currentList.FirstOrDefault(c => c.UniqueId == id);
                if (card != null)
                {
                    var sourceOfCreation = GetSourceOfCreatedCard(card);
                    BoardManager.Instance.CreateCardObjectFromRuntime(card, sourceOfCreation, ZoneSide.Neutral);
                }
                else
                {
                    Debug.LogError($"[CompletedActionProcessor] Missing UniqueCard for ID: {id.Value}");
                }
            }
            _visualQueue.Enqueue(new MoveCardCommand(id, zoneType, zoneSide));
        }

        storage.Clear();
        foreach (var id in currentIds)
            storage.Add(id);
    }

    private void CompareAndQueueAgents(HashSet<(UniqueId, bool, int)> previousSet, List<SerializedAgent> currentAgents, int enemyPowerDiff, ZoneSide side, HashSet<(UniqueId, bool, int)> storage)
    {
        var currentSet = currentAgents.Select(agent => (agent.RepresentingCard.UniqueId, agent.Activated, agent.CurrentHp)).ToHashSet();
        var previousIds = previousSet.Select(pair => pair.Item1).ToHashSet();
        var currentIds = currentSet.Select(pair => pair.Item1).ToHashSet();

        var removedAgents = previousIds.Except(currentIds);

        foreach (var id in removedAgents)
        {
            if (enemyPowerDiff < 0)
            {
                _visualQueue.Enqueue(new PlayProjectileCommand(id, side));
            }
            if (BoardManager.Instance.HasCardObject(id))
            {
                var cardObj = BoardManager.Instance.GetCardObject(id);
                _visualQueue.Enqueue(new ShowAgentActivationCommand(cardObj, false));
            }
            _visualQueue.Enqueue(new MoveCardCommand(id, ZoneType.CooldownPile, side));
        }

        var added = currentIds.Except(previousIds);
        foreach (var id in added)
        {
            if (!BoardManager.Instance.HasCardObject(id))
            {
                var agent = currentAgents.FirstOrDefault(a => a.RepresentingCard.UniqueId == id);
                if (agent != null)
                {
                    BoardManager.Instance.CreateCardObjectFromRuntime(agent.RepresentingCard, ZoneType.Agents, side);
                }
                else
                {
                    Debug.LogError($"[CompletedActionProcessor] Missing Agent for ID: {id.Value}");
                }
            }
            _visualQueue.Enqueue(new MoveCardCommand(id, ZoneType.Agents, side));
        }

        foreach (var (id, activated, currentHealth) in currentSet)
        {
            var old = previousSet.FirstOrDefault(x => x.Item1 == id);
            if (old != default && old.Item2 != activated)
            {
                if (BoardManager.Instance.HasCardObject(id))
                {
                    var cardObj = BoardManager.Instance.GetCardObject(id);
                    _visualQueue.Enqueue(new ShowAgentActivationCommand(cardObj, activated));
                }
            }
            if (old != default && old.Item3 > currentHealth)
            {
                if (BoardManager.Instance.HasCardObject(id))
                {
                    _visualQueue.Enqueue(new PlayProjectileCommand(id, side));
                }
            }
        }
        storage.Clear();
        foreach (var tuple in currentSet)
            storage.Add(tuple);
    }


    private void Start()
    {
        StartCoroutine(ConsumeQueue());
    }

    private IEnumerator ConsumeQueue()
    {
        while (true)
        {
            if (_visualQueue.Count > 0)
            {
                var cmd = _visualQueue.Dequeue();
                yield return cmd.Execute();
            }
            else
            {
                yield return null;
            }
        }
    }

    private ZoneType GetSourceOfCreatedCard(UniqueCard card)
    {
        return card.CommonId switch
        {
            CardId.WRIT_OF_COIN => ZoneType.TREASURY,
            CardId.CHAINBREAKER_SERGEANT => ZoneType.SAINT_ALESSIA,
            CardId.SOLDIER_OF_THE_EMPIRE => ZoneType.SAINT_ALESSIA,
            CardId.SUMMERSET_SACKING => ZoneType.ORGNUM,
            CardId.BEWILDERMENT => ZoneType.RAJHIN,
            _ => ZoneType.Tavern
        };
    }
}
