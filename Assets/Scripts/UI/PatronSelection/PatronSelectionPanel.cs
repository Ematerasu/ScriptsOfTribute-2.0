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
        draftText.text = $"{pickOrder[pickIndex]} is picking";
    }

    private void OnPatronClicked(PatronId patron)
    {
        StartCoroutine(HandlePatronPickCoroutine(patron));
    }

    private IEnumerator HandlePatronPickCoroutine(PatronId patron)
    {
        if (!availablePatrons.Contains(patron)) yield break;

        selectedPatrons.Add(patron);
        availablePatrons.Remove(patron);

        if (patronButtons.TryGetValue(patron, out GameObject btnGO))
        {
            Color color = pickOrder[pickIndex] == PlayerEnum.PLAYER1
                ? new Color32(0x57, 0x6B, 0xEE, 255) // #576BEE
                : new Color32(0xEE, 0x67, 0x57, 255); // #EE6757

            float glow = 2.0f;

            btnGO.GetComponent<PatronGlowPulsing>().Initialize(color, glow);
            btnGO.GetComponent<Button>().interactable = false;
        }

        yield return new WaitForSeconds(0.5f);

        pickIndex++;

        if (selectedPatrons.Count == 5)
        {
            panelRoot.SetActive(false);
            GameManager.Instance.StartGameWithPatrons(selectedPatrons.ToArray());
        }
        else if (selectedPatrons.Count == 2)
        {
            selectedPatrons.Add(PatronId.TREASURY);
            RefreshUI();
        }
        else
        {
            RefreshUI();
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
