using System;
using UnityEngine;
public class VisualEffectsManager : MonoBehaviour
{
    public static VisualEffectsManager Instance { get; private set; }
    [SerializeField] private GameObject powerProjectilePrefab;
    [SerializeField] private Transform powerOriginPointPlayer1;
    [SerializeField] private Transform powerOriginPointPlayer2;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayPowerAttackEffect(Vector3 targetPosition, ZoneSide side, Action? onComplete = null)
    {
        Transform powerOrigin = side == ZoneSide.HumanPlayer ? powerOriginPointPlayer2 : powerOriginPointPlayer1;
        GameObject projectile = Instantiate(powerProjectilePrefab, powerOrigin.position, Quaternion.identity, transform);
        ProjectileMoveScript moveScript = projectile.GetComponent<ProjectileMoveScript>();
        if (moveScript != null)
        {
            Vector3 direction = (targetPosition - powerOrigin.position).normalized;
            projectile.transform.forward = direction;
            moveScript.fireRate = 1;
            moveScript.speed = 15f;
            moveScript.accuracy = 100f;
            moveScript.rotate = true;
            moveScript.bounce = false;
            AudioManager.Instance.PlayProjectileSound();
            moveScript.SetTargetPosition(targetPosition, onComplete);
        }
    }

    public void PlayHealingEffect(GameObject agentObj, ZoneSide side, Action? onComplete = null)
    {

    }
}
