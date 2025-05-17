using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EffectPopupManager : MonoBehaviour
{
    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private float popupLifetime = 1.3f;
    [SerializeField] private float delayBetweenBatches = 0.2f;
    [SerializeField] private float delayBetweenPopup = 0.01f;
    [SerializeField] private Transform playerContainer;
    [SerializeField] private Transform enemyContainer;

    private Queue<(List<string> texts, ZoneSide side)> _batches = new();
    private bool _isShowing = false;

    public void EnqueuePopupBatch(List<string> texts, ZoneSide side)
    {
        _batches.Enqueue((texts, side));
        if (!_isShowing)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        _isShowing = true;

        while (_batches.Count > 0)
        {
            var (batch, side) = _batches.Dequeue();
            var activePopups = new List<PopupEffect>();

            var parent = side == ZoneSide.HumanPlayer ? playerContainer : enemyContainer;

            foreach (var text in batch)
            {
                var go = Instantiate(popupPrefab, parent);
                var popup = go.GetComponent<PopupEffect>();
                popup.Setup(text);
                activePopups.Add(popup);
            }

            foreach (var popup in activePopups)
            {
                StartCoroutine(popup.PlayQueuedAnimation(popupLifetime));
                yield return new WaitForSeconds(delayBetweenPopup);
            }
                

            yield return new WaitForSeconds(popupLifetime + delayBetweenBatches);
        }

        _isShowing = false;
    }
}
