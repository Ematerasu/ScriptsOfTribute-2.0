using UnityEngine;
using ScriptsOfTribute;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.Cards;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using ScriptsOfTribute.Serializers;

public class SoTGameManager : MonoBehaviour
{
    private IScriptsOfTributeApi _api;
    private List<Move> _cachedLegalMoves = new();
    private PatronStates _patronStates;

    public PatronStates PatronStates => _patronStates;
    public List<Move> LegalMoves => _cachedLegalMoves;

    public PlayerEnum CurrentPlayer => _api.CurrentPlayerId;

    [SerializeField] private CompletedActionProcessor completedActionProcessor;

    public void InitializeGame(PatronId[] patrons, ulong seed = 0)
    {
        if (seed == 0)
        {
            seed = (ulong)Random.Range(int.MinValue, int.MaxValue);
        }
        _api = new ScriptsOfTributeApi(patrons, seed);
        FullGameState initialState = _api.GetFullGameState();
        BoardManager.Instance.InitializeBoard(initialState);
        completedActionProcessor.SetInitialSnapshot(initialState);
        RefreshCache(initialState);
    }

    public void InitializeDebugGame(FullGameState debugState)
    {
        _api = ScriptsOfTributeApi.FromSerializedBoard(debugState);
        BoardManager.Instance.InitializeBoard(debugState);
        completedActionProcessor.SetInitialSnapshot(debugState);
        RefreshCache(debugState);
    }

    public void RefreshCache(FullGameState newState)
    {
        _cachedLegalMoves = _api.GetListOfPossibleMoves();
        _patronStates = newState.PatronStates;
    }

    public FullGameState GetCurrentGameState()
    {
        return _api.GetFullGameState();
    }

    public bool IsMoveLegal(Move move)
    {
        return _cachedLegalMoves.Contains(move);
    }

    public bool IsChoicePending()
    {
        return _api.PendingChoice is not null;
    }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public SerializedChoice? GetPendingChoice()
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    {
        return _api.PendingChoice;
    }

    public Choice.DataType PendingChoiceType => _api.PendingChoice.Type;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public EndGameState? PlayCard(UniqueCard card)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    {
        var endGame = _api.PlayCard(card);
        var newState = _api.GetFullGameState();
        RefreshCache(newState);            
        BoardManager.Instance.UpdateBoard(newState, UpdateReason.PLAY_CARD);
        completedActionProcessor.CompareAndQueueChanges(newState);
        return endGame;
    }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public EndGameState? BuyCard(UniqueCard card)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    {
        var endGame = _api.BuyCard(card);
        var newState = _api.GetFullGameState();
        RefreshCache(newState);
        BoardManager.Instance.UpdateBoard(newState, UpdateReason.BUY_CARD);
        completedActionProcessor.CompareAndQueueChanges(newState);
        AudioManager.Instance.PlayCardBuySfx();
        return endGame;
    }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public EndGameState? ActivateAgent(UniqueCard agent)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    {
        var endGame = _api.ActivateAgent(agent);
        var newState = _api.GetFullGameState();
        RefreshCache(newState);
        BoardManager.Instance.UpdateBoard(newState, UpdateReason.AGENT_ACTIVATION);
        completedActionProcessor.CompareAndQueueChanges(newState);
        return endGame;
    }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public EndGameState? EndTurn(ZoneSide side)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    {
        var endGame = _api.EndTurn();
        var newState = _api.GetFullGameState();
        RefreshCache(newState);
        BoardManager.Instance.UpdateBoard(newState, UpdateReason.END_TURN);
        completedActionProcessor.CompareAndQueueChanges(newState);
        return endGame;
    }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public EndGameState? ActivatePatron(PatronId patron)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    {
        var endGame = _api.PatronActivation(patron);
        var newState = _api.GetFullGameState();
        RefreshCache(newState);
        BoardManager.Instance.UpdateBoard(newState, UpdateReason.PATRON_ACTIVATION);
        completedActionProcessor.CompareAndQueueChanges(newState);
        return endGame;
    }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public EndGameState? MakeChoice(List<UniqueCard> selectedCards)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    {
        var endGame = _api.MakeChoice(selectedCards);
        var newState = _api.GetFullGameState();
        RefreshCache(newState);
        BoardManager.Instance.UpdateBoard(newState, UpdateReason.CHOICE_MADE);
        completedActionProcessor.CompareAndQueueChanges(newState);
        return endGame;
    }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public EndGameState? MakeChoice(UniqueEffect selectedEffect)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    {
        var endGame = _api.MakeChoice(selectedEffect);
        var newState = _api.GetFullGameState();
        RefreshCache(newState);
        BoardManager.Instance.UpdateBoard(newState, UpdateReason.CHOICE_MADE);
        completedActionProcessor.CompareAndQueueChanges(newState);
        return endGame;
    }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public EndGameState? AttackAgent(UniqueCard agent)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    {
        var endGame = _api.AttackAgent(agent);
        var newState = _api.GetFullGameState();
        RefreshCache(newState);
        BoardManager.Instance.UpdateBoard(newState, UpdateReason.AGENT_ATTACK);
        completedActionProcessor.CompareAndQueueChanges(newState);
        return endGame;
    }

    public void DebugPrintHands(FullGameState state)
    {
        Debug.Log("=== Player 1 Hand ===");
        foreach (var card in state.CurrentPlayer.Hand)
        {
            Debug.Log($"[P1] {card.Name} | CommonId: {card.CommonId} | UniqueId: {card.UniqueId.Value}");
        }

        Debug.Log("=== Player 2 Hand ===");
        foreach (var card in state.EnemyPlayer.Hand)
        {
            Debug.Log($"[P2] {card.Name} | CommonId: {card.CommonId} | UniqueId: {card.UniqueId.Value}");
        }
    }
}