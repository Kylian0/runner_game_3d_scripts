using System;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    // References to other components 
    public GroundSegmenter groundSegmenter;
    private CharacterAnimation characterAnimation;

    // Start in the middle lane
    int currentLane = 1;

    // Target position
    public Vector3 characterPosition;

    // Target position for smooth movement
    private Vector3 targetPosition;

    // Speed of lane switching
    float laneSwitchSpeed = 5f;

    // Jump parameters
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpSpeed = 2f;
    private bool isJumping = false;
    private float jumpStartY;
    private float jumpTargetY;
    private float jumpTimer = 0f;
    private float jumpDuration = 0.5f;
    private bool isGoingUp = true;

    // Slide parameters
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float slideTimer = 0f;
    private bool isSliding = false;
    private float slideElapsed = 0f;
    private Quaternion slideStartRotation;
    private Quaternion slideTargetRotation;

    private void Start()
    {
        // Security Check for references
        if (groundSegmenter != null)
        {
            Debug.Log($"GroundSegmenter is assigned {groundSegmenter}");
        } else
        {
            Debug.Log($"GroundSegmenter is not assigned {groundSegmenter}");
        }

        // Initialize character position to the current lane's center
        characterPosition = transform.position;

        characterAnimation = GetComponent<CharacterAnimation>();
        if (characterAnimation == null)
        {
            Debug.LogError("CharacterAnimation component not found on the character.");
        }

    }

    private void Update()
    {
        if (groundSegmenter != null)
        {
            // Take the position of the actual lane
            Vector3 LanePosition = groundSegmenter.GetLaneWorldCenter(currentLane);
            // Move the character towards the target position smoothly
            targetPosition = new Vector3(LanePosition.x, transform.position.y, transform.position.z);

            // Change position if left or right key is pressed
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * laneSwitchSpeed);
        }
        HorizontalMovement();
        VerticalMovement();
    }

    // Handle horizontal movement (left/right)
    private void HorizontalMovement()
    {
        // Left Movement \\

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentLane--;
            currentLane = Mathf.Clamp(currentLane, 0, 2);
        }

        // Right Movement \\

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentLane++;
            currentLane = Mathf.Clamp(currentLane, 0, 2);
        }
    }

    // Handle vertical movement (jump/slide)
    private void VerticalMovement()
    {
        // Jump Movement \\

        if (Input.GetKeyDown(KeyCode.UpArrow) && !isJumping)
        {
            // Start jump
            isJumping = true;
            isGoingUp = true;

            jumpStartY = transform.position.y;
            jumpTargetY = jumpStartY + jumpHeight;
            jumpTimer = 0f;
        }

        // Gestion fluid of jump
        if (isJumping)
        {
            float speed = jumpHeight / (jumpDuration / 2f);
            // Going up

            if (isGoingUp)
            {
                jumpTimer+= Time.deltaTime;
                // Move character up
                float newY = Mathf.MoveTowards(transform.position.y, jumpTargetY, speed * Time.deltaTime);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);

                // Check if reached the peak of the jump
                if (Mathf.Approximately(newY, jumpTargetY) || jumpTimer >= jumpDuration / 2f)
                {
                    isGoingUp = false;
                    jumpTimer = 0f;
                }
            } else
            {
                jumpTimer += Time.deltaTime;
                float newY = Mathf.MoveTowards(transform.position.y, jumpStartY, speed * Time.deltaTime);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                // Check if landed
                if (Mathf.Approximately(newY, jumpStartY) || jumpTimer >= jumpDuration / 2f)
                {
                    isJumping = false;
                    transform.position = new Vector3(transform.position.x, jumpStartY, transform.position.z);
                }
            }
        } 

        // Slide Movement \\
        if (Input.GetKeyDown(KeyCode.DownArrow) && !isSliding)
        {
            // Start slide
            isSliding = true;
            slideElapsed = 0f;
            slideStartRotation = transform.rotation;
            slideTargetRotation = Quaternion.Euler(-80f, 0f, 0f);
        }

        // Gestion fluid of slide
        if (isSliding)
        {
            slideElapsed += Time.deltaTime;
            // Smoothly rotate the character downwards
            float t = Mathf.Clamp01(slideElapsed / slideDuration);
            transform.rotation = Quaternion.Slerp(slideStartRotation, slideTargetRotation, t);
            // Check if slide duration is over
            if (slideElapsed >= slideDuration)
            {
                isSliding = false;
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
        
    }
}
