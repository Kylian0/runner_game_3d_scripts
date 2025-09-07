using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    // References to other components 
    public CharacterKeyboardConfig keyboardConfig;
    public GroundSegmenter groundSegmenter;

    // Start in the middle lane
    int currentLane = 1;

    // Speed of lane switching
    float laneSwitchSpeed = 10f;

    // Target position
    public Vector3 characterPosition;

    // To prevent multiple triggers for a single key press
    private bool leftKeyPressed = false;
    private bool rightKeyPressed = false;
    private bool upKeyPressed = false;
    private bool downKeyPressed = false;

    private void Start()
    {
        // Automatic assignment if not set in the inspector
        if (keyboardConfig == null)
        {
            keyboardConfig = GetComponent<CharacterKeyboardConfig>();
        }
        if (groundSegmenter == null)
        {
            groundSegmenter = GetComponent<GroundSegmenter>();
        }

        // Initialize character position to the current lane's center
        characterPosition = transform.position;
    }

    private void Update()
    {
        if (keyboardConfig.isMoveLeftPressed == leftKeyPressed)
        {
            currentLane--;
            currentLane = Mathf.Clamp(currentLane, 0, 2);
            leftKeyPressed = false;
        }
    }
}
