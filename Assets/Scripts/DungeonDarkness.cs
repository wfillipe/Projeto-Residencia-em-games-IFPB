using UnityEngine;
using UnityEngine.UI;

// Atualiza a abertura depois que a câmera termina de seguir o jogador.
[DefaultExecutionOrder(50)]
public sealed class DungeonDarkness : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Transform player;
    [SerializeField] private Vector2 lightOffset = new(0f, 0.4f);
    [SerializeField] private Image[] shadowLayers;

    private static readonly int Center = Shader.PropertyToID("_Center");
    private Material[] materials;

    private void Awake()
    {
        materials = new Material[shadowLayers.Length];
        for (int i = 0; i < shadowLayers.Length; i++)
        {
            Image layer = shadowLayers[i];
            if (layer == null || layer.material == null)
                continue;

            // Cada cena atualiza só as suas cópias dos materiais.
            materials[i] = new Material(layer.material);
            layer.material = materials[i];
        }
        UpdateCenter();
    }

    private void LateUpdate() => UpdateCenter();

    private void UpdateCenter()
    {
        if (player == null || worldCamera == null)
            return;

        Vector3 viewport = worldCamera.WorldToViewportPoint(player.position + (Vector3)lightOffset);
        var center = new Vector4(viewport.x, viewport.y, 0f, 0f);
        foreach (Material material in materials)
        {
            if (material != null)
                material.SetVector(Center, center);
        }
    }

    private void OnDestroy()
    {
        foreach (Material material in materials)
        {
            if (material != null)
                Destroy(material);
        }
    }
}
