using UnityEngine;

public abstract class FootMovementStrategy : Updateable, IMovementComponent
{
    protected readonly FootTarget footTarget;
    protected readonly FootStateController state;

    protected Vector3 position;
    protected Quaternion rotation;

    public FootMovementStrategy(FootTarget footTarget)
    {
        this.footTarget = footTarget;
        state = new FootStateController();

        position = footTarget.transform.position;
        rotation = footTarget.transform.rotation;
    }

    public FootStateController GetState() { return state; }

    public Vector3 GetPosition() { return position; }
    /* Velocity is not used for foot movement -- we just directly calculate positions */
    public Vector3 GetVelocity() { return Vector3.zero; }
    public Quaternion GetRotation() { return rotation; }
    public void StartUpdate() { Register(); }
    public void StopUpdate() { Deregister(); }
}
