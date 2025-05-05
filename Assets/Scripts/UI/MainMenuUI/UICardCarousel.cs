using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UICardCarouselSprite : MonoBehaviour
{
    [System.Serializable]
    public class CarouselSlot
    {
        public Vector3 position;
        public Vector3 scale;
        public float alpha;
        public int sortingOrder;
    }

    public List<GameObject> cards;
    public List<CarouselSlot> slots;  
    public List<Sprite> cardSprites;
    public float transitionTime = 0.5f;
    public float delayBetweenCycles = 2f;

    private Coroutine currentAnim;
    private int currentSpriteIndex = 0;

    private List<Vector3> startPos = new();
    private List<Vector3> startScale = new();
    private List<float> startAlpha = new();

    void Start()
    {
        ApplySlotMapping();
        StartCoroutine(AutoRotate());
    }

    IEnumerator AutoRotate()
    {
        while (true)
        {
            yield return new WaitForSeconds(delayBetweenCycles);
            ShiftLeft();
            SwapSprite();
            yield return AnimateTransition();
        }
    }

    void ShiftLeft()
    {
        var first = cards[0];
        cards.RemoveAt(0);
        cards.Add(first);
    }

    void SwapSprite()
    {
        var last = cards[4];
        last.GetComponent<Image>().sprite = cardSprites[currentSpriteIndex];
        currentSpriteIndex = (currentSpriteIndex + 1) % cardSprites.Count;
    }

    IEnumerator AnimateTransition()
    {
        float t = 0f;
        startPos.Clear();
        startScale.Clear();
        startAlpha.Clear();

        for (int i = 0; i < cards.Count; i++)
        {
            var rt = cards[i].GetComponent<RectTransform>();
            startPos.Add(rt.anchoredPosition3D);
            startScale.Add(rt.localScale);
            startAlpha.Add(rt.GetComponent<CanvasGroup>().alpha);
        }

        while (t < transitionTime)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, t / transitionTime);

            for (int i = 0; i < cards.Count && i < slots.Count; i++)
            {
                var rt = cards[i].GetComponent<RectTransform>();
                var cg = rt.GetComponent<CanvasGroup>();

                rt.anchoredPosition3D = Vector3.Lerp(startPos[i], slots[i].position, progress);
                rt.localScale = Vector3.Lerp(startScale[i], slots[i].scale, progress);
                cg.alpha = Mathf.Lerp(startAlpha[i], slots[i].alpha, progress);
            }

            yield return null;
        }

        ApplySlotMapping();
    }

    void ApplySlotMapping()
    {
        for (int i = 0; i < cards.Count && i < slots.Count; i++)
        {
            var rt = cards[i].GetComponent<RectTransform>();
            var cg = rt.GetComponent<CanvasGroup>();

            var slot = slots[i];

            rt.anchoredPosition3D = slot.position;
            rt.localScale = slot.scale;
            cg.alpha = slot.alpha;
            rt.SetSiblingIndex(slot.sortingOrder);
        }
    }
}
