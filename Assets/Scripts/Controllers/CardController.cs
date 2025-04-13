using UnityEngine;
using UnityEngine.EventSystems;
public class CardController : MonoBehaviour, IPointerClickHandler
{    
    public Card card;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (card.IsAnimating() || !card.IsVisible()) return;
        if (!GameManager.Instance.IsHumanPlayersTurn) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            //Debug.Log($"Click on {card.GetCard().CommonId} {card.GetCard().UniqueId.Value}");
            if (card.IsInZone(ZoneType.Hand, ZoneSide.Player1))
            {   
                GameManager.Instance.PlayCard(card.GetCard(), ZoneSide.Player1);
            }
            else if (card.IsInZone(ZoneType.TavernAvailable, ZoneSide.Neutral))
            { 
                GameManager.Instance.BuyCard(card.GetCard(), ZoneSide.Player1);
            }
            else if (card.IsInZone(ZoneType.Agents, ZoneSide.Player1))
            { 
                GameManager.Instance.ActivateAgent(card.GetCard(), ZoneSide.Player1);
            }
            else if (card.IsInZone(ZoneType.Agents, ZoneSide.Player2))
            {
                GameManager.Instance.AttackAgent(card.GetCard(), ZoneSide.Player1);
            }
        }
    }
}
