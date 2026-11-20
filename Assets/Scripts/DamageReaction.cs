using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D))]
public sealed class DamageReaction : MonoBehaviour
{
    [SerializeField, Min(0f)] private float knockbackSpeed = 5f;
    [SerializeField, Min(0f)] private float knockbackDuration = 0.16f;
    [SerializeField, Min(0f)] private float flashDuration = 0.12f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private SpriteRenderer[] renderers;

    private static readonly int FlashColor = Shader.PropertyToID("_FlashColor");
    private Health health;
    private Rigidbody2D body;
    private Material[] originalMaterials;
    private Material instance;
    private float knockbackUntil;
    private float flashUntil;
    private bool flashing;

    public bool IsRecoiling => isActiveAndEnabled && Time.time < knockbackUntil;

    private void Awake()
    {
        health = GetComponent<Health>();
        body = GetComponent<Rigidbody2D>();
        originalMaterials = new Material[renderers.Length];
    }

    private void OnEnable() => health.Hit += React;

    private void OnDisable()
    {
        health.Hit -= React;
        RestoreMaterials();
        knockbackUntil = 0;
    }

    private void React(Vector2? source)
    {
        // A velocidade inicial vem da direção do golpe. A física continua resolvendo colisões.
        if (source.HasValue && knockbackSpeed > 0f && knockbackDuration > 0f)
        {
            Vector2 away = (body.position - source.Value).normalized;
            body.linearVelocity = away * knockbackSpeed;
            knockbackUntil = Time.time + knockbackDuration;
        }

        if (flashMaterial == null || flashDuration <= 0f)
            return;

        if (instance == null)
            instance = new Material(flashMaterial);
        instance.SetColor(FlashColor, flashColor);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;
            if (!flashing)
                originalMaterials[i] = renderers[i].sharedMaterial;
            renderers[i].sharedMaterial = instance;
        }
        flashing = true;
        flashUntil = Time.time + flashDuration;
    }

    private void LateUpdate()
    {
        if (flashing && Time.time >= flashUntil)
            RestoreMaterials();
    }

    private void RestoreMaterials()
    {
        if (!flashing)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].sharedMaterial = originalMaterials[i];
        }
        flashing = false;
    }

    private void OnDestroy()
    {
        if (instance != null)
            Destroy(instance);
    }
}
