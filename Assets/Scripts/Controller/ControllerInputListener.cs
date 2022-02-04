using UnityEngine;

public class ControllerInputListener : MonoBehaviour
{
    private bool jumpKeyPressed;
    private bool swimKeyHeld;
    private float horizontalInput;
    private float verticalInput;

    public ControllerInputListener() {}

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
            jumpKeyPressed = true;

        swimKeyHeld = Input.GetButton("Jump");

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    // If a jump key press was logged, returns true then sets flag to false.
    public bool ReadJumpRequest()
    {
        if (jumpKeyPressed)
        {
            jumpKeyPressed = false;
            return true;
        }
        return false;
    }

    public bool IsSwimKeyHeld() { return swimKeyHeld; }
    public float ReadHorizontalInput() { return horizontalInput; }
    public float ReadVerticalInput() { return verticalInput; }
}
