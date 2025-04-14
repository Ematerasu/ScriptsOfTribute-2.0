using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using ScriptsOfTribute;

public class PatronSelectionPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI draftText;
    [SerializeField] private Transform patronButtonContainer;
    [SerializeField] private GameObject patronButtonPrefab;

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

        RefreshUI();
        panelRoot.SetActive(true);
    }

    private void RefreshUI()
    {
        foreach (Transform child in patronButtonContainer)
            Destroy(child.gameObject);

        foreach (var patron in availablePatrons)
        {
            var btnGO = Instantiate(patronButtonPrefab, patronButtonContainer);
            var btn = btnGO.GetComponent<Button>();
            btnGO.GetComponentInChildren<Image>().sprite = LoadPatronSprite(patron);
            btn.onClick.AddListener(() => OnPatronClicked(patron));
        }

        draftText.text = $"{pickOrder[pickIndex]} is picking";
    }

    private void OnPatronClicked(PatronId patron)
    {
        Debug.Log("Click");
        if (!availablePatrons.Contains(patron)) return;

        selectedPatrons.Add(patron);
        availablePatrons.Remove(patron);
        pickIndex++;

        if (selectedPatrons.Count == 4)
        {
            panelRoot.SetActive(false);
            GameManager.Instance.StartGameWithPatrons(selectedPatrons.ToArray());
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
