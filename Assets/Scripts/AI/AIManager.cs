using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptsOfTribute;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Serializers;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    [SerializeField] private CompletedActionProcessor completedActionProcessor;

    private AI _bot;
    private PlayerEnum _aiPlayer;

    private SoTGameManager _gameManager;
    public bool BotIsPlaying { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void InitializeBot(AI bot, PlayerEnum botId, SoTGameManager gameManager)
    {
        _bot = bot;
        _aiPlayer = botId;
        _bot.Id = botId;
        _gameManager = gameManager;

        try
        {
            _bot.PregamePrepare();
        }
        catch (Exception e)
        {
            Debug.LogError($"AI preparation failed: {e.Message}");
        }
    }

    public bool IsAITurn()
    {
        return _gameManager.CurrentPlayer == _aiPlayer;
    }

    public void PlaySingleMove()
    {
        BotIsPlaying = true;
        StartCoroutine(PlaySingleMoveCoroutine());
    }

    public void PlayFullTurn()
    {
        BotIsPlaying = true;
        StartCoroutine(PlayFullTurnCoroutine());
    }

    public IEnumerator PregamePrepareCoroutine()
    {
        bool finished = false;

        System.Threading.Thread t = new System.Threading.Thread(() =>
        {
            try
            {
                _bot.PregamePrepare();
            }
            catch (Exception e)
            {
                Debug.LogError($"AI.PregamePrepare crash: {e.Message}");
            }
            finished = true;
        });
        t.Start();

        while (!finished)
        {
            yield return null;
        }
    }

    public IEnumerator SelectPatronCoroutine(List<PatronId> availablePatrons, int round, Action<PatronId> onResult)
    {
        PatronId chosen = PatronId.TREASURY; 
        bool finished = false;

        System.Threading.Thread t = new System.Threading.Thread(() =>
        {
            try
            {
                chosen = _bot.SelectPatron(availablePatrons, round);
            }
            catch (Exception e)
            {
                Debug.LogError($"AI.SelectPatron crash: {e.Message}");
            }
            finished = true;
        });
        t.Start();

        while (!finished)
        {
            yield return null;
        }

        onResult?.Invoke(chosen);
    }

    private IEnumerator PlaySingleMoveCoroutine()
    {
        if (!IsAITurn()) yield break;
        var state = new GameState(_gameManager.GetCurrentGameState());
        var possibleMoves = _gameManager.LegalMoves;

        yield return StartCoroutine(GetMoveFromBotCoroutine(
            state,
            possibleMoves,
            move =>
            {
                if (move == null || !_gameManager.IsMoveLegal(move))
                {
                    Debug.LogWarning($"AI returned illegal or null move: {move}");
                    return;
                }
                GameManager.Instance.ExecuteMove(move, _aiPlayer);
                BotIsPlaying = false;
            }
        ));
    }

    private IEnumerator PlayFullTurnCoroutine()
    {
        if (!IsAITurn()) yield break;
        bool endTurn = false;
        while (!endTurn)
        {
            var state = new GameState(_gameManager.GetCurrentGameState());
            var possibleMoves = _gameManager.LegalMoves;

            bool moveFinished = false;

            yield return StartCoroutine(GetMoveFromBotCoroutine(
                state,
                possibleMoves,
                move =>
                {
                    if (move == null || !_gameManager.IsMoveLegal(move))
                    {
                        Debug.LogWarning($"AI returned illegal or null move: {move}");
                    }
                    else if (move.Command == CommandEnum.END_TURN)
                    {
                        endTurn = true;
                    }
                    else
                    {
                        GameManager.Instance.ExecuteMove(move, _aiPlayer);
                    }
                    moveFinished = true;
                }
            ));

            float t = 0f;
            while (!moveFinished && t < 30f)
            {
                Debug.Log("Waiting for bot to move");
                t += Time.deltaTime;
                yield return null;
            }

            while(completedActionProcessor.IsBusy)
            {
                Debug.Log("Kolejka wisi");
                Debug.Log($"W kolejce jest {completedActionProcessor.ElementsInQueue} elementów");
                yield return null;
            }
            //yield return new WaitUntil(() => );
        }
        yield return new WaitForSeconds(0.3f);
        BotIsPlaying = false;
        GameManager.Instance.HandleEndTurn(ZoneSide.Player2);
    }

    private IEnumerator GetMoveFromBotCoroutine(GameState state, List<Move> legalMoves, Action<Move> onResult)
    {
        Move move = null;
        bool finished = false;
        System.Threading.Thread t = new System.Threading.Thread(() =>
        {
            try
            {
                move = _bot.Play(state, legalMoves, TimeSpan.FromSeconds(30));
            }
            catch (Exception e)
            {
                Debug.LogError($"AI.Play crash: {e.Message}");
            }
            finished = true;
        });
        t.Start();

        while (!finished)
        {
            yield return null;
        }
        onResult?.Invoke(move);
    }

    public IEnumerator GameEndCoroutine(EndGameState result, FullGameState? finalBoardState)
    {
        bool finished = false;

        System.Threading.Thread t = new System.Threading.Thread(() =>
        {
            try
            {
                _bot.GameEnd(result, finalBoardState);
            }
            catch (Exception e)
            {
                Debug.LogError($"AI.GameEnd crash: {e.Message}");
            }
            finished = true;
        });
        t.Start();

        while (!finished)
        {
            yield return null;
        }
    }
}
