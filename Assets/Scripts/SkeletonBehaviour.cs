using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D), typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public sealed class SkeletonBehaviour : MonoBehaviour
{
    private enum State { Idle, Chase, Attack, Dead }

    [SerializeField, Min(0.1f)] private float detectionRange = 4f;
    [SerializeField, Min(0.1f)] private float attackStartDistance = 1f;
    [SerializeField] private BoxCollider2D attackArea;
    [SerializeField] private LayerMask hitLayers = ~0;
    [SerializeField, Min(0f)] private float moveSpeed = 1.5f;
    [SerializeField, Min(1)] private int attackDamage = 2;
    [SerializeField, Min(0.01f)] private float attackDuration = 0.65f;
    [SerializeField, Range(0f, 1f)] private float impactPoint = 0.6f;
    [SerializeField, Min(0f)] private float attackCooldown = 0.6f;
    [SerializeField, Min(1f)] private float framesPerSecond = 10f;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] walkFrames;
    [SerializeField] private Sprite[] attackFrames;
    [SerializeField] private Sprite[] deathFrames;

    private readonly List<Collider2D> hits = new(8);
    private Health health;
    private DamageReaction reaction;
    private Health player;
    private Rigidbody2D body;
    private SpriteRenderer visual;
    private State state;
    private float elapsed;
    private float nextAttack;
    private bool hitApplied;

    private void Awake()
    {
        health = GetComponent<Health>();
        reaction = GetComponent<DamageReaction>();
        body = GetComponent<Rigidbody2D>();
        visual = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        GameObject target = GameObject.FindGameObjectWithTag("Player");
        if (target != null)
            player = target.GetComponent<Health>();
    }

    private void FixedUpdate()
    {
        if (reaction != null && reaction.IsRecoiling)
            return;

        if (state == State.Dead)
            return;

        if (!health.IsAlive)
        {
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
            ChangeState(State.Dead);
            return;
        }

        Vector2 toPlayer = player != null ? (Vector2)player.transform.position - body.position : Vector2.zero;
        float distanceSquared = toPlayer.sqrMagnitude;
        bool hasTarget = player != null && player.IsAlive;
        body.linearVelocity = Vector2.zero;

        if (state == State.Attack)
        {
            // O impacto sÃ³ acerta se o jogador ainda estiver ao alcance.
            if (!hitApplied && elapsed >= attackDuration * impactPoint)
            {
                hitApplied = true;
                if (hasTarget && CanHitPlayer())
                    player.TakeDamage(attackDamage, body.position);
            }
            if (elapsed >= attackDuration)
            {
                nextAttack = Time.time + attackCooldown;
                ChangeState(State.Idle);
            }
            return;
        }

        if (!hasTarget || distanceSquared > detectionRange * detectionRange)
        {
            ChangeState(State.Idle);
            return;
        }

        if (Mathf.Abs(toPlayer.x) > 0.01f)
            visual.flipX = toPlayer.x < 0f;

        if (attackArea != null && distanceSquared > 0f)
            AttackArea.Aim(attackArea, toPlayer);

        if (distanceSquared <= attackStartDistance * attackStartDistance)
        {
            if (Time.time >= nextAttack)
            {
                hitApplied = false;
                ChangeState(State.Attack);
            }
            else
                ChangeState(State.Idle);
            return;
        }

        ChangeState(State.Chase);
        body.linearVelocity = toPlayer.normalized * moveSpeed;
    }

    private bool CanHitPlayer()
    {
        if (attackArea == null)
            return false;

        AttackArea.Collect(attackArea, hitLayers, hits);
        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit.GetComponentInParent<Health>() == player)
                return true;
        }
        return false;
    }

    private void Update()
    {
        // MantÃ©m a duraÃ§Ã£o do ataque independente das folhas de sprites.
        if (reaction == null || !reaction.IsRecoiling)
            elapsed += Time.deltaTime;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        Sprite[] frames = state switch
        {
            State.Chase => walkFrames,
            State.Attack => attackFrames,
            State.Dead => deathFrames,
            _ => idleFrames
        };

        if (frames == null || frames.Length == 0)
            return;

        int frame = state == State.Attack ? Mathf.FloorToInt(elapsed / attackDuration * frames.Length)
                                         : Mathf.FloorToInt(elapsed * framesPerSecond);
        frame = state is State.Attack or State.Dead ? Mathf.Min(frame, frames.Length - 1) : frame % frames.Length;
        visual.sprite = frames[frame];
    }

    private void ChangeState(State next)
    {
        if (state == next)
            return;

        state = next;
        elapsed = 0f;
    }

    private void OnDisable()
    {
        if (body != null)
            body.linearVelocity = Vector2.zero;
    }
    private void OnDrawGizmosSelected() => AttackArea.Draw(attackArea);
}
