using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerCombat), typeof(Rigidbody2D))]
public sealed class PlayerAttackDamage : MonoBehaviour
{
    [SerializeField, Min(1)] private int damage = 1;
    [SerializeField] private BoxCollider2D attackArea;
    [SerializeField] private BoxCollider2D rightAttackArea;
    [SerializeField] private BoxCollider2D leftAttackArea;
    [SerializeField] private LayerMask hitLayers = ~0;

    // Reutiliza os resultados e impede dano duplicado em alvos com vÃ¡rios colisores.
    private readonly List<Collider2D> hitColliders = new(8);
    private readonly HashSet<IPickaxeDamageable> hitTargets = new();

    private PlayerCombat combat;
    private Rigidbody2D body;
    private Health health;
    private bool hitApplied;
    private BoxCollider2D activeAttackArea;

    internal bool HasArea => attackArea != null;

    private void Awake()
    {
        combat = GetComponent<PlayerCombat>();
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
    }

    internal void PrepareAttack(Vector2 direction)
    {
        hitApplied = false;
        // Guarda a caixa do golpe até o evento de impacto da animação.
        activeAttackArea = attackArea;
        if (direction.x > 0f && rightAttackArea != null)
            activeAttackArea = rightAttackArea;
        else if (direction.x < 0f && leftAttackArea != null)
            activeAttackArea = leftAttackArea;

        AttackArea.Aim(activeAttackArea, direction);
    }

    // Chamado por um evento no quadro de impacto de cada clipe.
    public void ApplyPickaxeHit()
    {
        if (!isActiveAndEnabled || activeAttackArea == null || !combat.IsAttacking || hitApplied || (health != null && !health.IsAlive))
            return;

        hitApplied = true;
        hitTargets.Clear();
        AttackArea.Collect(activeAttackArea, hitLayers, hitColliders);
        foreach (Collider2D hit in hitColliders)
        {
            if (hit == null || hit.attachedRigidbody == body)
                continue;

            var target = hit.GetComponentInParent<IPickaxeDamageable>();
            if (target == null || !hitTargets.Add(target))
                continue;
            if (target is Health targetHealth)
                targetHealth.TakeDamage(damage, body.position);
            else
                target.TakePickaxeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        AttackArea.Draw(attackArea);
        AttackArea.Draw(rightAttackArea);
        AttackArea.Draw(leftAttackArea);
    }
}
