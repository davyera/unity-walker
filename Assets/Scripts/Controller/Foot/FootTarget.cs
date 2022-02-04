using UnityEngine;

public class FootTarget : MonoBehaviour
{
    private FootTarget footPair;

    private FootMovementStrategy movement;

    private float restAngle;
    private bool rightHandSide;

    private Vector3 debugCube = new Vector3(0.1f, 0.1f, 0.1f);

    public void Init(FootTarget footPair, float restAngle)
    {
        this.footPair = footPair;
        this.restAngle = restAngle;

        // Movement will not be connected to parent movement
        transform.parent = null;

        // determine if we are on the right hand side from the restAngle
        rightHandSide = restAngle > 0 && restAngle < 180;
    }

    public void InitMovementStrategy(FootMovementStrategy movement) { this.movement = movement; }

    public FootMovementStrategy GetPairMovement() { return footPair.movement; }
    public float GetRestAngle() { return restAngle; }

    private void LateUpdate()
    {
        transform.position = movement.GetPosition();
        transform.rotation = movement.GetRotation();
    }

    public bool IsRightHandSide() { return rightHandSide; }

    public Vector3 GetPlayerVelocityEffect() 
    {
        return (movement is WalkingMovement walkingMovement) ? walkingMovement.GetPlayerVelocityEffect() : Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        if (movement == null) return;
        FootStateController state = movement.GetState();

        if (state.IsFloating()) Gizmos.color = Color.blue;
        else if (state.IsPlanted()) Gizmos.color = Color.green;
        else if (state.IsStepping()) Gizmos.color = Color.red;
        else Gizmos.color = Color.gray;

        // foot target
        Gizmos.DrawCube(transform.position, debugCube);

        // forward
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);

        // normal
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + transform.up);
    }
}
