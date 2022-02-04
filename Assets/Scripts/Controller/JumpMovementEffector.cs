using UnityEngine;

public class JumpMovementEffector : MonoBehaviour
{
    // Duration (in seconds) for this limiter effect)
    [SerializeField]
    private float duration = 1f;
    private float movementMultiplier = 1;
    private float movementMultiplierVelocity;

    private void Start()
    {
        PlayerStateController state = GetComponent<PlayerStateController>();
        state.OnGrounded += HandleGrounded;
        state.OnJumped += HandleJumped;
        state.OnSlide += HandleSlide;
    }

    public float Get() { return movementMultiplier; }

    private void FixedUpdate()
    {
        if (movementMultiplier < 1)
            movementMultiplier = Mathf.SmoothDamp(movementMultiplier, 1, ref movementMultiplierVelocity, duration);
    }

    // When we become grounded, restore full movement
    public void HandleGrounded() { movementMultiplier = 1; }

    // When we jump, start the limiting effect
    public void HandleJumped() { movementMultiplier = 0; }

    // When we start sliding, start the limiting effect
    public void HandleSlide() { movementMultiplier = 0; }
}
