using UnityEngine;

public class FootRange
{
    private readonly Transform range;
    private readonly FootWalkManager manager;

    public FootRange(FootWalkManager manager, Vector3 restingPosition)
    {
        this.manager = manager;

        range = new GameObject("[FootRange]").transform;
        range.position = restingPosition;
        range.parent = manager.transform;
    }

    public float GetSize() { return manager.GetRangeSize(); }
    public Vector3 GetCenter() { return range.position + manager.GetRangeCenterOffset(); }
    public Vector3 GetFront() { return GetCenter() + range.forward * GetSize(); }
    public Vector3 GetBack() { return GetCenter() - range.forward * GetSize(); }

    public bool IsWithinRange(Vector3 point)
    {
        return GetDistanceToCenter(point) <= GetSize();
    }

    public bool IsWithinSlope(Vector3 normal)
    {
        return manager.IsSlopeTraversable(normal);
    }

    public Vector3 GetClosestPointTo(Vector3 point)
    {
        // If point is already within range, just return the point
        if (IsWithinRange(point))
            return point;

        // Otherwise, find the point on the range perimeter closest to the point
        Vector3 center = GetCenter();
        Vector3 direction = (point - center).normalized;
        return center + direction * GetSize();
    }

    public float GetDistanceToCenter(Vector3 point)
    {
        return Vector3.Distance(point, GetCenter());
    }

    public bool IsInRestRange(Vector3 point)
    {
        float lateralRadius = manager.FootRestDistanceThreshold;
        float verticalRadius = GetSize();
        Vector3 ellipsoidRadii = new Vector3(lateralRadius, verticalRadius, lateralRadius);
        return MathUtils.IsPointWithinEllipsoid(point, range.position, ellipsoidRadii);
    }

    public float GetDistanceBehindCenter(Vector3 point, Vector3 forward)
    {
        Vector3 toCenter = GetCenter() - point;

        // If we have no directionality, just return the distance to the center
        if (forward == Vector3.zero)
            return toCenter.magnitude;

        Vector3 toCenterProjected = Vector3.Project(toCenter, forward);
        bool sign = Vector3.Dot(forward, toCenter) >= 0;
        return (sign ? 1 : -1) * toCenterProjected.magnitude;
    }
}
