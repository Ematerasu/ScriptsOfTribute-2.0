using UnityEngine;
using ScriptsOfTribute;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Board;
using System.Collections;
using Bots;
using ScriptsOfTribute.AI;
using System.Collections.Generic;
using System.Linq;
using ScriptsOfTribute.Serializers;
using System;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private SoTGameManager _soTGameManager; 
    [SerializeField] private UIManager _uiManager;
    // [SerializeField] private AudioManager _audioManager;
    [SerializeField] private AIManager _aiManager;
    [SerializeField] private PatronManager _patronManager;

    [SerializeField] private PlayerEnum CurrentTurn = PlayerEnum.NO_PLAYER_SELECTED;
    [SerializeField] public PlayerEnum HumanPlayer = PlayerEnum.PLAYER1;
    [SerializeField] public PlayerEnum AIPlayer => HumanPlayer == PlayerEnum.PLAYER1 ? PlayerEnum.PLAYER2 : PlayerEnum.PLAYER1;

    [Header("Debug mode")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool debugPlayerStartsFirst = true;

    [SerializeField] private List<CardId> debugPlayer1Hand = new();
    [SerializeField] private List<CardId> debugPlayer2Hand = new();
    [SerializeField] private List<CardId> debugPlayer1DrawPile = new();
    [SerializeField] private List<CardId> debugPlayer2DrawPile = new();
    [SerializeField] private List<CardId> debugTavernCards = new();
    [SerializeField] private List<PatronId> debugPatrons = new();

    [SerializeField] private int debugPlayer1Prestige = 0;
    [SerializeField] private int debugPlayer2Prestige = 0;
    [SerializeField] private int debugPlayer1Power = 0;
    [SerializeField] private int debugPlayer2Power = 0;
    [SerializeField] private int debugPlayer1Coins = 0;
    [SerializeField] private int debugPlayer2Coins = 0;

    public bool IsDebugMode => debugMode;

    public bool IsHumanPlayersTurn => CurrentTurn == HumanPlayer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (debugMode)
        {
            InitializeDebugGame();
        }
        else
        {
            NormalStart();
        }
    }
    private void NormalStart()
    {
        AI bot = new MaxPrestigeBot();
        _aiManager.InitializeBot(bot, AIPlayer);
        CurrentTurn = PlayerEnum.PLAYER1;
        List<PatronId> patronsAvailable = new List<PatronId>()
        { 
            PatronId.ANSEI,
            PatronId.DUKE_OF_CROWS,
            PatronId.RAJHIN, 
            //PatronId.PSIJIC, 
            PatronId.ORGNUM, 
            PatronId.HLAALU, 
            PatronId.PELIN, 
            PatronId.RED_EAGLE,
            PatronId.SAINT_ALESSIA,
        };
        _uiManager.ShowPatronDraft(patronsAvailable);
    }

    public void StartGameWithPatrons(PatronId[] patrons)
    {
        _soTGameManager.InitializeGame(patrons);
        _patronManager.InitializePatrons(patrons);
        CurrentTurn = _soTGameManager.CurrentPlayer;

        _aiManager.InitializeManager(_soTGameManager);

        if (CurrentTurn == HumanPlayer)
            StartCoroutine(_uiManager.ShowYourTurnMessage());
        
        _uiManager.ShowAiButtons(CurrentTurn == AIPlayer);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log(_soTGameManager.GetCurrentGameState().ToString());
            Debug.Log("=============");
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            var state = _soTGameManager.GetCurrentGameState();
            BoardManager.Instance.DebugCheckHandSync(state);
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Debug.Log(string.Join("\n", _soTGameManager.GetCurrentGameState().CompletedActions.Select(action => action.ToString()).ToList()));
            Debug.Log("=============");
        }
    }

    public void OnEndTurnButtonClicked()
    {
        HandleEndTurn(ZoneSide.Player1);
    }

    public void PlaySingleAIMove()
    {
        if (CurrentTurn == AIPlayer)
        {
            _aiManager.PlaySingleMove();
        }
    }

    public void PlayAiTurn()
    {
        if (CurrentTurn == AIPlayer)
        {
            _aiManager.PlayFullTurn();
        }
    }

    public void HandleEndTurn(ZoneSide sideCalling)
    {
        var endGameState = _soTGameManager.EndTurn(sideCalling);
        if (endGameState != null)
        {
            _uiManager.ShowAiButtons(false);
            _uiManager.HandleEndGame(endGameState, _soTGameManager.GetCurrentGameState());
            return;
        }
        CurrentTurn = _soTGameManager.CurrentPlayer;
        _uiManager.ShowAiButtons(CurrentTurn == AIPlayer);

        if (IsHumanPlayersTurn)
        {
            StartCoroutine(ShowStartOfHumanTurnRoutine());
        }
    }

    public void ExecuteMove(Move move, PlayerEnum player)
    {
        ZoneSide side = player == PlayerEnum.PLAYER1 ? ZoneSide.Player1 : ZoneSide.Player2;
        switch(move.Command)
        {
            case CommandEnum.END_TURN:
            {
                HandleEndTurn(side);
                break;
            }
            case CommandEnum.PLAY_CARD:
            {
                if (move is SimpleCardMove cardMove)
                    PlayCard(cardMove.Card, side);
                break;
            }
            case CommandEnum.BUY_CARD:
            {
                if (move is SimpleCardMove cardMove)
                    BuyCard(cardMove.Card, side);
                break;
            }
            case CommandEnum.CALL_PATRON:
                if (move is SimplePatronMove patronMove)
                    CallPatron(patronMove.PatronId, side);
                break;
            case CommandEnum.MAKE_CHOICE:
                if (move is MakeChoiceMoveUniqueCard cardChoiceMove)
                    MakeChoice(cardChoiceMove.Choices);
                else if (move is MakeChoiceMoveUniqueEffect effectChoiceMove)
                    MakeChoice(effectChoiceMove.Choices[0]);
                break;
            case CommandEnum.ACTIVATE_AGENT:
                if (move is SimpleCardMove agentMove)
                    ActivateAgent(agentMove.Card, side);
                break;
            case CommandEnum.ATTACK:
                if (move is SimpleCardMove attackAgentMove)
                    AttackAgent(attackAgentMove.Card, side);
                break;
            default:
            {
                Debug.Log($"Unhandled move: {move}");
                break;
            }
        }
    }

    public void PlayCard(UniqueCard card, ZoneSide side)
    {
        Move move = Move.PlayCard(card);
        if (!_soTGameManager.IsMoveLegal(move))
        {
            Debug.LogWarning($"Move {move} is not legal");
            return;
        }
        var result = _soTGameManager.PlayCard(card);
        if (result is EndGameState end)
        {
            HandleEndGame(end, move);
        }

        if (IsHumanPlayersTurn && _soTGameManager.IsChoicePending())
        {
            var choice = _soTGameManager.GetPendingChoice();
            if (choice != null)
            {
                _uiManager.ShowChoice(choice);
            }
        }
    }
    public void BuyCard(UniqueCard card, ZoneSide side)
    {
        Move move = Move.BuyCard(card);
        if (!_soTGameManager.IsMoveLegal(move))
        {
            Debug.LogWarning($"Move {move} is not legal");
            return;
        }
        var result = _soTGameManager.BuyCard(card);
        if (result is EndGameState end)
        {
            HandleEndGame(end, move);
        }
        if (IsHumanPlayersTurn && _soTGameManager.IsChoicePending())
        {
            var choice = _soTGameManager.GetPendingChoice();
            if (choice != null)
            {   
                _uiManager.ShowChoice(choice);
            }
        }
    }

    public void ActivateAgent(UniqueCard card, ZoneSide side)
    {
        Move move = Move.ActivateAgent(card);
        if (!_soTGameManager.IsMoveLegal(move))
        {
            Debug.LogWarning($"Move {move} is not legal");
            return;
        }

        var result = _soTGameManager.ActivateAgent(card);
        if (result is EndGameState end)
        {
            HandleEndGame(end, move);
        }

        if (IsHumanPlayersTurn && _soTGameManager.IsChoicePending())
        {
            var choice = _soTGameManager.GetPendingChoice();
            if (choice != null)
            {
                _uiManager.ShowChoice(choice);
            }
        }
    }

    public void AttackAgent(UniqueCard card, ZoneSide side)
    {
        Move move = Move.Attack(card);
        if (!_soTGameManager.IsMoveLegal(move))
        {
            Debug.LogWarning($"Move {move} is not legal");
            return;
        }

        var result = _soTGameManager.AttackAgent(card);
        BoardManager.Instance.PlayPowerAttackEffect(card.UniqueId, side);
        if (result is EndGameState end)
        {
            HandleEndGame(end, move);
        }

        if (IsHumanPlayersTurn && _soTGameManager.IsChoicePending())
        {
            var choice = _soTGameManager.GetPendingChoice();
            if (choice != null)
            {
                _uiManager.ShowChoice(choice);
            }
        }
    }
    public void CallPatron(PatronId patron, ZoneSide side)
    {
        Move move = Move.CallPatron(patron);
        if (!_soTGameManager.IsMoveLegal(move))
        {
            Debug.LogWarning($"Move {move} is not legal");
            return;
        }
        var result = _soTGameManager.ActivatePatron(patron);
        if (result is EndGameState end)
        {
            HandleEndGame(end, move);
        }
        _patronManager.UpdateObjects(_soTGameManager.PatronStates);
        if (IsHumanPlayersTurn && _soTGameManager.IsChoicePending())
        {
            var choice = _soTGameManager.GetPendingChoice();
            if (choice != null)
            {
                _uiManager.ShowChoice(choice);
            }
        }
    }

    public void MakeChoice(List<UniqueCard> selectedCards)
    {
        if (!_soTGameManager.IsChoicePending())
        {
            Debug.LogWarning($"Theres no choice to be made");
            return;
        }

        if (_soTGameManager.PendingChoiceType != Choice.DataType.CARD)
        {
            Debug.LogWarning($"Effect choice is pending and we try to make card choice");
            return;
        }
        var followUp = _soTGameManager.GetPendingChoice().ChoiceFollowUp;
        if (followUp == ChoiceFollowUp.DESTROY_CARDS || followUp == ChoiceFollowUp.COMPLETE_TREASURY || followUp == ChoiceFollowUp.COMPLETE_HLAALU)
        {
            BoardManager.Instance.DestroyCards(selectedCards);
        }
        var result = _soTGameManager.MakeChoice(selectedCards);
        if (result is EndGameState end)
        {
            HandleEndGame(end, Move.MakeChoice(selectedCards));
        }
        
        if (IsHumanPlayersTurn && _soTGameManager.IsChoicePending())
        {
            var choice = _soTGameManager.GetPendingChoice();
            if (choice != null)
            {
                _uiManager.ShowChoice(choice);
            }
        }
    }

    public void MakeChoice(UniqueEffect selectedEffect)
    {
        if (!_soTGameManager.IsChoicePending())
        {
            Debug.LogWarning($"Theres no choice to be made");
            return;
        }

        if (_soTGameManager.PendingChoiceType != Choice.DataType.EFFECT)
        {
            Debug.LogWarning($"Card choice is pending and we try to make effect choice");
            return;
        }

        var result = _soTGameManager.MakeChoice(selectedEffect);
        if (result is EndGameState end)
        {
            HandleEndGame(end, Move.MakeChoice(selectedEffect));
        }
        if (IsHumanPlayersTurn && _soTGameManager.IsChoicePending())
        {
            var choice = _soTGameManager.GetPendingChoice();
            if (choice != null)
            {
                _uiManager.ShowChoice(choice);
            }
        }
    }

    private void HandleEndGame(EndGameState end, Move triggeringMove)
    {
        if (end.Reason == GameEndReason.INCORRECT_MOVE)
        {
            Debug.LogError($"Engine refused {triggeringMove}, even though we thought its legal!");
        }

        _uiManager.HandleEndGame(end, _soTGameManager.GetCurrentGameState());
    }

    private IEnumerator ShowStartOfHumanTurnRoutine()
    {
        yield return StartCoroutine(_uiManager.ShowYourTurnMessage());

        if (_soTGameManager.IsChoicePending())
        {
            var choice = _soTGameManager.GetPendingChoice();
            if (choice != null)
            {
                _uiManager.ShowChoice(choice);
            }
        }
    }

