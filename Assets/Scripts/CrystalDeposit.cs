using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public sealed class CrystalDeposit : MonoBehaviour, IPickaxeDamageable
{
    [SerializeField, Range(1, 3), Tooltip("Nível exato da picareta para minerar este cristal.")]
    private int tier = 1;
    [SerializeField, HideInInspector] private string depositId;
    [SerializeField, Min(1)] private int hitsToMine = 2;
    [SerializeField, Min(1)] private int oreYield = 1;
    [SerializeField, Min(0.01f)] private float flashDuration = 0.12f;
    [SerializeField] private Material flashMaterial;

    private SpriteRenderer visual;
    private BoxCollider2D footprint;
    private Material originalMaterial;
    private Coroutine flash;
    private int hitsRemaining;
    private string runtimeDepositId;

    private void Awake()
    {
        visual = GetComponent<SpriteRenderer>();
        footprint = GetComponent<BoxCollider2D>();
        originalMaterial = visual.sharedMaterial;
        hitsRemaining = Mathf.Max(1, hitsToMine);
        gameObject.tag = $"OreTier{tier}";
        runtimeDepositId = BuildDepositId();
        if (OreInventory.WasMined(runtimeDepositId))
            gameObject.SetActive(false);
    }

    private string BuildDepositId()
    {
        // Cada cópia recebe sua própria identidade ao recarregar o mapa.
        string hierarchyPath = "";
        for (Transform node = transform; node != null; node = node.parent)
            hierarchyPath = $"/{node.GetSiblingIndex()}{hierarchyPath}";

        return $"{gameObject.scene.path}:{hierarchyPath}:{depositId}";
    }

    public void TakePickaxeDamage(int damage)
    {
        if (!isActiveAndEnabled || hitsRemaining <= 0 || damage <= 0 || OreInventory.PickaxeLevel != tier)
            return;

        // Mineração conta golpes da picareta. Cada golpe vale um acerto.
        hitsRemaining--;
        bool mined = hitsRemaining == 0;
        if (mined)
        {
            footprint.enabled = false;
            OreInventory.Collect(runtimeDepositId, tier, oreYield);
        }

        if (flash != null)
            StopCoroutine(flash);
        flash = StartCoroutine(Flash(mined));
    }

    private IEnumerator Flash(bool mined)
    {
        if (flashMaterial != null)
            visual.sharedMaterial = flashMaterial;

        yield return new WaitForSeconds(flashDuration);

        visual.sharedMaterial = originalMaterial;
        flash = null;
        if (mined)
            gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (flash == null)
            return;

        StopCoroutine(flash);
        flash = null;
        visual.sharedMaterial = originalMaterial;
    }
}
