using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerHearts : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image[] hearts;
    [SerializeField] private Color filledColor = new(0.94f, 0.24f, 0.30f);
    [SerializeField] private Color emptyColor = new(0.23f, 0.14f, 0.19f);

    private void Start()
    {
        if (health != null)
            Refresh();
    }

    private void OnEnable()
    {
        if (health == null)
            return;
        health.Damaged += Refresh;
        if (health.IsAlive)
            Refresh();
    }

    private void OnDisable()
    {
        if (health != null)
            health.Damaged -= Refresh;
    }

    private void Refresh()
    {
        int slots = Mathf.Min(health.Max, hearts.Length);
        float remaining = (float)health.Current * slots / health.Max;
        for (int i = 0; i < hearts.Length; i++)
        {
            Image heart = hearts[i];
            if (heart == null)
                continue;
            heart.enabled = i < slots;
            heart.color = Color.Lerp(emptyColor, filledColor, Mathf.Clamp01(remaining - i));
        }
    }
}
