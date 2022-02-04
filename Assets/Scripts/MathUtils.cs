using System;
using UnityEngine;

/** 
 *  Static methods for various Math operations not provided by Unity.
 */
public sealed class MathUtils
{
    private MathUtils()
    {
        throw new InvalidOperationException();
    }

    public static float Normalize(float value, float max, float min)
    {
        return Mathf.Clamp((value - min) / (max - min), 0, 1);
    }

    public static bool IsPointWithinEllipsoid(Vector3 point, Vector3 center, Vector3 ellipsoidRadii)
    {
        return  Mathf.Pow((point.x - center.x) / ellipsoidRadii.x, 2f) +
                Mathf.Pow((point.y - center.y) / ellipsoidRadii.y, 2f) +
                Mathf.Pow((point.z - center.z) / ellipsoidRadii.z, 2f) <= 1f;
    }

    public static bool IsInLayerMask(GameObject gameObject, LayerMask layerMask)
    {
        return (layerMask.value & (1 << gameObject.layer)) > 0;
    }
}
