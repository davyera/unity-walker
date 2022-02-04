using System.Collections.Generic;
using UnityEngine;

public abstract class StepBehaviour
{
    protected float lerp;

    protected readonly FootWalkManager manager;
    protected readonly FootRange footRange;

    protected Vector3 startPosition;
    protected Vector3 startNormal;

    protected Vector3 position;
    protected Vector3 normal;

    public StepBehaviour(FootWalkManager manager, Transform initialState, FootRange footRange)
    {
        this.manager = manager;
        this.footRange = footRange;

        position = initialState.position;
        normal = initialState.up;

        startPosition = position;
        startNormal = normal;
    }

    public void UpdateStep()
    {
        DoUpdate();

        // Increase the lerp depending on step duration
        if (lerp < 1)
            lerp += Time.deltaTime / GetStepDuration();

        // Ensure we don't go over 1
        if (lerp > 1)
            lerp = 1;
    }

    protected abstract void DoUpdate();

    protected abstract float GetStepDuration();

    protected abstract bool PerformRaycast(Vector3 origin, out RaycastHit hit);

    public abstract bool IsComplete();

    public abstract bool FailedToStep();

    public Vector3 GetPosition() { return position; }

    public Quaternion GetRotation()
    {
        Quaternion normalRotation = Quaternion.FromToRotation(manager.transform.up, normal);
        return normalRotation * manager.GetParentRotation();
    }

    protected LinkedList<Vector3> DefaultRaycastOrigins()
    {
        LinkedList<Vector3> defaultPositions = new LinkedList<Vector3>();
        // First we try for the foot's natural resting position (center of the range)
        Vector3 rangeCenter = footRange.GetCenter();
        defaultPositions.AddFirst(rangeCenter);
        // Then, for edge cases, we try the center of the parent transform
        defaultPositions.AddLast(manager.transform.position);
        // Further edge cases: Try front and back of the range
        defaultPositions.AddLast(footRange.GetFront());
        defaultPositions.AddLast(footRange.GetBack());
        return defaultPositions;
    }

    protected bool PerformRaycasts(LinkedList<Vector3> origins, out RaycastHit hit)
    {
        foreach (Vector3 origin in origins)
            if (PerformRaycast(origin, out hit))
                return true;
            
        hit = default;
        return false;
    }

}
