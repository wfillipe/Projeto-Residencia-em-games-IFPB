using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IPickaxeDamageable
{
    void TakePickaxeDamage(int damage);
}

[DefaultExecutionOrder(10)]
[RequireComponent(typeof(PlayerController), typeof(Animator), typeof(Rigidbody2D))]
public sealed class PlayerCombat : MonoBehaviour
{
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int AttackFacing = Animator.StringToHash("AttackFacing");

    [SerializeField, Min(1)] private int damage = 1;
    [SerializeField, Min(0.1f)] private float reach = 0.85f;
    [SerializeField] private LayerMask hitLayers = ~0;

    // Reutiliza os resultados e impede dano duplicado em alvos com vários colisores.
    private readonly List<Collider2D> hitColliders = new(8);
    private readonly HashSet<IPickaxeDamageable> hitTargets = new();

    private PlayerController movement;
    private Animator animator;
    private Rigidbody2D body;
    private Vector2 attackDirection = Vector2.down;
    private bool attackRequested;
    private bool hitApplied;

    public bool IsAttacking => attackRequested ||
        (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));

    private void Awake()
    {
        movement = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!AttackPressed() || !animator.isActiveAndEnabled || IsAttacking)
            return;

        attackDirection = movement.AttackDirection;
        movement.SetSpriteFacing(attackDirection.x < 0f);
        body.linearVelocity = Vector2.zero;
        animator.SetFloat(AttackFacing, attackDirection.x != 0f ? 2f : attackDirection.y > 0f ? 1f : 0f);
        hitApplied = false;
        attackRequested = true;
        animator.SetTrigger(Attack);
    }

    private void LateUpdate()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            attackRequested = false;
    }

    private void OnDisable()
    {
        attackRequested = false;
        hitApplied = false;
        if (animator != null && animator.isActiveAndEnabled)
            animator.ResetTrigger(Attack);
    }

    // Chamado por um evento no quadro de impacto de cada clipe.
    public void ApplyPickaxeHit()
    {
        if (!isActiveAndEnabled || hitApplied)
            return;

        hitApplied = true;
        var filter = new ContactFilter2D { useTriggers = Physics2D.queriesHitTriggers };
        filter.SetLayerMask(hitLayers);
        Vector2 center = body.position + attackDirection * (reach * 0.5f);
        float angle = attackDirection.x != 0f ? 0f : 90f;
        Physics2D.OverlapBox(center, new Vector2(reach, 0.85f), angle, filter, hitColliders);
        foreach (Collider2D hit in hitColliders)
        {
            if (hit == null || hit.attachedRigidbody == body)
                continue;

            var target = hit.GetComponentInParent<IPickaxeDamageable>();
            if (target != null && hitTargets.Add(target))
                target.TakePickaxeDamage(damage);
        }
    }

    private static bool AttackPressed()
    {
        return Keyboard.current?.spaceKey.wasPressedThisFrame == true ||
               Mouse.current?.leftButton.wasPressedThisFrame == true ||
               Gamepad.current?.buttonWest.wasPressedThisFrame == true;
    }
}
