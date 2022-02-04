using UnityEngine;

public class IdleMovement : FootMovementStrategy
{
    private readonly IdleFootManager idleManager;

    public IdleMovement(IdleFootManager idleManager, FootTarget footTarget) : base(footTarget)
    {
        this.idleManager = idleManager;
    }

    public override void Update()
    {
        position = idleManager.GetRestPosition(footTarget.GetRestAngle());
        rotation = idleManager.GetRestRotation(footTarget.GetRestAngle());
    }
}
