using System.Collections.Generic;
using UnityEngine;

public class MovementTransition : MonoBehaviour
{
    private float toggleTransitionTime;

    private bool incrementing;

    private float lerp;

    private List<MovementBlend> registeredMovementBlends = new List<MovementBlend>();

    public void Init(float toggleTransitionTime, bool startingState)
    {
        this.toggleTransitionTime = toggleTransitionTime;
        lerp = startingState ? 1 : 0;

        // By default we will not be incrementing or decrementing the counter
        incrementing = startingState;
    }

    /** Registers a new MovementBlend to be managed by this Transition */
    public MovementBlend RegisterMovementTransition(IMovementComponent offMovement, IMovementComponent onMovement)
    {
        MovementBlend blend = new MovementBlend(offMovement, onMovement, this);
        registeredMovementBlends.Add(blend);
        return blend;
    }

    public void DeregisterMovementBlend(MovementBlend blend)
    {
        registeredMovementBlends.Remove(blend);
    }

    public float GetProgress() { return lerp; }

    public void Increment() { incrementing = true; }
    public void Decrement() { incrementing = false; }

    private void Update()
    {
        UpdateLerp();
        UpdateMovements();
    }

    private void UpdateLerp()
    {
        if (!IsComplete())
            // Ensure we don't go over or under our bounds with min/max
            if (incrementing)
                lerp = Mathf.Min(lerp + Time.deltaTime / toggleTransitionTime, 1);
            else
                lerp = Mathf.Max(lerp - Time.deltaTime / toggleTransitionTime, 0);
    }

    private void UpdateMovements()
    {
        bool off = IsOff();
        bool on = IsOn();
        foreach (MovementBlend blend in registeredMovementBlends)
        {
            if (off) 
                blend.onMovement.StopUpdate();
            else if (on) 
                blend.offMovement.StopUpdate();
            else
            {
                blend.onMovement.StartUpdate();
                blend.offMovement.StartUpdate();
            }
        }
    }

    private bool IsComplete() { return IsOn() || IsOff(); }
    private bool IsOff() { return !incrementing && lerp <= 0; }
    private bool IsOn() { return incrementing && lerp >= 1; }
}

public class MovementBlend : IMovementComponent
{
    internal IMovementComponent offMovement;
    internal IMovementComponent onMovement;

    protected MovementTransition transition;

    internal MovementBlend(IMovementComponent offMovement, IMovementComponent onMovement, MovementTransition transition)
    {
        this.offMovement = offMovement;
        this.onMovement = onMovement;
        this.transition = transition;
    }

    private float Lerp() { return transition.GetProgress(); }

    public Vector3 GetPosition()
    {
        return Vector3.Lerp(offMovement.GetPosition(), onMovement.GetPosition(), Lerp());
    }

    public Vector3 GetVelocity()
    {
        return Vector3.Lerp(offMovement.GetVelocity(), onMovement.GetVelocity(), Lerp());
    }

    public Quaternion GetRotation()
    {
        return Quaternion.Lerp(offMovement.GetRotation(), onMovement.GetRotation(), Lerp());
    }
    public void StartUpdate() { }
    public void StopUpdate() { }
}