// DEBUG
    public void InitializeDebugGame()
    {
        if (debugPatrons.Count != 4)
        {
            Debug.LogError("You must select exactly 4 patrons (excluding Treasury)!");
            return;
        }
        List<PatronId> patronsWithTreasury = new List<PatronId>(debugPatrons);
        patronsWithTreasury.Insert(2, PatronId.TREASURY);
        var patrons = patronsWithTreasury.Select(pid => Patron.FromId(pid)).ToList();
        var patronStates = new PatronStates(patrons);

        var availableCards = GlobalCardDatabase.Instance.GetCardsByPatron(patronsWithTreasury.ToArray(), Array.Empty<CardId>());
        
        var player1Hand = debugPlayer1Hand.Select(id => FindAndRemoveCard(id, availableCards)).ToList();
        var player2Hand = debugPlayer2Hand.Select(id => FindAndRemoveCard(id, availableCards)).ToList();
        var player1DrawPile = debugPlayer1DrawPile.Select(id => FindAndRemoveCard(id, availableCards)).ToList();
        var player2DrawPile = debugPlayer2DrawPile.Select(id => FindAndRemoveCard(id, availableCards)).ToList();
        var tavernAvailable = debugTavernCards.Select(id => FindAndRemoveCard(id, availableCards)).ToList();

        if (tavernAvailable.Count != 5)
        {
            Debug.LogError("Tavern must have exactly 5 cards selected!");
            return;
        }

        var tavernPile = availableCards;

        var player1 = new SerializedPlayer(
            PlayerEnum.PLAYER1,
            hand: player1Hand,
            drawPile: player1DrawPile,
            cooldownPile: new List<UniqueCard>(),
            played: new List<UniqueCard>(),
            agents: new List<SerializedAgent>(),
            power: debugPlayer1Power,
            patronCalls: 1,
            coins: debugPlayer1Coins,
            prestige: debugPlayer1Prestige
        );

        var player2 = new SerializedPlayer(
            PlayerEnum.PLAYER2,
            hand: player2Hand,
            drawPile: player2DrawPile,
            cooldownPile: new List<UniqueCard>(),
            played: new List<UniqueCard>(),
            agents: new List<SerializedAgent>(),
            power: debugPlayer2Power,
            patronCalls: 1,
            coins: debugPlayer2Coins,
            prestige: debugPlayer2Prestige
        );

        var debugState = new FullGameState(
            currentPlayer: debugPlayerStartsFirst ? player1 : player2,
            enemyPlayer: debugPlayerStartsFirst ? player2 : player1,
            patronStates: patronStates,
            tavernAvailableCards: tavernAvailable,
            tavernCards: tavernPile,
            currentSeed: (ulong)UnityEngine.Random.Range(0, int.MaxValue),
            cheats: true
        );

        _soTGameManager.InitializeDebugGame(debugState);
        _patronManager.InitializePatrons(patronsWithTreasury.ToArray());
        CurrentTurn = debugState.CurrentPlayer.PlayerID;
        HumanPlayer = PlayerEnum.PLAYER1;

        AI bot = new MCTSBot();
        _aiManager.InitializeBot(bot, AIPlayer);
        _aiManager.InitializeManager(_soTGameManager);

        if (CurrentTurn == HumanPlayer)
            StartCoroutine(_uiManager.ShowYourTurnMessage());
        _uiManager.ShowAiButtons(CurrentTurn == AIPlayer);
    }

    private UniqueCard FindAndRemoveCard(CardId id, List<UniqueCard> availableCards)
    {
        var card = availableCards.FirstOrDefault(c => c.CommonId == id);
        if (card == null)
        {
            throw new Exception($"Card {id} not found in available cards! Check if your debug selection matches the chosen patrons.");
        }
        availableCards.Remove(card);
        return card;
    }
}
