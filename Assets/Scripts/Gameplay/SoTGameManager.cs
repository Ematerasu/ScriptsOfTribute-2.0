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
        RefreshMoveList();
    }

    public void RefreshMoveList()
    {
        _cachedLegalMoves = _api.GetListOfPossibleMoves();
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

    public SerializedChoice? GetPendingChoice()
    {
        return _api.PendingChoice;
    }

    public Choice.DataType PendingChoiceType => _api.PendingChoice.Type;

    public EndGameState? PlayCard(UniqueCard card)
    {
        var endGame = _api.PlayCard(card);
        RefreshMoveList();            
        var newState = _api.GetFullGameState();
        BoardManager.Instance.UpdateBoard(newState, UpdateReason.PLAY_CARD);
        completedActionProcessor.CompareAndQueueChanges(newState);
        return endGame;
    }

    public EndGameState? BuyCard(UniqueCard card)
    {
        var endGame = _api.BuyCard(card);
        RefreshMoveList();
        var newState = _api.GetFullGameState();
        BoardManager.Instance.UpdateBoard(newState, UpdateReason.BUY_CARD);
        completedActionProcessor.CompareAndQueueChanges(newState);
        return endGame;
    }

    public EndGameState? ActivateAgent(UniqueCard agent)
    {
        var endGame = _api.ActivateAgent(agent);
        RefreshMoveList();
        var newState = _api.GetFullGameState();
        BoardManager.Instance.UpdateBoard(newState, UpdateReason.AGENT_ACTIVATION);
        completedActionProcessor.CompareAndQueueChanges(newState);
        return endGame;
    }

    public EndGameState? EndTurn(ZoneSide side)
    {
        var endGame = _api.EndTurn();
        RefreshMoveList();
        var newState = _api.GetFullGameState();
        BoardManager.Instance.UpdateBoard(newState, UpdateReason.END_TURN);
        completedActionProcessor.CompareAndQueueChanges(newState);
        return endGame;
    }

    public EndGameState? ActivatePatron(PatronId patron)
    {
        var endGame = _api.PatronActivation(patron);
        RefreshMoveList();
        var newState = _api.GetFullGameState();
        BoardManager.Instance.UpdateBoard(newState, UpdateReason.PATRON_ACTIVATION);
        completedActionProcessor.CompareAndQueueChanges(newState);
        return endGame;
    }

    public EndGameState? MakeChoice(List<UniqueCard> selectedCards)
    {
        var endGame = _api.MakeChoice(selectedCards);
        RefreshMoveList();
        var newState = _api.GetFullGameState();
        BoardManager.Instance.UpdateBoard(newState, UpdateReason.CHOICE_MADE);
        completedActionProcessor.CompareAndQueueChanges(newState);
        return endGame;
    }

    public EndGameState? MakeChoice(UniqueEffect selectedEffect)
    {
        var endGame = _api.MakeChoice(selectedEffect);
        RefreshMoveList();
        var newState = _api.GetFullGameState();
        BoardManager.Instance.UpdateBoard(newState, UpdateReason.CHOICE_MADE);
        completedActionProcessor.CompareAndQueueChanges(newState);
        return endGame;
    }

    public EndGameState? AttackAgent(UniqueCard agent)
    {
        var endGame = _api.AttackAgent(agent);
        RefreshMoveList();
        var newState = _api.GetFullGameState();
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