using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public sealed class OreCounter : MonoBehaviour
{
    private Text label;

    private void Awake() => label = GetComponent<Text>();

    private void OnEnable()
    {
        OreInventory.Changed += Refresh;
        Refresh(OreInventory.Count);
    }

    private void OnDisable() => OreInventory.Changed -= Refresh;

    private void Refresh(int count) => label.text = $"Picareta Nv. {OreInventory.PickaxeLevel}  |  Minérios: {count}";
}
