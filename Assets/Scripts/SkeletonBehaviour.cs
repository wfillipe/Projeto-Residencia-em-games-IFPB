using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D), typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D), typeof(Animator))]
public sealed class SkeletonBehaviour : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int Dead = Animator.StringToHash("Dead");

    [SerializeField, Min(0.1f)] private float detectionRange = 4f;
    [SerializeField, Min(0.1f)] private float attackStartDistance = 1f;
    [SerializeField] private BoxCollider2D attackArea;
    [SerializeField] private LayerMask hitLayers = ~0;
    [SerializeField, Min(0f)] private float moveSpeed = 1.5f;
    [SerializeField, Min(1)] private int attackDamage = 2;
    [SerializeField, Min(0f)] private float attackCooldown = 0.6f;

    private readonly List<Collider2D> hits = new(8);
    private Health health;
    private DamageReaction reaction;
    private Health player;
    private Rigidbody2D body;
    private SpriteRenderer visual;
    private Animator animator;
    private float nextAttack;
    private bool attacking;
    private bool dead;
    private bool hitApplied;

    private void Awake()
    {
        health = GetComponent<Health>();
        reaction = GetComponent<DamageReaction>();
        body = GetComponent<Rigidbody2D>();
        visual = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        GameObject target = GameObject.FindGameObjectWithTag("Player");
        if (target != null)
            player = target.GetComponent<Health>();
    }

    private void Update()
    {
        // Pausa o clipe durante o recuo, inclusive antes do impacto do golpe.
        animator.speed = reaction != null && reaction.IsRecoiling ? 0f : 1f;
    }

    private void FixedUpdate()
    {
        if (dead || (reaction != null && reaction.IsRecoiling))
            return;

        body.linearVelocity = Vector2.zero;
        animator.SetBool(IsMoving, false);

        if (!health.IsAlive)
        {
            dead = true;
            body.simulated = false;
            animator.SetBool(IsAttacking, false);
            animator.SetBool(Dead, true);
            return;
        }

        if (attacking)
        {
            // O tempo de recuperação começa quando o clipe termina.
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsTag("Attack") && state.normalizedTime >= 1f)
            {
                attacking = false;
                animator.SetBool(IsAttacking, false);
                nextAttack = Time.time + attackCooldown;
            }
            return;
        }

        if (player == null || !player.IsAlive)
            return;

        Vector2 direction = (Vector2)player.transform.position - body.position;
        float distanceSquared = direction.sqrMagnitude;
        if (distanceSquared > detectionRange * detectionRange)
            return;

        if (Mathf.Abs(direction.x) > 0.01f)
            visual.flipX = direction.x < 0f;

        if (distanceSquared <= attackStartDistance * attackStartDistance)
        {
            if (Time.time >= nextAttack && attackArea != null)
            {
                // Mantém a direção do golpe até o quadro de impacto.
                if (distanceSquared > 0f)
                    AttackArea.Aim(attackArea, direction);
                attacking = true;
                hitApplied = false;
                animator.SetBool(IsAttacking, true);
            }
            return;
        }

        body.linearVelocity = direction.normalized * moveSpeed;
        animator.SetBool(IsMoving, true);
    }

    // Animation Event do clipe Attack. O jogador ainda precisa estar dentro da caixa.
    public void ApplySkeletonHit()
    {
        if (!isActiveAndEnabled || !attacking || hitApplied || !health.IsAlive ||
            player == null || !player.IsAlive || attackArea == null)
            return;

        hitApplied = true;
        AttackArea.Collect(attackArea, hitLayers, hits);
        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit.GetComponentInParent<Health>() == player)
            {
                player.TakeDamage(attackDamage, body.position);
                break;
            }
        }
    }

    private void OnDisable()
    {
        if (body != null)
            body.linearVelocity = Vector2.zero;
        if (animator != null)
            animator.speed = 1f;
    }

    private void OnDrawGizmosSelected() => AttackArea.Draw(attackArea);
}
