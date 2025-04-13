using System.Collections;
using System.Collections.Generic;
using ScriptsOfTribute;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.Cards;
using UnityEngine;
using System.Linq;

public class CompletedActionProcessor : MonoBehaviour
{
    private Queue<MoveVisualCommand> _visualQueue = new();

    private HashSet<UniqueId> _prevPlayer1Hand = new();
    private HashSet<UniqueId> _prevPlayer1Played = new();
    private HashSet<UniqueId> _prevPlayer1Cooldown = new();
    private HashSet<UniqueId> _prevPlayer1Draw = new();
    private HashSet<UniqueId> _prevPlayer1Agents = new();

    private HashSet<UniqueId> _prevPlayer2Hand = new();
    private HashSet<UniqueId> _prevPlayer2Played = new();
    private HashSet<UniqueId> _prevPlayer2Cooldown = new();
    private HashSet<UniqueId> _prevPlayer2Draw = new();
    private HashSet<UniqueId> _prevPlayer2Agents = new();

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

    public void SetInitialSnapshot(FullGameState state)
    {
        var humanPlayer = state.GetPlayer(GameManager.Instance.HumanPlayer);
        var aiPlayer = state.GetPlayer(GameManager.Instance.AIPlayer);

        _prevPlayer1Hand = humanPlayer.Hand.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer1Played = humanPlayer.Played.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer1Cooldown = humanPlayer.CooldownPile.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer1Draw = humanPlayer.DrawPile.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer1Agents = humanPlayer.Agents.Select(agent => agent.RepresentingCard.UniqueId).ToHashSet();

        _prevPlayer2Hand = aiPlayer.Hand.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer2Played = aiPlayer.Played.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer2Cooldown = aiPlayer.CooldownPile.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer2Draw = aiPlayer.DrawPile.Select(c => c.UniqueId).ToHashSet();
        _prevPlayer2Agents = aiPlayer.Agents.Select(agent => agent.RepresentingCard.UniqueId).ToHashSet();

        _prevTavernAvailable = state.TavernAvailableCards.Select(c => c.UniqueId).ToHashSet();
        _prevTavernPile = state.TavernCards.Select(c => c.UniqueId).ToHashSet();

        //Debug.Log("[Snapshot] Initial snapshot set.");
    }

    public void CompareAndQueueChanges(FullGameState newState)
    {
        var humanPlayer = newState.GetPlayer(GameManager.Instance.HumanPlayer);
        var aiPlayer = newState.GetPlayer(GameManager.Instance.AIPlayer);

        CompareAndQueue(_prevPlayer1Agents, humanPlayer.Agents.Select(agent => agent.RepresentingCard).ToList(), ZoneType.Agents, ZoneSide.Player1, _prevPlayer1Agents);
        CompareAndQueue(_prevPlayer1Draw, humanPlayer.DrawPile, ZoneType.DrawPile, ZoneSide.Player1, _prevPlayer1Draw);
        CompareAndQueue(_prevPlayer1Cooldown, humanPlayer.CooldownPile, ZoneType.CooldownPile, ZoneSide.Player1, _prevPlayer1Cooldown);
        CompareAndQueue(_prevPlayer1Played, humanPlayer.Played, ZoneType.PlayedPile, ZoneSide.Player1, _prevPlayer1Played);
        CompareAndQueue(_prevPlayer1Hand, humanPlayer.Hand, ZoneType.Hand, ZoneSide.Player1, _prevPlayer1Hand);

        CompareAndQueue(_prevPlayer2Agents, aiPlayer.Agents.Select(agent => agent.RepresentingCard).ToList(), ZoneType.Agents, ZoneSide.Player2, _prevPlayer2Agents);
        CompareAndQueue(_prevPlayer2Draw, aiPlayer.DrawPile, ZoneType.DrawPile, ZoneSide.Player2, _prevPlayer2Draw);
        CompareAndQueue(_prevPlayer2Cooldown, aiPlayer.CooldownPile, ZoneType.CooldownPile, ZoneSide.Player2, _prevPlayer2Cooldown);
        CompareAndQueue(_prevPlayer2Played, aiPlayer.Played, ZoneType.PlayedPile, ZoneSide.Player2, _prevPlayer2Played);
        CompareAndQueue(_prevPlayer2Hand, aiPlayer.Hand, ZoneType.Hand, ZoneSide.Player2, _prevPlayer2Hand);

        CompareAndQueue(_prevTavernPile, newState.TavernCards, ZoneType.Tavern, ZoneSide.Neutral, _prevTavernPile);
        CompareAndQueue(_prevTavernAvailable, newState.TavernAvailableCards, ZoneType.TavernAvailable, ZoneSide.Neutral, _prevTavernAvailable);

        //Debug.Log("[Snapshot] Compared and queued changes.");
    }

    private void CompareAndQueue(HashSet<UniqueId> previousSet, List<UniqueCard> currentList, ZoneType zoneType, ZoneSide zoneSide, HashSet<UniqueId> storage)
    {
        var currentIds = currentList.Select(c => c.UniqueId).ToHashSet();
        var added = currentIds.Except(previousSet);

        foreach (var id in added)
        {
            //Debug.Log($"[VisualQueue] Card {id.Value} added to {zoneType} ({zoneSide})");
            _visualQueue.Enqueue(new MoveVisualCommand(id, zoneType, zoneSide));
        }

        storage.Clear();
        foreach (var id in currentIds)
            storage.Add(id);
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
                if (cmd.Zone == ZoneType.TavernAvailable)
                {
                    //Debug.Log($"[Animate] Moving card {cmd.CardId.Value} to {cmd.Zone} ({cmd.Side})");
                    StartCoroutine(BoardManager.Instance.AnimateAddCardToTavernDelayed(cmd.CardId));
                }
                else if (cmd.Zone == ZoneType.Hand)
                {
                    Transform target = BoardManager.Instance.GetZoneTransform(cmd.Zone, cmd.Side);
                    BoardManager.Instance.MoveCardToZone(cmd.CardId, target, cmd.Zone, cmd.Side, () => BoardManager.Instance.ArrangeCardsInHand(target));
                }
                else if (cmd.Zone == ZoneType.Agents)
                {
                    Transform target = BoardManager.Instance.GetZoneTransform(cmd.Zone, cmd.Side);
                    BoardManager.Instance.MoveCardToZone(cmd.CardId, target, cmd.Zone, cmd.Side, () => BoardManager.Instance.ArrangeAgentsInZone(target));
                }
                else
                {
                    Transform target = BoardManager.Instance.GetZoneTransform(cmd.Zone, cmd.Side);
                    BoardManager.Instance.MoveCardToZone(cmd.CardId, target, cmd.Zone, cmd.Side);
                }
                yield return new WaitForSeconds(0.15f);
            }
            else
            {
                yield return null;
            }
        }
    }
}

public class MoveVisualCommand
{
    public UniqueId CardId;
    public ZoneType Zone;
    public ZoneSide Side;

    public MoveVisualCommand(UniqueId cardId, ZoneType zone, ZoneSide side)
    {
        CardId = cardId;
        Zone = zone;
        Side = side;
    }
}
