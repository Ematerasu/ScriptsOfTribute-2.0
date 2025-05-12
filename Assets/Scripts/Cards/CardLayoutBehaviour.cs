using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class CardLayoutBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float playerHandScale = 1.0f;
    [SerializeField] private float playerPlayedScale = 0.7f;
    [SerializeField] private float playerAgentScale = 0.8f;
    [SerializeField] private float enemyHandScale =  0.65f;
    [SerializeField] private float enemyPlayedScale = 0.5f;
    [SerializeField] private float enemyAgentScale = 0.575f;
    private Vector3 defaultScale;
    private int _originalSortingOrder;
    private bool _isHovered;
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private TextMeshPro _hpText;

    [SerializeField] private float defaultPlayerRotation = 0f;
    [SerializeField] private float defaultEnemyRotation = 180f;

    [SerializeField] private float scaleFactor = 1.5f;
    [SerializeField] private int hoverSortingOrder = 100;

    private SortingGroup sortingGroup;

    private Card _card;

    private void Start()
    {
        _card = GetComponent<Card>();
        _isHovered = false;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        sortingGroup = GetComponent<SortingGroup>();

        if (_spriteRenderer != null)
        {
            _originalSortingOrder = _spriteRenderer.sortingOrder;
        }
        float scale = CardManager.Instance.GetDefaultScaleForDeck(_card.GetCard().Deck);
        defaultScale = Vector3.one * scale;
        ApplyLayout();
    }

    public void ApplyLayout()
    {
        if (_card == null) return;
        var zRotation = _card.ZoneSide == ZoneSide.EnemyPlayer ? defaultEnemyRotation : defaultPlayerRotation;
        switch (_card.ZoneType)
        {
            case ZoneType.Hand:
                LeanTween.scale(gameObject, defaultScale * (_card.ZoneSide == ZoneSide.HumanPlayer ? playerHandScale : enemyHandScale), 0.1f);
                transform.localRotation = Quaternion.Euler(0, 0, zRotation);
                break;

            case ZoneType.Tavern:
            case ZoneType.TavernAvailable:
                LeanTween.scale(gameObject, defaultScale, 0.1f);
                transform.localRotation = Quaternion.Euler(20f, 0, 0);
                break;

            case ZoneType.DrawPile:
            case ZoneType.CooldownPile:
            case ZoneType.PlayedPile:
                LeanTween.scale(gameObject, defaultScale * (_card.ZoneSide == ZoneSide.HumanPlayer ? playerPlayedScale : enemyPlayedScale), 0.1f);
                transform.localRotation = Quaternion.Euler(0, 0, zRotation);
                break;
            case ZoneType.Agents:
                LeanTween.scale(gameObject, defaultScale * (_card.ZoneSide == ZoneSide.HumanPlayer ? playerAgentScale : enemyAgentScale), 0.1f);
                transform.localRotation = Quaternion.Euler(0, 0, zRotation);
                break;
            default:
                transform.localScale = defaultScale;
                transform.localRotation = Quaternion.identity;
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isHovered || _card.IsAnimating()) return;

        _isHovered = true;
        LeanTween.scale(gameObject, transform.localScale * scaleFactor, 0.1f); 
        if (_card.IsInPlayerHand())
        {
            transform.localRotation = Quaternion.identity;
            Vector3 raised = _card.GetLayoutPosition() + new Vector3(0, 0.5f, 0);
            LeanTween.moveLocal(gameObject, raised, 0.1f);
        }
        else if (_card.IsInEnemyHand())
        {
            transform.localRotation = Quaternion.identity;
            Vector3 raised = _card.GetLayoutPosition() - new Vector3(0, 0.5f, 0);
            LeanTween.moveLocal(gameObject, raised, 0.1f);
        }
        SetSortingLayer("CardHover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isHovered || _card.IsAnimating()) return;
        _isHovered = false;

        ApplyLayout();

        if (_card.IsInPlayerHand() || _card.IsInEnemyHand())
        {
            transform.localRotation = Quaternion.Euler(0, 0, _card.ZoneSide == ZoneSide.EnemyPlayer ? defaultEnemyRotation : defaultPlayerRotation);
            LeanTween.moveLocal(gameObject, _card.GetLayoutPosition(), 0.1f).setOnComplete(() =>
            {
                ApplyLayout();
            });
        }
        else
        {
            ApplyLayout();
        }

        SetSortingLayer("Cards");
    }

    public void SetSortingOrder(int baseOrder)
    {
        if (sortingGroup != null)
            sortingGroup.sortingOrder = baseOrder;
    }

    public void SetSortingLayer(string layerName)
    {
        sortingGroup.sortingLayerName = layerName;
    }
}

