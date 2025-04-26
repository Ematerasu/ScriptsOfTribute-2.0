using System.Collections.Generic;
using UnityEngine;
using ScriptsOfTribute.Board;
using System.Collections;

public class CompletedActionHistoryBuilder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject completedActionUIPrefab;
    [SerializeField] private Sprite player1Sprite;
    [SerializeField] private Sprite player2Sprite;
    [SerializeField] private GameObject loadingSpinner;

    [Header("Settings")]
    [SerializeField] private int batchSize = 10;
    [SerializeField] private float delayBetweenBatches = 0.05f;

    private static readonly HashSet<CompletedActionType> PlayerDirectActions = new()
    {
        CompletedActionType.BUY_CARD,
        CompletedActionType.PLAY_CARD,
        CompletedActionType.ACTIVATE_AGENT,
        CompletedActionType.ACTIVATE_PATRON,
        CompletedActionType.ATTACK_AGENT,
        CompletedActionType.ACQUIRE_CARD,
        CompletedActionType.REPLACE_TAVERN,
        CompletedActionType.DESTROY_CARD,
        CompletedActionType.DISCARD,
        CompletedActionType.REFRESH,
        CompletedActionType.TOSS,
        CompletedActionType.KNOCKOUT,
        CompletedActionType.END_TURN
    };

    public void BuildHistory(List<CompletedAction> actions)
    {
        StartCoroutine(BuildHistoryCoroutine(actions));
    }

    private IEnumerator BuildHistoryCoroutine(List<CompletedAction> actions)
    {
        loadingSpinner.SetActive(true);
        int created = 0;
        for (int i = 0; i < actions.Count; i++)
        {
            var action = actions[i];
            if (!PlayerDirectActions.Contains(action.Type))
                continue;

            string mainText = GenerateMainText(action);
            string subText = GenerateSubText(actions, i);

            var go = Instantiate(completedActionUIPrefab, contentParent);
            var ui = go.GetComponent<CompletedActionUI>();
            ui.Initialize(action, player1Sprite, player2Sprite, mainText, subText);

            created++;

            if (created % batchSize == 0)
            {
                yield return new WaitForSecondsRealtime(delayBetweenBatches);
            }
        }

        loadingSpinner.SetActive(false);
    }

    private string GenerateMainText(CompletedAction action)
    {
        string actionName = action.Type switch
        {
            CompletedActionType.BUY_CARD => "Buy",
            CompletedActionType.ACQUIRE_CARD => "Acquire",
            CompletedActionType.PLAY_CARD => "Play",
            CompletedActionType.ACTIVATE_AGENT => "Activate Agent",
            CompletedActionType.ACTIVATE_PATRON => "Activate Patron",
            CompletedActionType.ATTACK_AGENT => "Attack Agent",
            CompletedActionType.REPLACE_TAVERN => "Replace in Tavern",
            CompletedActionType.DESTROY_CARD => "Destroy Card",
            CompletedActionType.DISCARD => "Discard",
            CompletedActionType.REFRESH => "Refresh",
            CompletedActionType.TOSS => "Toss",
            CompletedActionType.KNOCKOUT => "Knockout",
            CompletedActionType.END_TURN => "End Turn",
            _ => "Action"
        };

        if (action.SourceCard != null)
            return $"{actionName} {action.SourceCard.Name}";
        if (action.TargetCard != null)
            return $"{actionName} {action.TargetCard.Name}";
        if (action.SourcePatron.HasValue)
            return $"{actionName} {action.SourcePatron.Value}";

        return actionName;
    }

    private string GenerateSubText(List<CompletedAction> actions, int startIndex)
    {
        List<string> parts = new();
        int currentPlayer = (int)actions[startIndex].Player;

        for (int i = startIndex + 1; i < actions.Count; i++)
        {
            var nextAction = actions[i];
            if ((int)nextAction.Player != currentPlayer)
                break;
            if (PlayerDirectActions.Contains(nextAction.Type))
                break;

            switch (nextAction.Type)
            {
                case CompletedActionType.GAIN_COIN:
                    parts.Add($"+{nextAction.Amount} Coin");
                    break;
                case CompletedActionType.GAIN_POWER:
                    parts.Add($"+{nextAction.Amount} Power");
                    break;
                case CompletedActionType.GAIN_PRESTIGE:
                    parts.Add($"+{nextAction.Amount} Prestige");
                    break;
                case CompletedActionType.AGENT_DEATH:
                    parts.Add($"Agent {nextAction.TargetCard?.Name} defeated");
                    break;
                case CompletedActionType.HEAL_AGENT:
                    parts.Add($"Heal {nextAction.TargetCard?.Name} (+{nextAction.Amount})");
                    break;
                case CompletedActionType.DRAW:
                    parts.Add($"Draw {nextAction.Amount}");
                    break;
                case CompletedActionType.ADD_PATRON_CALLS:
                    parts.Add($"Patron calls +{nextAction.Amount}");
                    break;
                case CompletedActionType.ADD_SUMMERSET_SACKING:
                    parts.Add($"Summerset Sacking +{nextAction.Amount}");
                    break;
                case CompletedActionType.ADD_BEWILDERMENT_TO_OPPONENT:
                    parts.Add("Bewilderment to opponent");
                    break;
                case CompletedActionType.ADD_WRIT_OF_COIN:
                    parts.Add("Add Writ of Coin");
                    break;
                default:
                    break;
            }
        }

        return string.Join("; ", parts);
    }
}
