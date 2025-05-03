using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardLayoutManager : MonoBehaviour
{
    public static CardLayoutManager Instance { get; private set; }

    private readonly List<(ZoneType, ZoneSide)> pendingLayouts = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ScheduleLayout(ZoneType zone, ZoneSide side)
    {
        if (zone != ZoneType.Hand && zone != ZoneType.Agents) return;
        pendingLayouts.Add((zone, side));
    }

    private void LateUpdate()
    {
        foreach (var (zone, side) in pendingLayouts)
        {
            switch (zone)
            {
                case ZoneType.Hand:
                    ArrangeHand(side);
                    break;
                case ZoneType.Agents:
                    ArrangeAgents(side);
                    break;
            }
        }
        pendingLayouts.Clear();
    }

    private void ArrangeHand(ZoneSide side)
    {
        Transform handTransform = BoardManager.Instance.GetZoneTransform(ZoneType.Hand, side);
        var cards = handTransform
            .GetComponentsInChildren<Card>()
            .Where(c => c.transform.parent == handTransform && !c.IsAnimating())
            .ToList();

        float spacing = 0.8f;
        float totalWidth = (cards.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            Vector3 pos = new Vector3(startX + i * spacing, 0f, 0f);
            LeanTween.moveLocal(card.gameObject, pos, 0.15f).setEase(LeanTweenType.easeOutSine);
            card.SetLayoutPosition(pos);

            var cardLayout = card.GetComponent<CardLayoutBehaviour>();
            if (cardLayout != null)
                cardLayout.SetSortingOrder(i+1);
        }
    }

    private void ArrangeAgents(ZoneSide side)
    {
        Transform zoneTransform = BoardManager.Instance.GetZoneTransform(ZoneType.Agents, side);
        var cards = zoneTransform
            .GetComponentsInChildren<Card>()
            .Where(c => c.transform.parent == zoneTransform && !c.IsAnimating())
            .ToList();

        if (cards.Count == 0)
            return;

        float cardWidth = 1.2f;
        float gap = cardWidth;
        int overlapStartAt = 6;
        float overlapFactor = 0.65f;
        float minOverlapFactor = 0.25f;

        if (cards.Count >= overlapStartAt)
            gap = cardWidth * overlapFactor;

        var box = zoneTransform.GetComponent<BoxCollider2D>();
        if (box != null)
        {
            float zoneWidth = box.size.x;
            float targetWidth = (cards.Count - 1) * gap;

            if (targetWidth > zoneWidth)
            {
                float minGap = cardWidth * minOverlapFactor;
                gap = Mathf.Max(minGap, zoneWidth / Mathf.Max(1, cards.Count - 1));
            }
        }

        float startX = -(cards.Count - 1) * gap * 0.5f;

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            Vector3 pos = new Vector3(startX + i * gap, 0f, -0.01f * i);
            LeanTween.moveLocal(card.gameObject, pos, 0.15f).setEase(LeanTweenType.easeOutSine);
            card.SetLayoutPosition(pos);

            var cardLayout = card.GetComponent<CardLayoutBehaviour>();
            if (cardLayout != null)
                cardLayout.SetSortingOrder(i+1);
        }
    }
}
