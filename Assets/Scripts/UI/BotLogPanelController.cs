using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

public class BotLogPanelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private TMP_InputField logInputField;
    [SerializeField] private int maxLogsToShow = 100;

    [Header("Animation")]
    [SerializeField] private float hiddenX = -240f;
    [SerializeField] private float visibleX = 0f;
    [SerializeField] private float slideSpeed = 500f;

    private bool isMouseOver = false;
    private bool isAnimating = false;

    private int lastLogIndex = 0;
    private StringBuilder _logBuilder = new();

    private void Start()
    {
        if (!GameSetupManager.Instance.IsBotDebugMode)
        {
            gameObject.SetActive(false);
            return;
        }
        panelRoot.anchoredPosition = new Vector2(hiddenX, panelRoot.anchoredPosition.y);
        logInputField.text = string.Empty;
        StartCoroutine(LogWatcher());
    }

    private IEnumerator LogWatcher()
    {
        if (!GameSetupManager.Instance.IsBotDebugMode) yield return null;

        while (!AIManager.Instance.BotInitialized)
            yield return null;
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            if (GameSetupManager.Instance.IsBotDebugMode)
            {
                TryAppendNewLogs();
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void Update()
    {
        if (!GameSetupManager.Instance.IsBotDebugMode) return;

        float targetX = isMouseOver ? visibleX : hiddenX;
        Vector2 pos = panelRoot.anchoredPosition;
        pos.x = Mathf.MoveTowards(pos.x, targetX, slideSpeed * Time.deltaTime);
        panelRoot.anchoredPosition = pos;
    }

    public void OnPointerEnter() => isMouseOver = true;
    public void OnPointerExit() => isMouseOver = false;

    private void TryAppendNewLogs()
    {
        var allLogs = AIManager.Instance.GetBotLogs();

        if (allLogs.Count == lastLogIndex)
            return;

        for (int i = lastLogIndex; i < allLogs.Count; i++)
        {
            var (time, message) = allLogs[i];
            _logBuilder.AppendLine($"[{time:HH:mm:ss}] {message}");
        }

        lastLogIndex = allLogs.Count;

        string[] allLines = _logBuilder.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int linesToSkip = Mathf.Max(0, allLines.Length - maxLogsToShow);
        string visibleText = string.Join("\n", allLines, linesToSkip, allLines.Length - linesToSkip);

        logInputField.text = visibleText;

    }

    public void Clear()
    {
        _logBuilder.Clear();
        logInputField.text = string.Empty;
        lastLogIndex = 0;
    }
}
