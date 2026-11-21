using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class SceneExit : MonoBehaviour
{
    [SerializeField] private string destinationScene;
    [SerializeField] private string upgradedDestinationScene;
    [SerializeField, Range(2, 3)] private int minimumUpgradeLevel = 2;
    [SerializeField] private string maxLevelDestinationScene;
    [SerializeField] private SceneFade fade;

    private void OnTriggerEnter2D(Collider2D other) => TryExit(other);

    private void OnTriggerStay2D(Collider2D other) => TryExit(other);

    private void TryExit(Collider2D other)
    {
        if (SceneFade.Busy || fade == null)
            return;

        Rigidbody2D body = other.attachedRigidbody;
        if (body != null && body.CompareTag("Player") &&
            body.TryGetComponent<Health>(out var health) && health.IsAlive)
        {
            bool useUpgradedDestination = !string.IsNullOrEmpty(upgradedDestinationScene) &&
                OreInventory.PickaxeLevel >= minimumUpgradeLevel;
            string destination = useUpgradedDestination ? upgradedDestinationScene : destinationScene;
            if (OreInventory.PickaxeLevel == 3 && !string.IsNullOrEmpty(maxLevelDestinationScene))
                destination = maxLevelDestinationScene;

            fade.GoTo(destination);
        }
    }
}
