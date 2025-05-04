using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Bots;
using System.IO;
using System.Reflection;
using System.Linq;
using ScriptsOfTribute.AI;
using System;
using ScriptsOfTribute;
using System.Collections;
using UnityBots;

[System.Serializable]
public class BotData
{
    public string Name;
    public string ShortDescription;
    public string LongDescription;
    public Sprite Avatar;
    public string TypeName;

    [NonSerialized]
    public Type AIClass;
}

public enum PlayerSideSelection
{
    Player1,
    Player2,
    Random
}

public class GameSetupPanelController : MonoBehaviour
{
    [Header("Prefab and container")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject botFramePrefab;
    [SerializeField] private Transform botListContainer;

    [Header("UI elements")]
    [SerializeField] private TMP_InputField seedInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private SwitchButton debugModeButton;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Built-in bots")]
    [SerializeField] private Sprite bestMCTS3AvatarSprite;
    [SerializeField] private Sprite SOISMCTSAvatarSprite;
    [SerializeField] private Sprite maxPrestigeBotAvatarSprite;

    [Header("Side selection")]
    [SerializeField] private Button player1Button;
    [SerializeField] private Button player2Button;
    [SerializeField] private Button randomButton;

    private BotFrameDisplay selectedBot;
    private List<BotFrameDisplay> allBotFrames = new();
    private PlayerSideSelection selectedSide = PlayerSideSelection.Random;

    public void StartSetup()
    {
        player1Button.onClick.AddListener(() => SelectSide(PlayerSideSelection.Player1));
        player2Button.onClick.AddListener(() => SelectSide(PlayerSideSelection.Player2));
        randomButton.onClick.AddListener(() => SelectSide(PlayerSideSelection.Random));
        SelectSide(PlayerSideSelection.Random);
        var allBots = GetBuiltinBots();
        allBots.AddRange(LoadExternalBots());
        PopulateBotList(allBots);
        confirmButton.onClick.AddListener(OnConfirmClicked);
        confirmButton.interactable = false;
        mainPanel.SetActive(true);
    }

    private List<BotData> GetBuiltinBots()
    {
        return new List<BotData>
        {
            new BotData
            {
                Name = "BestMCTS3",
                ShortDescription = "IEEE CoG 2024 winner",
                LongDescription = "Authored by ...",
                Avatar = bestMCTS3AvatarSprite,
                AIClass = typeof(BestMCTS3)
            },
            new BotData
            {
                Name = "SOISMCTS",
                ShortDescription = "IEEE CoG 2024 3rd place",
                LongDescription = "Authored by ...",
                Avatar = SOISMCTSAvatarSprite,
                AIClass = typeof(SOISMCTS.SOISMCTS)
            },
            new BotData
            {
                Name = "Basic bot",
                ShortDescription = "Simple agent, low level",
                LongDescription = "Based on simple heuristic and search on depth of 2",
                Avatar = maxPrestigeBotAvatarSprite,
                AIClass = typeof(MaxPrestigeBot)
            },
            new BotData
            {
                Name = "gRPC bot",
                ShortDescription = "External bot",
                LongDescription = "Remember to run separate process with the bot on ports 50000/49000",
                Avatar = null,
                AIClass = typeof(GrpcBotAI)
            }
        };
    }

    void PopulateBotList(List<BotData> allBots)
    {
        foreach (var bot in allBots)
        {
            var go = Instantiate(botFramePrefab, botListContainer);
            var frame = go.GetComponent<BotFrameDisplay>();
            frame.Setup(bot, this);
            allBotFrames.Add(frame);
        }
    }

    private List<BotData> LoadExternalBots()
    {
        var list = new List<BotData>();
        string dir = Path.Combine(Application.streamingAssetsPath, "Bots");

        if (!Directory.Exists(dir)) return list;

        foreach (var dll in Directory.GetFiles(dir, "*.dll"))
        {
            try
            {
                var asm = Assembly.LoadFrom(dll);
                var types = asm.GetTypes().Where(t => typeof(AI).IsAssignableFrom(t) && !t.IsAbstract);
                foreach (var t in types)
                {
                    list.Add(new BotData {
                        Name = t.Name,
                        ShortDescription = "Your bot",
                        LongDescription = $"Loaded from {Path.GetFileName(dll)}",
                        AIClass = t,
                        Avatar = null,
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Błąd ładowania DLL: {dll} => {e.Message}");
            }
        }

        return list;
    }

    public void SelectBot(BotFrameDisplay frame)
    {
        if (selectedBot != null)
            selectedBot.Deselect();

        selectedBot = frame;
        selectedBot.Select();
        confirmButton.interactable = true;
    }

    private void SelectSide(PlayerSideSelection side)
    {
        selectedSide = side;

        HighlightButton(player1Button, side == PlayerSideSelection.Player1);
        HighlightButton(player2Button, side == PlayerSideSelection.Player2);
        HighlightButton(randomButton,  side == PlayerSideSelection.Random);
    }

    private void HighlightButton(Button button, bool active)
    {
        button.gameObject.GetComponent<Image>().color = active ? Color.green : Color.black;
    }

    void OnConfirmClicked()
    {
        if (selectedBot == null)
        {
            Debug.LogWarning("Nie wybrano bota!");
            return;
        }

        Type chosenBot = selectedBot.AIClass;
        string seed = seedInputField.text;
        bool debugMode = debugModeButton.GetState();

        (PlayerEnum humanPlayer, PlayerEnum aiPlayer) sides  = selectedSide switch
        {
            PlayerSideSelection.Player1 => (PlayerEnum.PLAYER1, PlayerEnum.PLAYER2),
            PlayerSideSelection.Player2 => (PlayerEnum.PLAYER2, PlayerEnum.PLAYER1),
            PlayerSideSelection.Random  => UnityEngine.Random.Range(0, 2) == 0 ? (PlayerEnum.PLAYER1, PlayerEnum.PLAYER2) : (PlayerEnum.PLAYER2, PlayerEnum.PLAYER1),
            _ => (PlayerEnum.PLAYER1, PlayerEnum.PLAYER2)
        };
        mainPanel.SetActive(false);
        GameSetupManager.Instance.StartInitialSetup(chosenBot, sides.humanPlayer, sides.aiPlayer, debugMode, seed);
    }

    private IEnumerator FadeOutAndDisable(CanvasGroup canvasGroup, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(false);
    }
}
