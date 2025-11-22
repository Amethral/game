using FishNet.Object;
using FishNet.Component.Animating;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerStats))]
public class PlayerCombat : NetworkBehaviour
{
    [Header("Combat Settings")]
    public float attackStaminaCost = 25f;
    public float attackRange = 1.5f;
    public int damageAmount = 20;
    public LayerMask enemyLayer; // Assign "Player" or "Enemy" layer here

    [Header("Visuals")]
    public Transform damagePoint; // Create an Empty GameObject at the tip of your dagger and drag it here
    public NetworkAnimator networkAnimator;

    // Dependencies
    private PlayerStats _playerStats;
    private PlayerControls _playerControls;
    private PlayerMovement _playerMovement;

    // State
    private bool _isAttacking = false;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        _playerControls = new PlayerControls();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable() => _playerControls.Enable();
    private void OnDisable() => _playerControls.Disable();

    private void Update()
    {
        if (!IsOwner) return;

        // Check for Attack Input
        // Assuming you added an "Attack" action to your PlayerControls Input Action asset
        if (_playerControls.Player.Attack.WasPressedThisFrame())
        {
            AttemptAttack();
        }
    }

    private void AttemptAttack()
    {
        // 1. Check if we are already attacking (prevent spamming)
        if (_isAttacking) return;

        // 2. Check Stamina (Souls-like requirement)
        if (_playerStats.Stamina.Value < attackStaminaCost) return;

        _playerMovement.CanMove = false;

        // 3. Consume Stamina
        _playerStats.CmdUseStamina(attackStaminaCost);

        // 4. Start Animation
        networkAnimator.SetTrigger("Attack");

        // 5. Lock logic (Optional: Disable movement here if you want hard-lock)
        StartCoroutine(AttackCooldownRoutine());
    }

    private IEnumerator AttackCooldownRoutine()
    {
        _isAttacking = true;
        // Wait for rough length of animation before allowing another attack
        yield return new WaitForSeconds(0.8f);
        _isAttacking = false;

        _playerMovement.CanMove = true;
    }

    // ---------------------------------------------------------
    // CALLED BY ANIMATION EVENT
    // ---------------------------------------------------------
    public void OnAttackHit()
    {
        if (!IsOwner) return;

        // Create a hitbox check exactly when the animation event fires
        Collider[] hitEnemies = Physics.OverlapSphere(damagePoint.position, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            // Check if we hit a valid target
            if (enemy.TryGetComponent(out NetworkObject netObj))
            {
                // Don't hit yourself
                if (netObj == base.NetworkObject) continue;

                // Deal Damage via Server
                CmdDealDamage(netObj);
            }
        }
    }

    [ServerRpc]
    private void CmdDealDamage(NetworkObject target)
    {
        // Validation: Ensure target actually has PlayerStats
        if (target.TryGetComponent(out PlayerStats targetStats))
        {
            // Call the function you already wrote in PlayerStats
            targetStats.TakeDamage(damageAmount);

            Debug.Log($"Hit registered on {target.name}");
        }
    }

    // Debug Visualizer for the Hitbox
    private void OnDrawGizmosSelected()
    {
        if (damagePoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(damagePoint.position, attackRange);
    }
}