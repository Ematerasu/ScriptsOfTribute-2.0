using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute;

public class CardManager : MonoBehaviour
{

    public static CardManager Instance { get; private set; }

    [Header("Deck Card Prefabs")]
    [SerializeField] private GameObject[] DeckPrefabs; // 9 prefabów, dla każdego decku
    
    [Header("Effect Icon Mapping")]
    [SerializeField] private List<EffectPrefabPairs> EffectIconsList;
    private Dictionary<VisualEffectType, Sprite> EffectIcons = new Dictionary<VisualEffectType, Sprite>();

    [Header("Effect Prefab")]
    [SerializeField] private GameObject BasicEffectPrefab;
    [SerializeField] private GameObject ComboEffectPrefab;

    [Header("Card Settings")]
    [SerializeField] public float CardScale = 1.0f;
    [SerializeField] private List<DeckScaleOverride> deckScaleOverrides;
    private Dictionary<PatronId, float> _overrideDict;

    public float GetDefaultScaleForDeck(PatronId deck) => _overrideDict.TryGetValue(deck, out var s) ? s : CardScale;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeEffectIcons();
        _overrideDict = new Dictionary<PatronId, float>();
        foreach (var o in deckScaleOverrides)
            _overrideDict[o.Deck] = o.Scale;
    }

    private void InitializeEffectIcons()
    {
        EffectIcons.Clear();
        foreach (var pair in EffectIconsList)
        {
            if (!EffectIcons.ContainsKey(pair.Type))
            {
                EffectIcons.Add(pair.Type, pair.Icon);
            }
        }
    }

    public GameObject CreateCardVisual(UniqueCard cardData)
    {
        GameObject cardPrefab = DeckPrefabs[(int)cardData.Deck];
        GameObject cardObject = Instantiate(cardPrefab);

        float scale = _overrideDict.TryGetValue(cardData.Deck, out var s)
                        ? s
                        : CardScale;

        cardObject.transform.localScale = Vector3.one * scale;
        
        ConfigureCard(cardObject, cardData);

        return cardObject;
    }

    private void ConfigureCard(GameObject cardObject, UniqueCard cardData)
    {
        var nameText = cardObject.transform.Find("Name").GetComponent<TextMeshPro>();
        var costText = cardObject.transform.Find("Cost").GetComponent<TextMeshPro>();
        var hpText = cardObject.transform.Find("HP").GetComponent<TextMeshPro>();

        nameText.text = cardData.Name;
        costText.text = cardData.Cost > 0 ? cardData.Cost.ToString() : "";
        hpText.text = cardData.HP > 0 ? cardData.HP.ToString() : "";
        cardObject.GetComponent<SpriteRenderer>().sprite = CardUtils.LoadCardSprite(cardData.Deck, cardData.CommonId);
        
        var basicEffectsContainer = cardObject.transform.Find("BasicEffectsContainer");
        var comboEffectsContainer = cardObject.transform.Find("ComboEffectsContainer");

        ConfigureEffects(basicEffectsContainer, GetDecomposedEffects(cardData.Effects, 0));
        ConfigureEffects(comboEffectsContainer, GetComboEffects(cardData.Effects), IsBasic: false);
    }

    private List<UniqueBaseEffect> GetDecomposedEffects(UniqueComplexEffect?[] effects, int index)
    {
        List<UniqueBaseEffect> decomposedEffects = new List<UniqueBaseEffect>();

        if (effects == null || index >= effects.Length || effects[index] == null)
            return decomposedEffects;

        if (effects[index] is UniqueComplexEffect complexEffect)
        {
            foreach (var effect in complexEffect.Decompose())
            {
                if (effect is UniqueEffectOr orEffect)
                {
                    decomposedEffects.Add(orEffect);
                }
                else if (effect is UniqueBaseEffect baseEffect)
                {
                    decomposedEffects.Add(baseEffect);
                }
            }
        }

        return decomposedEffects;
    }

    private List<UniqueBaseEffect> GetComboEffects(UniqueComplexEffect?[] effects)
    {
        List<UniqueBaseEffect> comboEffects = new List<UniqueBaseEffect>();

        for (int i = 1; i <= 3; i++)
        {
            if (effects == null || i >= effects.Length || effects[i] == null)
                continue;

            comboEffects.AddRange(GetDecomposedEffects(effects, i));
        }

        return comboEffects;
    }

    private void ConfigureEffects(Transform container, List<UniqueBaseEffect> effects, bool IsBasic=true)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        if (effects == null || effects.Count == 0) return;

        foreach (var effect in effects)
        {
            GameObject effectObject = Instantiate(IsBasic ? BasicEffectPrefab : ComboEffectPrefab, container);
            ConfigureEffect(effectObject, effect);
        }
    }

    private void ConfigureEffect(GameObject effectObject, UniqueBaseEffect effect)
    {
        var icon = effectObject.transform.Find("EffectIcon").GetComponent<SpriteRenderer>();
        var valueText = effectObject.transform.Find("EffectValue").GetComponent<TextMeshPro>();

        VisualEffectType visualType = MapEffectTypeToVisual(effect);
        if (EffectIcons.TryGetValue(visualType, out Sprite sprite))
        {
            icon.sprite = sprite;
        }
        else
        {
            Debug.LogWarning($"Brak ikony dla efektu: {visualType}");
        }

        valueText.text = GetEffectText(effect);
    }

    private VisualEffectType MapEffectTypeToVisual(UniqueBaseEffect effect)
    {
        if (effect is UniqueEffectOr)
            return VisualEffectType.CHOICE;

        if (effect is UniqueEffect uniqueEffect)
        {
            return uniqueEffect.Type switch
            {
                EffectType.GAIN_COIN => VisualEffectType.GAIN_COIN,
                EffectType.GAIN_POWER => VisualEffectType.GAIN_POWER,
                EffectType.GAIN_PRESTIGE => VisualEffectType.GAIN_PRESTIGE,
                EffectType.OPP_LOSE_PRESTIGE => VisualEffectType.OPP_LOSE_PRESTIGE,
                EffectType.DRAW => VisualEffectType.DRAW,
                EffectType.REPLACE_TAVERN => VisualEffectType.REPLACE_TAVERN,
                EffectType.ACQUIRE_TAVERN => VisualEffectType.ACQUIRE_TAVERN,
                EffectType.DESTROY_CARD => VisualEffectType.DESTROY_CARD,
                EffectType.HEAL => VisualEffectType.HEAL,
                EffectType.KNOCKOUT => VisualEffectType.KNOCKOUT,
                EffectType.PATRON_CALL => VisualEffectType.PATRON_CALL,
                EffectType.CREATE_SUMMERSET_SACKING => VisualEffectType.CREATE_CARD,
                EffectType.DONATE => VisualEffectType.DONATE,
                _ => VisualEffectType.CHOICE
            };
        }

        return VisualEffectType.CHOICE;
    }

    private string GetEffectText(UniqueBaseEffect effect)
    {
        if (effect is UniqueEffect uniqueEffect)
            return $"{uniqueEffect.Amount}";
        else if (effect is UniqueEffectOr)
            return "2";

        return "";
    }
}

public enum VisualEffectType
{
    GAIN_COIN,
    GAIN_POWER,
    GAIN_PRESTIGE,
    OPP_LOSE_PRESTIGE,
    REPLACE_TAVERN,
    ACQUIRE_TAVERN,
    DESTROY_CARD,
    DRAW,
    OPP_DISCARD,
    RETURN_TOP,
    KNOCKOUT,
    PATRON_CALL,
    CREATE_CARD,
    HEAL,
    DONATE,
    CHOICE,
}


[System.Serializable]
public struct EffectPrefabPairs
{
    public VisualEffectType Type;
    public Sprite Icon;
}

[System.Serializable]
public struct DeckScaleOverride
{
    public PatronId Deck;
    public float    Scale;
}