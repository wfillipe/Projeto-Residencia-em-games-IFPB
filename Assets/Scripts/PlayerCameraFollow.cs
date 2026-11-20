using UnityEngine;

public sealed class PlayerCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset = new Vector2(0f, 0.45f);

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Segue a posição interpolada do jogador e mantém a profundidade da câmera.
        Vector3 targetPosition = target.position;
        transform.position = new Vector3(
            targetPosition.x + offset.x,
            targetPosition.y + offset.y,
            transform.position.z);
    }
}
