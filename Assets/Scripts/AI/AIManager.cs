using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptsOfTribute;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Serializers;
using Bots;
using UnityBots;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    [SerializeField] private CompletedActionProcessor completedActionProcessor;

    private AI _bot;
    private PlayerEnum _aiPlayer;

    private SoTGameManager _gameManager;
    public bool BotIsPlaying { get; private set; }

    public bool BotInitialized { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void InitializeBot(Type botType, PlayerEnum botId)
    {
        _bot = (AI)Activator.CreateInstance(botType);
        _aiPlayer = botId;
        _bot.Id = botId;
        BotInitialized = true;
        try
        {
            _bot.PregamePrepare();
        }
        catch (Exception e)
        {
            Debug.LogError($"AI preparation failed: {e.Message}");
        }
    }

    public void InitializeManager(SoTGameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public bool IsAITurn()
    {
        return _gameManager.CurrentPlayer == _aiPlayer;
    }

    public void PickOnePatron(List<PatronId> availablePatrons, int round, Action<PatronId> onResult)
    {
        BotIsPlaying = true;
        StartCoroutine(SelectPatronCoroutine(availablePatrons, round, onResult));
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
        BotIsPlaying = false;
    }

    private IEnumerator PlaySingleMoveCoroutine()
    {
        if (!IsAITurn()) yield break;
        var fullGameState = _gameManager.GetCurrentGameState();
        if (_bot is GrpcBotAI grpcBot)
        {
            grpcBot.SetFullGameState(fullGameState);
        }
        var state = new GameState(fullGameState);
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
            var fullGameState = _gameManager.GetCurrentGameState();
            if (_bot is GrpcBotAI grpcBot)
            {
                grpcBot.SetFullGameState(fullGameState);
            }
            var state = new GameState(fullGameState);
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
                // Debug.Log("Kolejka wisi");
                // Debug.Log($"W kolejce jest {completedActionProcessor.ElementsInQueue} elementów");
                yield return null;
            }
            yield return new WaitForSeconds(0.3f);
        }
        yield return new WaitForSeconds(0.3f);
        BotIsPlaying = false;
        GameManager.Instance.HandleEndTurn(ZoneSide.EnemyPlayer);
    }

    private IEnumerator GetMoveFromBotCoroutine(GameState state, List<Move> legalMoves, Action<Move> onResult)
    {
        Move move = null;
        bool finished = false;
        Exception botException = null;
        System.Threading.Thread t = new System.Threading.Thread(() =>
        {
            try
            {
                move = _bot.Play(state, legalMoves, TimeSpan.FromSeconds(30));
                Debug.Log($"Ai.Play {move}");
            }
            catch (Exception e)
            {
                botException = e;
                Debug.LogError($"AI.Play crash: {e.Message}");
            }
            finally
            {
                finished = true;
            }
        });
        t.Start();

        float timeout = 30f;
        float elapsed = 0f;

        while (!finished && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!finished)
        {
            Debug.LogWarning("AI bot przekroczył limit czasu – kończę grę jako timeout.");
            EndGameState timeoutState = new EndGameState(GameManager.Instance.HumanPlayer, GameEndReason.TURN_TIMEOUT, "Bot timed out");
            GameManager.Instance.HandleEndGame(timeoutState, null);
            StopAllCoroutines();
            yield break;
        }

        if (botException != null)
        {
            EndGameState endGameState = new EndGameState(GameManager.Instance.HumanPlayer, GameEndReason.BOT_EXCEPTION, botException.Message);
            GameManager.Instance.HandleEndGame(endGameState, move);
            StopAllCoroutines();
        }
        onResult?.Invoke(move);
    }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public IEnumerator GameEndCoroutine(EndGameState result, FullGameState? finalBoardState)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
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

    public AI CreateBotByType(BotType botType)
    {
        switch (botType)
        {
            case BotType.MaxPrestige:
                return new MaxPrestigeBot();
            case BotType.Random:
                return new RandomBot();
            case BotType.MCTS:
                return new MCTSBot();
            case BotType.BeamSearch:
                return new BeamSearchBot();
            case BotType.DecisionTree:
                return new DecisionTreeBot();
            case BotType.Akame:
                return new Akame();
            case BotType.GrpcBot:
                return new GrpcBotAI();
            case BotType.BestMCTS3:
                return new BestMCTS3();
            case BotType.SOISMCTS:
                return new SOISMCTS.SOISMCTS();
            default:
                Debug.LogWarning($"Unknown BotType {botType}. Falling back to MaxPrestigeBot.");
                return new MaxPrestigeBot();
        }
    }

    public List<(DateTime, string)> GetBotLogs() => _bot.LogMessages;

    private void OnApplicationQuit()
    {
        TrySendGameEndIfGrpcBot();
    }

    private void OnDisable()
    {
    #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            TrySendGameEndIfGrpcBot();
        }
    #endif
    }

    private void TrySendGameEndIfGrpcBot()
    {
        if (_bot is GrpcBotAI grpcBot)
        {
            Debug.Log("[AIManager] Sending GameEndRequest before shutdown.");

            grpcBot.GameEnd(
                new EndGameState(PlayerEnum.PLAYER1, GameEndReason.INTERNAL_ERROR, "Application quit"),
                null
            );
        }
    }

}
