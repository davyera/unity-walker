using System.Collections.Generic;
using UnityEngine;

public class GroundStep : StepBehaviour
{
    private readonly float stepHeight;

    private readonly bool failedToStep;

    private float stepDuration;

    private Vector3 targetPosition;
    private Vector3 targetNormal;

    public GroundStep(FootWalkManager manager, Transform initialState, FootRange footRange)
        : base(manager, initialState, footRange)
    {
        stepHeight = manager.GetStepHeight();

        // Initialize the final step details here
        if (RaycastStep(out RaycastHit hit))
        {
            targetPosition = hit.point;
            targetNormal = hit.normal;
            failedToStep = false;

            CalculateStepDuration();
        }
        else
        {
            failedToStep = true;
        }
    }

    private bool RaycastStep(out RaycastHit hit)
    {
        // Construct list of ordered step positions
        Vector3 preferredTarget = footRange.GetCenter() + manager.GetStepSize();
        LinkedList<Vector3> origins = DefaultRaycastOrigins();
        origins.AddFirst(preferredTarget);

        return PerformRaycasts(origins, out hit);
    }

    private void CalculateStepDuration()
    {
        float movementSpeed = manager.GetStepSpeed();
        float stepDistance = Vector3.Distance(targetPosition, position);
        stepDuration = stepDistance / movementSpeed;
    }

    protected override float GetStepDuration() { return stepDuration; }

    protected override void DoUpdate()
    {
        Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, lerp);
        Vector3 newNormal = Vector3.Lerp(startNormal, targetNormal, lerp);

        // Apply arc effects if the multiplier is greater than 0
        if (stepHeight > 0)
        {
            float arcPosition = manager.StepArcCurve.Evaluate(lerp);
            newPosition += arcPosition * stepHeight * newNormal;
        }

        position = newPosition;
        normal = newNormal;

        // Once Time.deltaTime accumulation is equal to our step duration, we are done with our step
        lerp += Time.deltaTime / stepDuration;
    }

    protected override bool PerformRaycast(Vector3 origin, out RaycastHit hit)
    {
        Vector3 adjustedOrigin = origin + normal * manager.GetRaycastRange() / 2;
        Ray ray = new Ray(adjustedOrigin, -normal);
        bool rayHit = Physics.Raycast(ray, out hit, manager.GetRaycastRange(), GlobalState.GroundLayerMask);
        return rayHit && footRange.IsWithinRange(hit.point) && footRange.IsWithinSlope(hit.normal);
    }

    public override bool IsComplete() { return failedToStep || lerp >= 1; }

    public override bool FailedToStep() { return failedToStep; }

}
