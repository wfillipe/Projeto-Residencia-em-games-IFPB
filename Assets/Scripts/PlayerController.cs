using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public sealed class PlayerController : MonoBehaviour
{
    // Os valores correspondem aos thresholds dos Blend Trees no Animator.
    private enum Facing { South = 0, North = 1, East = 2, NorthEast = 3, SouthEast = 4 }

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int FacingParameter = Animator.StringToHash("Facing");

    [SerializeField, Min(0f)] private float moveSpeed = 3f;
    [SerializeField] private SpriteRenderer bodySprite;
    [SerializeField] private SpriteRenderer hairSprite;

    private Rigidbody2D body;
    private Animator animator;
    private Vector2 direction;
    private Facing facing;
    private PlayerCombat combat;
    private bool lastMovementFlip;

    internal Vector2 AttackDirection { get; private set; } = Vector2.down;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        combat = GetComponent<PlayerCombat>();
    }

    private void Update()
    {
        if (combat != null && combat.IsAttacking)
            return;

        direction = ReadDirection(Keyboard.current);
        bool moving = direction != Vector2.zero;

        if (moving)
        {
            facing = GetFacing(direction);
            // Em uma diagonal, o golpe usa o eixo vertical.
            AttackDirection = Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                ? Vector2.right * Mathf.Sign(direction.x)
                : Vector2.up * Mathf.Sign(direction.y);
            bool flip = direction.x < 0f;

            // Mantém a orientação ajustada para a arte da diagonal de costas.
            if (facing == Facing.NorthEast)
                flip = !flip;

            lastMovementFlip = flip;
        }

        // O Animator decide as transições e seleciona o clipe em cada Blend Tree.
        SetSpriteFacing(lastMovementFlip);

        animator.SetFloat(FacingParameter, (int)facing);
        animator.SetBool(IsMoving, moving);
    }

    private void FixedUpdate()
    {
        body.linearVelocity = combat != null && combat.IsAttacking ? Vector2.zero : direction * moveSpeed;
    }

    private void OnDisable()
    {
        direction = Vector2.zero;
        body.linearVelocity = Vector2.zero;
        animator.SetBool(IsMoving, false);
    }

    internal void SetSpriteFacing(bool flip)
    {
        if (bodySprite != null && bodySprite.flipX != flip)
            bodySprite.flipX = flip;
        if (hairSprite != null && hairSprite.flipX != flip)
            hairSprite.flipX = flip;
    }

    private static Facing GetFacing(Vector2 direction)
    {
        if (direction.x == 0f)
            return direction.y > 0f ? Facing.North : Facing.South;

        if (direction.y == 0f)
            return Facing.East;

        return direction.y > 0f ? Facing.NorthEast : Facing.SouthEast;
    }

    private static Vector2 ReadDirection(Keyboard keys)
    {
        bool right = keys != null && (keys.dKey.isPressed || keys.rightArrowKey.isPressed);
        bool left = keys != null && (keys.aKey.isPressed || keys.leftArrowKey.isPressed);
        bool up = keys != null && (keys.wKey.isPressed || keys.upArrowKey.isPressed);
        bool down = keys != null && (keys.sKey.isPressed || keys.downArrowKey.isPressed);
        Vector2 keyboardDirection = new Vector2((right ? 1 : 0) - (left ? 1 : 0),
                           (up ? 1 : 0) - (down ? 1 : 0)).normalized;
        var gamepad = Gamepad.current;
        if (keyboardDirection != Vector2.zero || gamepad == null)
            return keyboardDirection;

        Vector2 pad = gamepad.dpad.ReadValue();
        if (pad == Vector2.zero)
            pad = gamepad.leftStick.ReadValue();
        if (pad.sqrMagnitude < 0.09f)
            return Vector2.zero;

        // Converte o analógico nas mesmas oito direções do teclado.
        const float diagonalThreshold = 0.38f;
        float absX = Mathf.Abs(pad.x);
        float absY = Mathf.Abs(pad.y);
        float x = absX >= diagonalThreshold * absY ? Mathf.Sign(pad.x) : 0f;
        float y = absY >= diagonalThreshold * absX ? Mathf.Sign(pad.y) : 0f;
        return new Vector2(x, y).normalized;
    }
}
