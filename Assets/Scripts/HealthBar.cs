using UnityEngine;

[RequireComponent(typeof(Health))]
public sealed class HealthBar : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new(0f, 1.45f);
    [SerializeField, Min(0.1f)] private float width = 0.9f;

    private static Sprite pixel;
    private Health health;
    private Transform fill;
    private GameObject bar;

    private void Awake() => health = GetComponent<Health>();
    private void OnEnable() => health.Damaged += Refresh;
    private void OnDisable() => health.Damaged -= Refresh;

    private void Refresh()
    {
        if (!health.IsAlive)
        {
            if (bar != null)
                bar.SetActive(false);
            return;
        }

        // A barra só é criada quando o personagem toma dano.
        if (bar == null)
        {
            if (pixel == null)
                pixel = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);

            bar = new GameObject("Health Bar");
            bar.transform.SetParent(transform, false);
            bar.transform.localPosition = offset;
            Transform background = AddPart("Background", new Color32(38, 27, 31, 255), 100);
            background.localScale = new Vector3(width + 0.08f, 0.16f, 1f);
            fill = AddPart("Fill", new Color32(225, 69, 67, 255), 101);
        }

        float filledWidth = width * health.Current / health.Max;
        fill.localScale = new Vector3(filledWidth, 0.08f, 1f);
        fill.localPosition = new Vector3((filledWidth - width) * 0.5f, 0f, 0f);
    }

    private Transform AddPart(string partName, Color color, int order)
    {
        var part = new GameObject(partName);
        part.transform.SetParent(bar.transform, false);
        var renderer = part.AddComponent<SpriteRenderer>();
        renderer.sprite = pixel;
        renderer.color = color;
        renderer.sortingOrder = order;
        return part.transform;
    }
}
