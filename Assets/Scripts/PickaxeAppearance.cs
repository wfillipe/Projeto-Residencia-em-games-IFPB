using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class PickaxeAppearance : MonoBehaviour
{
    private static readonly int Level = Shader.PropertyToID("_PickaxeLevel");
    private SpriteRenderer visual;
    private MaterialPropertyBlock properties;

    private void Awake()
    {
        visual = GetComponent<SpriteRenderer>();
        properties = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        OreInventory.Changed += Refresh;
        Refresh(OreInventory.Count);
    }

    private void OnDisable() => OreInventory.Changed -= Refresh;

    private void Refresh(int _)
    {
        visual.GetPropertyBlock(properties);
        properties.SetFloat(Level, OreInventory.PickaxeLevel);
        visual.SetPropertyBlock(properties);
    }
}
