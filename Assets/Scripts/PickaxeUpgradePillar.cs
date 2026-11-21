using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class PickaxeUpgradePillar : MonoBehaviour
{
    [SerializeField] private Health player;
    [SerializeField] private Transform pillar;
    [SerializeField] private Camera worldCamera;
    [SerializeField, Min(0.1f)] private float interactionRange = 2f;
    [SerializeField] private GameObject prompt;
    [SerializeField] private Text buttonLabel;
    [SerializeField] private Text message;

    private bool CanInteract => player.IsAlive && Time.timeScale > 0f && !SceneFade.Busy &&
        ((Vector2)(player.transform.position - pillar.position)).sqrMagnitude <= interactionRange * interactionRange;

    private void Update()
    {
        bool near = CanInteract;
        if (prompt.activeSelf != near)
        {
            prompt.SetActive(near);
            message.text = "";
        }
        if (!near)
            return;

        int level = OreInventory.PickaxeLevel;
        buttonLabel.text = level < 3 ? $"Melhorar picareta para nível {level + 1} [E]" : "Picareta no nível máximo";
        if (Keyboard.current?.eKey.wasPressedThisFrame == true || ClickedPillar())
            Interact();
    }

    private bool ClickedPillar()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame ||
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return false;

        Vector2 point = worldCamera.ScreenToWorldPoint(mouse.position.ReadValue());
        foreach (Collider2D collider in Physics2D.OverlapPointAll(point))
        {
            if (collider.transform.IsChildOf(pillar))
                return true;
        }
        return false;
    }

    public void Interact()
    {
        if (!CanInteract)
            return;

        int level = OreInventory.PickaxeLevel;
        if (level == 3)
        {
            message.text = "A picareta já está no nível máximo.";
            return;
        }

        int required = GameManager.Settings?.OresForUpgrade(level) ?? 3;
        int missing = required - OreInventory.CountForTier(level);
        if (missing > 0)
        {
            string ore = missing == 1 ? "minério" : "minérios";
            message.text = $"Pegue todos os minérios necessários. Faltam {missing} {ore}.";
            return;
        }

        OreInventory.Upgrade();
        message.text = $"Picareta nível {OreInventory.PickaxeLevel} liberada!";
    }
}
