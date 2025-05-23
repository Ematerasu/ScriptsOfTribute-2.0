using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using ScriptsOfTribute;
using System.Collections;

public class PatronSelectionPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI draftText;
    [SerializeField] private Transform patronButtonContainer;
    [SerializeField] private GameObject patronButtonPrefab;
    [SerializeField] private Button randomPickButton;

    private Dictionary<PatronId, GameObject> patronButtons = new();
    private List<PatronId> availablePatrons;
    private List<PatronId> selectedPatrons = new();
    private int pickIndex = 0;
    private PlayerEnum[] pickOrder = new[] {
        PlayerEnum.PLAYER1,
        PlayerEnum.PLAYER2,
        PlayerEnum.PLAYER2,
        PlayerEnum.PLAYER1
    };

    public void ShowPatronDraft(List<PatronId> patrons)
    {
        availablePatrons = new List<PatronId>(patrons);
        selectedPatrons.Clear();
        pickIndex = 0;

        foreach(Transform child in patronButtonContainer)
        {
            Destroy(child.gameObject);
        }

        SetUpUI();
        RefreshUI();
        panelRoot.SetActive(true);
    }

    private void SetUpUI()
    {
        foreach (var patron in availablePatrons)
        {
            var btnGO = Instantiate(patronButtonPrefab, patronButtonContainer);
            patronButtons[patron] = btnGO;

            var btn = btnGO.GetComponent<Button>();
            btnGO.GetComponentInChildren<Image>().sprite = LoadPatronSprite(patron);
            btn.onClick.AddListener(() => OnPatronClicked(patron));
        }
    }
    private void RefreshUI()
    {
        foreach (var patron in patronButtons.Values)
        {
            var btn = patron.GetComponent<Button>();
            btn.interactable = pickOrder[pickIndex] == GameManager.Instance.HumanPlayer;
        }

        if (pickOrder[pickIndex] == GameManager.Instance.AIPlayer)
        {
            AIManager.Instance.PickOnePatron(availablePatrons, pickIndex+1, (patronId) => OnPatronClicked(patronId));
        }
        string playerPicking = pickOrder[pickIndex] == GameManager.Instance.HumanPlayer ? "You are" : "Enemy is";
        draftText.text = $"{playerPicking} picking";
    }

    private void OnPatronClicked(PatronId patron)
    {
        if (!availablePatrons.Contains(patron)) return;

        foreach (var kv in patronButtons)
        {
            kv.Value.GetComponent<Button>().interactable = false;
        }

        StartCoroutine(HandlePatronPickCoroutine(patron));
    }

    public void OnRandomPickClicked()
    {
        if (pickOrder[pickIndex] != GameManager.Instance.HumanPlayer)
            return;

        if (availablePatrons.Count == 0)
            return;

        randomPickButton.interactable = false;

        var randomPick = availablePatrons[Random.Range(0, availablePatrons.Count)];
        foreach (var kv in patronButtons)
        {
            kv.Value.GetComponent<Button>().interactable = false;
        }

        StartCoroutine(HandlePatronPickCoroutine(randomPick));
    }
    private IEnumerator HandlePatronPickCoroutine(PatronId patron)
    {
        if (!availablePatrons.Contains(patron)) yield break;

        selectedPatrons.Add(patron);
        availablePatrons.Remove(patron);

        if (patronButtons.TryGetValue(patron, out GameObject btnGO))
        {
            Color color = pickOrder[pickIndex] == GameSetupManager.Instance.HumanPlayer
                ? new Color32(0x57, 0x6B, 0xEE, 255) // #576BEE
                : new Color32(0xEE, 0x67, 0x57, 255); // #EE6757

            btnGO.GetComponent<PatronGlowPulsing>().Initialize(color);
            btnGO.GetComponent<Button>().interactable = false;
        }

        yield return new WaitForSeconds(0.5f);

        pickIndex++;

        if (selectedPatrons.Count == 5)
        {
            panelRoot.SetActive(false);
            foreach (Transform child in patronButtonContainer)
            {
                Destroy(child.gameObject);
            }
            GameSetupManager.Instance.StartGameWithPatrons(selectedPatrons);
        }
        else if (selectedPatrons.Count == 2)
        {
            selectedPatrons.Add(PatronId.TREASURY);
            RefreshUI();
            randomPickButton.interactable = true;
        }
        else
        {
            RefreshUI();
            randomPickButton.interactable = true;
        }
    }

    private Sprite LoadPatronSprite(PatronId patronId)
    {
        string path = $"Sprites/Patrons/{patronId}";
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogError($"Nie znaleziono sprite'a patrona pod ścieżką: {path}");
        }
        return sprite;
    }
}
