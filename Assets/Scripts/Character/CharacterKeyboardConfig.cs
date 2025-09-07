using UnityEngine;

public class CharacterKeyboardConfig : MonoBehaviour
{
    // Speed of horizontal movement
    public float horizontalSpeed = 1f;

    // Speed of vertical movement
    public float verticalSpeed = 1f;

    // Variables to track button states
    public bool isMoveLeftPressed = false;
    public bool isMoveRightPressed = false;
    public bool isJumpPressed = false;
    public bool isSlidePressed = false;

    private void Start()
    {
        
    }

    private void Update()
    {
        // Get horizontal input for movement (left/right)
        float MoveX = Input.GetAxis("Horizontal");

        // Update button states for left and right movement
        if (MoveX < 0)
        {
            isMoveLeftPressed = true;
            isMoveRightPressed = false;
        }
        else if (MoveX > 0) 
        {
            isMoveRightPressed = true;
            isMoveLeftPressed = false;
        };

        // Update button states for jump and slide movement
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            isJumpPressed = true;
            Debug.Log("Jump pressed");
        }
        else
        {
            isJumpPressed = false;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            isSlidePressed = true;
            Debug.Log("Slide pressed");
        }
        else
        {
            isSlidePressed = false;
        }
    }
}
