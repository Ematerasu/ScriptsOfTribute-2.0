using System;
using System.Collections;
using System.Collections.Generic;
using ScriptsOfTribute;
using UnityEngine;

public class GameSetupManager : MonoBehaviour
{
    public static GameSetupManager Instance { get; private set; }

    [Header("Manager References")]
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private BoardManager _boardManager;
    [SerializeField] private AIManager _aiManager;
    [SerializeField] private CompletedActionHistoryBuilder _historyBuilder;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private GameSetupPanelController _setupcontroller;

    [Header("Game Settings")]
    private Type _selectedBot;
    [SerializeField] private bool botDebugMode = false;
    public bool IsBotDebugMode => botDebugMode;
    private ulong _seed = 0;
    private PlayerEnum _humanPlayer;
    private PlayerEnum _botPlayer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        _setupcontroller.StartSetup();
    }

    public void StartInitialSetup(Type selectedBot, PlayerEnum humanPlayer, PlayerEnum botPlayer, bool isDebugMode, string seed)
    {
        _selectedBot = selectedBot;
        _humanPlayer = humanPlayer;
        _botPlayer = botPlayer;
        botDebugMode = isDebugMode;
        bool parsed = ulong.TryParse(seed, out ulong parsedSeed);
        _seed = parsed ? parsedSeed : 0;

        _gameManager.HumanPlayer = humanPlayer;
        
        if (_gameManager.IsDebugMode)
        {
            _gameManager.InitializeDebugGame();
        }
        else
        {
            _aiManager.InitializeBot(selectedBot, botPlayer);
            _uiManager.ShowPatronDraft(GetAvailablePatrons());
            AudioManager.Instance.SwapMusic(AudioManager.Instance.gameMusic);
        }
    }

    public void StartGameWithPatrons(List<PatronId> selectedPatrons)
    {
        _gameManager.StartGameWithPatrons(selectedPatrons.ToArray(), _seed);
    }

    public void ResetAndStartPatronDraft()
    {
        StartCoroutine(ResetAndStartPatronDraftCoroutine());
    }

    private IEnumerator ResetAndStartPatronDraftCoroutine()
    {
        yield return null;

        _boardManager.ClearBoard();
        yield return null;

        _aiManager.InitializeBot(_selectedBot, _gameManager.AIPlayer);
        yield return null;

        _historyBuilder.ClearHistory();
        yield return null;

        _uiManager.ShowPatronDraft(GetAvailablePatrons());
    }

    private List<PatronId> GetAvailablePatrons()
    {
        return new List<PatronId>()
        { 
            PatronId.ANSEI,
            PatronId.DUKE_OF_CROWS,
            PatronId.RAJHIN, 
            PatronId.ORGNUM, 
            PatronId.HLAALU, 
            PatronId.PELIN, 
            PatronId.RED_EAGLE,
            PatronId.SAINT_ALESSIA,
        };
    }

}
