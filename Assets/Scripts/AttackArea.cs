using System.Collections.Generic;
using UnityEngine;

internal static class AttackArea
{
    // O Box Collider é uma referência visual. A consulta acontece apenas no quadro de impacto.
    public static void Aim(BoxCollider2D area, Vector2 direction)
    {
        area.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    public static void Collect(BoxCollider2D area, LayerMask layers, List<Collider2D> hits)
    {
        Vector2 scale = area.transform.lossyScale;
        Vector2 size = Vector2.Scale(area.size, new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y)));
        var filter = new ContactFilter2D { useTriggers = Physics2D.queriesHitTriggers };
        filter.SetLayerMask(layers);
        Physics2D.OverlapBox(area.transform.TransformPoint(area.offset), size, area.transform.eulerAngles.z, filter, hits);
    }

    public static void Draw(BoxCollider2D area)
    {
        if (area == null)
            return;

        Matrix4x4 previousMatrix = Gizmos.matrix;
        Color previousColor = Gizmos.color;
        Gizmos.matrix = area.transform.localToWorldMatrix;
        Gizmos.color = new Color(1f, 0.62f, 0.16f);
        Gizmos.DrawWireCube(area.offset, new Vector3(area.size.x, area.size.y, 0f));
        Gizmos.matrix = previousMatrix;
        Gizmos.color = previousColor;
    }
}
