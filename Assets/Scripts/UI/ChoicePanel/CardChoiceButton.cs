using UnityEngine;
using UnityEngine.UI;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute;
using Unity.VisualScripting;

public class CardChoiceButton : MonoBehaviour
{
    [SerializeField] private Image cardImage;

    [Header("Glow Settings")]
    [SerializeField] private Color selectedGlowColor = new Color(0.2f, 1f, 0.2f, 1f);
    [SerializeField] private float selectedGlowIntensity = 0f;
    [SerializeField] private float unselectedGlowIntensity = 0f;

    [Header("Outline Settings")]
    [SerializeField] private float outlineAlpha = 0.255f;
    [SerializeField] private float outlineGlow = 45.0f;
    [SerializeField] private float outlineWidth = 0.083f;
    [SerializeField] private int outlinePixelWidth = 2;

    private UniqueCard _card;
    private System.Action<UniqueCard> _onClick;
    private bool _selected = false;
    private Material _materialInstance;
    [SerializeField] private Material OGMaterial;

    private void Awake()
    {
        if (cardImage != null)
        {
            _materialInstance = Instantiate(OGMaterial);
            cardImage.material = _materialInstance;
            DisableAllEffects();
        }
    }

    public void Initialize(UniqueCard card, System.Action<UniqueCard> onClick)
    {
        _card = card;
        _onClick = onClick;

        if (_materialInstance == null)
        {
            _materialInstance = Instantiate(OGMaterial);
            cardImage.material = _materialInstance;
        }
        LoadCardSprite(card.Deck, card.CommonId);
        SetSelected(false);

        GetComponent<Button>().onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        _selected = !_selected;
        SetSelected(_selected);
        _onClick?.Invoke(_card);
    }

    private void SetSelected(bool selected)
    {
        if (_materialInstance == null) return;

        if (selected)
        {
            _materialInstance.EnableKeyword("GLOW_ON");
            _materialInstance.EnableKeyword("OUTBASE_ON");

            _materialInstance.SetFloat("_Glow", selectedGlowIntensity);
            _materialInstance.SetColor("_GlowColor", selectedGlowColor);

            _materialInstance.SetColor("_OutlineColor", selectedGlowColor);
            _materialInstance.SetFloat("_OutlineAlpha", outlineAlpha);
            _materialInstance.SetFloat("_OutlineGlow", outlineGlow);
            _materialInstance.SetFloat("_OutlineWidth", outlineWidth);
            _materialInstance.SetInt("_OutlinePixelWidth", outlinePixelWidth);
        }
        else
        {
            DisableAllEffects();
        }
    }

    private void DisableAllEffects()
    {
        if (_materialInstance == null) return;

        _materialInstance.DisableKeyword("GLOW_ON");
        _materialInstance.DisableKeyword("OUTBASE_ON");

        _materialInstance.SetFloat("_Glow", 0f);
        _materialInstance.SetFloat("_OutlineAlpha", 0f);
    }

    private void LoadCardSprite(PatronId deck, CardId cardId)
    {
        if (cardImage == null) return;

        string deckName = deck.ToString().Replace("_", string.Empty).ToLower().FirstCharacterToUpper();
        string cardName = cardId.ToString().ToLower().Replace("_", "-");
        string path = $"Sprites/Cards/{deckName}/{cardName}";
        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogError($"Nie znaleziono sprite'a dla ścieżki: {path}");
            return;
        }

        cardImage.sprite = sprite;
    }
} 
