using System;
using UnityEngine;

public class WalkingMovement : FootMovementStrategy
{
    private readonly FootWalkManager walkManager;

    private readonly FootRange footRange;
    private StepBehaviour step;

    private Vector3 previousPosition;

    public WalkingMovement(FootWalkManager walkManager, FootTarget footTarget, FootRange footRange) : base(footTarget)
    {
        this.walkManager = walkManager;
        this.footRange = footRange;
        previousPosition = position;
    }

    public float GetDistanceBehindCenter(Vector3 velocityForward)
    {
        return footRange.GetDistanceBehindCenter(position, velocityForward);
    }

    public override void Update()
    {
        if (ShouldFloat())
            TriggerFloat();
        else if (ShouldStep())
            TriggerStep();

        // If we are planted, just keep foot stationary
        if (state.IsPlanted())
            position = previousPosition;
        else
        {
            step.UpdateStep();

            position = step.GetPosition();
            rotation = step.GetRotation();
            previousPosition = position;

            if (step.IsComplete())
                state.SetState(FootState.PLANTED);
        }
    }

    private bool ShouldFloat()
    {
        if (!state.IsFloating())
        {
            bool isInRange = footRange.IsWithinRange(position);
            // Check to see if a jump was triggered and if foot has left range
            return walkManager.IsJumpTimedOut() && !isInRange;
        }
        return false;
    }

    private void TriggerFloat()
    {
        step = new FloatStep(walkManager, footTarget.transform, footRange);
        state.ForceSetState(FootState.FLOATING);
    }

    private WalkingMovement GetPairMovement()
    {
        FootMovementStrategy pairMovement = footTarget.GetPairMovement();
        if (!(pairMovement is WalkingMovement))
            throw new InvalidOperationException("Foot pair for " + footTarget.name + " is not WALKING");
        return (WalkingMovement)pairMovement;
    }

    private bool ShouldStep()
    {
        WalkingMovement pair = GetPairMovement();

        // We are only allowed to step if we are planted and the paired foot isn't stepping
        if (state.IsPlanted() && !pair.state.IsStepping())
        {
            // If player is moving, we are in our walk cycle
            Vector3 controllerVelocity = walkManager.GetControllerVelocity();
            bool isParentMoving = controllerVelocity.magnitude > 0.01f;
            if (isParentMoving)
            {
                float threshold = walkManager.FootRestDistanceThreshold;
                float distanceFromRest = GetDistanceBehindCenter(controllerVelocity);

                // If the other pair is floating, we should step if we are beyond the unbalanced threshold
                if (pair.state.IsFloating())
                    return distanceFromRest > threshold;

                // Otherwise, we should issue a step if both feet are behind the center of gravity...
                float pairDistanceFromRest = pair.GetDistanceBehindCenter(controllerVelocity);
                bool pairUnbalanced = distanceFromRest > threshold && pairDistanceFromRest > threshold;

                // ... and this foot is further back than the pair foot
                return pairUnbalanced && distanceFromRest > pairDistanceFromRest;

            }
            // If we are "idle" (parent is not moving), feet should move to center if they are not at "rest" position
            else
                return !IsAtRest();
        }
        return false;
    }

    private void TriggerStep()
    {
        step = new GroundStep(walkManager, footTarget.transform, footRange);

        if (step.FailedToStep())
            TriggerFloat();
        else
            state.SetState(FootState.STEPPING);
    }

    public bool IsAtRest() { return state.IsPlanted() && footRange.IsInRestRange(position); }

    public Vector3 GetPlayerVelocityEffect()
    {
        // Apply an "unbalanced" effect if the player is grounded but this foot is floating
        if (state.IsFloating() && walkManager.IsPlayerGrounded())
        {
            Vector3 playerGroundPosition = walkManager.GetPlayerGroundedPosition();
            Vector3 imbalanceDirection = position - playerGroundPosition;
            return imbalanceDirection * walkManager.UnbalancedSpeed * Time.deltaTime;
        }
        else
        {
            return Vector3.zero;
        }
    }
}