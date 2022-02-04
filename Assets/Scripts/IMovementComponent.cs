using UnityEngine;

public interface IMovementComponent
{
    Vector3 GetPosition();
    Vector3 GetVelocity();
    Quaternion GetRotation();
    void StartUpdate();
    void StopUpdate();
}
