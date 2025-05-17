using System.Collections.Generic;
using ScriptsOfTribute;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombosPanelController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private GameObject container;
    [SerializeField] private GameObject focusBackGlow; 
    [SerializeField] private GameObject bg;
    [SerializeField] private GameObject focusFrontGradient;

    [Header("Config")]
    [SerializeField] private float collapsedWidth = 30f;
    [SerializeField] private float expandedWidth = 600f;
    [SerializeField] private float expandSpeed = 10f;

    [Header("Content")]
    [SerializeField] private GameObject hiddenView;
    [SerializeField] private GameObject expandedView;


    [Header("Expanded View Columns")]
    [SerializeField] private List<PatronColumnUI> patronColumns;

    [Header("Prefabs")]
    [SerializeField] private GameObject patronIconPrefab;
    [SerializeField] private GameObject effectIconPrefab;

    [Header("Effect Sprites Mapping")]
    [SerializeField] private List<EffectTypeSpritePair> effectTypeSpritePairs = new();
    [SerializeField] private Sprite choiceSpriteIcon;

    private RectTransform rectTransform;
    private bool isExpanded = false;
    private bool fullyExpanded = false;
    private Dictionary<EffectType, Sprite> effectTypeToSprite = new();

    private void Awake()
    {
        rectTransform = container.GetComponent<RectTransform>();
        CollapseImmediate();

        foreach (var pair in effectTypeSpritePairs)
        {
            if (!effectTypeToSprite.ContainsKey(pair.effectType))
            {
                effectTypeToSprite.Add(pair.effectType, pair.sprite);
            }
        }
    }

    private void Update()
    {
        float targetHeight = isExpanded ? expandedWidth : collapsedWidth;
        Vector2 size = rectTransform.sizeDelta;
        size.y = Mathf.Lerp(size.y, targetHeight, Time.deltaTime * expandSpeed);
        rectTransform.sizeDelta = size;

        if (isExpanded && !fullyExpanded && Mathf.Abs(size.y - expandedWidth) < 1f)
        {
            fullyExpanded = true;
            hiddenView.SetActive(false);
            expandedView.SetActive(true);
        }
        else if (!isExpanded && fullyExpanded && Mathf.Abs(size.y - collapsedWidth) < 1f)
        {
            fullyExpanded = false;
            hiddenView.SetActive(true);
            expandedView.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Expand();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Collapse();
    }

    private void Expand()
    {
        isExpanded = true;
        focusBackGlow.SetActive(true);
        focusFrontGradient.SetActive(true);
        hiddenView.SetActive(false);
        expandedView.SetActive(true);
    }

    private void Collapse()
    {
        isExpanded = false;
        focusBackGlow.SetActive(false);
        focusFrontGradient.SetActive(false);
        hiddenView.SetActive(true);
        expandedView.SetActive(false);
    }

    private void CollapseImmediate()
    {
        isExpanded = false;
        fullyExpanded = false;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, collapsedWidth);
        focusBackGlow.SetActive(false);
        focusFrontGradient.SetActive(false);
        hiddenView.SetActive(true);
        expandedView.SetActive(false);
    }

    public void InitializeExpandedView(PatronId[] activePatrons)
    {
        int columnIndex = 0;

        foreach (var patronId in activePatrons)
        {
            if (patronId == PatronId.TREASURY)
                continue;

            if (columnIndex >= patronColumns.Count)
            {
                Debug.LogWarning("Not enough columns defined for active patrons.");
                break;
            }

            var column = patronColumns[columnIndex];
            column.patronId = patronId;
            column.header.sprite = LoadPatronSprite(patronId);
            column.header.gameObject.SetActive(true);
            columnIndex++;
        }

        for (int i = columnIndex; i < patronColumns.Count; i++)
        {
            patronColumns[i].header.gameObject.SetActive(false);
        }
    }

    public void UpdateCombos(ComboStates states)
    {
        foreach (Transform child in hiddenView.transform)
            Destroy(child.gameObject);

        foreach (var column in patronColumns)
        {
            ClearColumn(column);
        }

        foreach (var kvp in states.All)
        {
            var patronId = kvp.Key;
            if (patronId == PatronId.TREASURY)
                continue;
            var comboState = kvp.Value;
            var column = patronColumns.Find(c => c.patronId == patronId);
            column.comboLevel.text = comboState.CurrentCombo.ToString();
            bool hasAnyCombo = false;
            for (int i = 1; i <= 3; i++)
            {
                if (comboState.All[i] != null && comboState.All[i].Count > 0)
                {
                    hasAnyCombo = true;
                    break;
                }
            }

            if (!hasAnyCombo)
                continue;

            var patronIcon = Instantiate(patronIconPrefab, hiddenView.transform);
            var iconImage = patronIcon.GetComponentInChildren<UnityEngine.UI.Image>();
            iconImage.sprite = LoadPatronSprite(patronId);

            if (column.header != null)
            {
                column.header.sprite = LoadPatronSprite(patronId);
            }

            FillComboZone(column.combo2Zone, comboState.All[1]);
            FillComboZone(column.combo3Zone, comboState.All[2]);
            FillComboZone(column.combo4Zone, comboState.All[3]);
        }
    }

    private void FillComboZone(Transform zone, List<UniqueBaseEffect> effects)
    {
        if (zone == null || effects == null)
            return;

        foreach (var effect in effects)
        {
            var effectIcon = Instantiate(effectIconPrefab, zone);
            var image = effectIcon.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.sprite = GetSpriteForEffect(effect);
            }
        }
    }

    private void ClearColumn(PatronColumnUI column)
    {
        if (column.combo2Zone != null)
            foreach (Transform child in column.combo2Zone)
                Destroy(child.gameObject);

        if (column.combo3Zone != null)
            foreach (Transform child in column.combo3Zone)
                Destroy(child.gameObject);

        if (column.combo4Zone != null)
            foreach (Transform child in column.combo4Zone)
                Destroy(child.gameObject);
                
        column.comboLevel.text = "0";
    }

    private Sprite GetSpriteForEffect(UniqueBaseEffect effect)
    {
        if (effect is UniqueEffectOr)
        {
            return choiceSpriteIcon;
        }
        else if (effect is UniqueEffect simpleEffect)
        {
            if (effectTypeToSprite.TryGetValue(simpleEffect.Type, out var sprite))
            {
                return sprite;
            }
        }

        return null;
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

[System.Serializable]
public struct EffectTypeSpritePair
{
    public EffectType effectType;
    public Sprite sprite;
}