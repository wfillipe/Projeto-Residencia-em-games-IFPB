using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

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

    private PlayerController movement;
    private Animator animator;
    private Rigidbody2D body;
    private Health health;
    private DamageReaction reaction;
    private Vector2 attackDirection = Vector2.down;
    private bool attackRequested;
    private PlayerAttackDamage attackDamage;
    private CinemachineImpulseSource impulseSource;

    public bool IsAttacking => attackRequested ||
        (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));

    private void Awake()
    {
        movement = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        reaction = GetComponent<DamageReaction>();
        attackDamage = GetComponent<PlayerAttackDamage>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f || attackDamage == null || !attackDamage.isActiveAndEnabled || !attackDamage.HasArea || (reaction != null && reaction.IsRecoiling) || (health != null && !health.IsAlive) || !AttackPressed() || !animator.isActiveAndEnabled || IsAttacking)
            return;

        attackDirection = movement.AttackDirection;
        movement.SetSpriteFacing(attackDirection.x < 0f);
        body.linearVelocity = Vector2.zero;
        attackDamage.PrepareAttack(attackDirection);
        animator.SetFloat(AttackFacing, attackDirection.x < 0f ? 3f : attackDirection.x > 0f ? 2f : attackDirection.y > 0f ? 1f : 0f);
        attackRequested = true;
        animator.SetTrigger(Attack);

        if (impulseSource != null)
            impulseSource.GenerateImpulse();
    }

    private void LateUpdate()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            attackRequested = false;
    }

    private void OnDisable()
    {
        attackRequested = false;
        if (animator != null && animator.isActiveAndEnabled)
            animator.ResetTrigger(Attack);
    }

    private static bool AttackPressed()
    {
        return Keyboard.current?.spaceKey.wasPressedThisFrame == true ||
               Mouse.current?.leftButton.wasPressedThisFrame == true ||
               Gamepad.current?.buttonWest.wasPressedThisFrame == true;
    }
}
