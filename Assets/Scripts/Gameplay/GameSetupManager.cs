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

    [Header("Game Settings")]
    [SerializeField] private BotType selectedBot = BotType.MaxPrestige;
    [SerializeField] private bool botDebugMode = false;
    public bool IsBotDebugMode => botDebugMode;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartInitialSetup();
    }

    public void StartInitialSetup()
    {
        if (_gameManager.IsDebugMode)
        {
            _gameManager.InitializeDebugGame();
        }
        else
        {
            _aiManager.InitializeBot(selectedBot, _gameManager.AIPlayer);
            _uiManager.ShowPatronDraft(GetAvailablePatrons());
        }
    }

    public void StartGameWithPatrons(List<PatronId> selectedPatrons)
    {
        _gameManager.StartGameWithPatrons(selectedPatrons.ToArray());
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

        _aiManager.InitializeBot(selectedBot, _gameManager.AIPlayer);
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
