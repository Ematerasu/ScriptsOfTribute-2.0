using System.Collections;
using ScriptsOfTribute;
using UnityEngine;

public abstract class VisualCommand
{
    public abstract IEnumerator Execute();
}

public class MoveCardCommand : VisualCommand
{
    private UniqueId _cardId;
    private ZoneType _zone;
    private ZoneSide _side;

    public MoveCardCommand(UniqueId cardId, ZoneType zone, ZoneSide side)
    {
        _cardId = cardId;
        _zone = zone;
        _side = side;
    }

    public override IEnumerator Execute()
    {
        if (_zone == ZoneType.TavernAvailable)
        {
            yield return BoardManager.Instance.AnimateAddCardToTavernDelayed(_cardId);
        }
        else
        {
            Transform target = BoardManager.Instance.GetZoneTransform(_zone, _side);
            BoardManager.Instance.MoveCardToZone(_cardId, target, _zone, _side, () =>
            {
                if (_zone == ZoneType.Hand)
                    BoardManager.Instance.ArrangeCardsInHand(target);
                else if (_zone == ZoneType.Agents)
                    BoardManager.Instance.ArrangeAgentsInZone(target);
            });
            yield return new WaitForSeconds(0.15f);
        }
    }
}

public class PlayProjectileCommand : VisualCommand
{
    private UniqueId _targetCardId;
    private ZoneSide _side;

    public PlayProjectileCommand(UniqueId targetCardId, ZoneSide side)
    {
        _targetCardId = targetCardId;
        _side = side;
    }

    public override IEnumerator Execute()
    {
        if (!BoardManager.Instance.HasCardObject(_targetCardId))
            yield break;

        Vector3 targetPos = BoardManager.Instance.GetCardObject(_targetCardId).transform.position;
        bool finished = false;
        VisualEffectsManager.Instance.PlayPowerAttackEffect(targetPos, _side, () => finished = true);
        yield return new WaitUntil(() => finished);
        yield return new WaitForSeconds(0.2f);
    }
}

public class PlayHealingEffectCommand : VisualCommand
{
    private UniqueId _targetCardId;
    private ZoneSide _side;

    public PlayHealingEffectCommand(UniqueId targetCardId, ZoneSide side)
    {
        _targetCardId = targetCardId;
        _side = side;
    }

    public override IEnumerator Execute()
    {
        if (!BoardManager.Instance.HasCardObject(_targetCardId))
            yield break;

        bool finished = false;
        VisualEffectsManager.Instance.PlayHealingEffect(BoardManager.Instance.GetCardObject(_targetCardId), _side, () => finished = true);
        yield return new WaitUntil(() => finished);
    }
}

public class ShowAgentActivationCommand : VisualCommand
{
    private GameObject _cardObject;
    private bool _activate;

    public ShowAgentActivationCommand(GameObject cardObject, bool activate)
    {
        _cardObject = cardObject;
        _activate = activate;
    }

    public override IEnumerator Execute()
    {
        var card = _cardObject.GetComponent<Card>();
        if (card != null)
        {
            if (_activate)
                card.ShowActivationEffect();
            else
                card.RemoveActivationEffect();
        }
        yield return null;
    }
}
