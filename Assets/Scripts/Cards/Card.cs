using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using ScriptsOfTribute.Serializers;

public class Card : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public TextMeshPro hpText;

    public Vector3 DefaultScale => _scale;
    public Vector3 ShrinkedScale => _scale * 0.6f;

    private UniqueCard _card;
    [field: SerializeField]
    public int UniqueId { get; private set; }
    private bool _isAnimating = false;
    private bool _isVisible = true;
    private Sprite _originalSprite;
    private Vector3 _scale;

    [SerializeField] private ZoneType _zoneType;
    [SerializeField] private ZoneSide _zoneSide;
    public ZoneType ZoneType => _zoneType;
    public ZoneSide ZoneSide => _zoneSide;
    [SerializeField] private Sprite backSprite;
    [SerializeField] private GameObject agentActivateEffectPrefab;
    private GameObject activeGlowEffect;
    private Vector3 _layoutPosition;
    public void SetLayoutPosition(Vector3 pos) => _layoutPosition = pos;
    public Vector3 GetLayoutPosition() => _layoutPosition;

    public void Start()
    {
        _scale = gameObject.transform.localScale;
    }

    public UniqueCard GetCard()
    {
        return _card;
    }

    public void SetAnimating(bool value)
    {
        _isAnimating = value;
    }

    public bool IsAnimating()
    {
        return _isAnimating;
    }

    public void SetVisible(bool visible)
    {
        _isVisible = visible;
        spriteRenderer.sprite = visible ? _originalSprite : backSprite;
        hpText.enabled = visible;
    }

    public bool IsVisible()
    {
        return _isVisible;
    }

    public void InitializeCard(UniqueCard card)
    {
        _card = card;
        UniqueId = card.UniqueId.Value;
        gameObject.name = card.Name;
        LoadCardSprite(card.Deck, card.CommonId);
        ClearTextFields();
        if (_card.HP > 0)
        {
            hpText.SetText(_card.HP.ToString());
        }
    }

    public void UpdateAgentHealth(SerializedAgent agent)
    {
        if (_zoneType == ZoneType.Agents)
        {
            hpText.SetText(agent.CurrentHp.ToString());
        }
    }

    private void LoadCardSprite(PatronId deck, CardId cardId)
    {
        string deckName = deck.ToString().Replace("_", string.Empty).ToLower().FirstCharacterToUpper();
        string cardName = cardId.ToString().ToLower().Replace("_", "-");
        string path = $"Sprites/Cards/{deckName}/{cardName}";
        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogError($"Nie znaleziono sprite'a dla ścieżki: {path}");
        }
        spriteRenderer.sprite = sprite;
        _originalSprite = sprite;
    }

    private void ClearTextFields()
    {
        hpText.SetText("");
    }

    public void SetZoneInfo(ZoneType type, ZoneSide side)
    {
        _zoneType = type;
        _zoneSide = side;
    }

    public bool IsInPlayerHand()
    {
        return _zoneType == ZoneType.Hand && _zoneSide == ZoneSide.Player1;
    }

    public bool IsInEnemyHand()
    {
        return _zoneType == ZoneType.Hand && _zoneSide == ZoneSide.Player2;
    }

    public bool IsInTavern()
    {
        return _zoneType == ZoneType.Tavern || _zoneType == ZoneType.TavernAvailable;
    }

    public bool IsInZone(ZoneType type, ZoneSide side)
    {
        return _zoneType == type && _zoneSide == side;
    }

    public bool ShouldBeVisible()
    {
        return _zoneType == ZoneType.TavernAvailable ||
                IsInPlayerHand() ||
                (_zoneType == ZoneType.PlayedPile && _zoneSide == ZoneSide.Player1) ||
                (_zoneType == ZoneType.CooldownPile && _zoneSide == ZoneSide.Player1) ||
                (_zoneType == ZoneType.Agents);
    }

    public void ShowActivationEffect()
    {
        if (activeGlowEffect != null)
        {
            activeGlowEffect.SetActive(true);
            return;
        }
        activeGlowEffect = Instantiate(agentActivateEffectPrefab, transform);
        var renderer = activeGlowEffect.GetComponent<ParticleSystemRenderer>();
        if (renderer != null) renderer.enabled = true;
        activeGlowEffect.SetActive(true);
        activeGlowEffect.transform.localPosition = Vector3.zero;
    }

    public void RemoveActivationEffect()
    {
        activeGlowEffect.SetActive(false);
    }
}
