using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Models;

public class playerController : MonoBehaviour
{
    private CharacterController characterController;

    private DefaultInput defaultInput;

    private Vector2 inputMovement;

    private Vector2 inputView;

    private Vector3 newCameraRotation;

    private Vector3 newPlayerRotation;

    [Header("References")]
    public Transform cameraHolder;
    public Transform feetTransform;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70;
    public float viewClampYMax = 80;
    public LayerMask playerMask;

    [Header("Gravity")]
    public float gravity;
    public float gravityMin;
    private float playerGravity;

    [Header("Jump")]
    public Vector3 jumpForce;
    private Vector3 jumpForceVelocity;

    [Header("Stance")]
    public PlayerStance playerStance;
    public float playerStanceSmoothing; // time in sec to change camera height
    public PlayerStanceSettings playerStandStance;
    public PlayerStanceSettings playerCrouchStance;
    public PlayerStanceSettings playerProneStance;
    public float stanceCheckErrorMargin = 0.05f;
    private float cameraHeight;
    private float cameraHeightVelocity;

    private Vector3 stanceColiderCenterVelocity;
    private float stanceColiderHeightVelocity;

 
    private void Awake()
    {
        // Get the DefaultInput asset
        defaultInput = new DefaultInput();

        // Read the input from the asset, when movement is performed
        defaultInput.Character.Movement.performed += e =>
            inputMovement = e.ReadValue<Vector2>();

        // Read the input from the asset, when view is changed
        defaultInput.Character.View.performed += e =>
            inputView = e.ReadValue<Vector2>();

        // Call Jump() when the Jump action is performed
        defaultInput.Character.Jump.performed += e => Jump();

        // Call Jump() when the Crouch action is performed
        defaultInput.Character.Crouch.performed += e => Crouch();

        // Call Jump() when the Prone action is performed
        defaultInput.Character.Prone.performed += e => Prone();

        defaultInput.Enable();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newPlayerRotation = transform.localRotation.eulerAngles;

        // Get the CharacterController component
        characterController = GetComponent<CharacterController>();

        cameraHeight = cameraHolder.localPosition.y;
    }

    private void Update()
    {
        CalculateView();
        CalculateMovement();
        CalculateJump();
        CalculateStance();
    }

    private void CalculateView()
    {
        // Calculate the new rotation of the camera on the Y axis
        newCameraRotation.y +=
            (playerSettings.ViewXInverted ? -inputView.x : inputView.x) *
            playerSettings.ViewXSensitivity *
            Time.deltaTime;
        transform.localRotation = Quaternion.Euler(newPlayerRotation);

        // Calculate the new rotation of the camera on the X axis
        newCameraRotation.x +=
            (playerSettings.ViewYInverted ? inputView.y : -inputView.y) *
            playerSettings.ViewYSensitivity *
            Time.deltaTime;
        newCameraRotation.x =
            Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);
        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }

    private void CalculateMovement()
    {
        // Calculate the new movement vector
        var verticalSpeed =
            playerSettings.MovementForwardSpeed *
            inputMovement.y *
            Time.deltaTime;
        var horizontalSpeed =
            playerSettings.MovementStrafeSpeed *
            inputMovement.x *
            Time.deltaTime;

        var newMovementSpeed = new Vector3(horizontalSpeed, 0, verticalSpeed);
        newMovementSpeed = transform.TransformDirection(newMovementSpeed);

        // Apply gravity
        if (playerGravity > gravityMin)
            playerGravity -= gravity * Time.deltaTime;

        if (characterController.isGrounded && playerGravity < -0.1)
            playerGravity = -0.1f;


        newMovementSpeed.y += playerGravity;
        newMovementSpeed += jumpForce * Time.deltaTime;

        // Move based on the new movement vector
        characterController.Move (newMovementSpeed);
    }

    private void CalculateJump()
    {
        jumpForce =
            Vector3
                .SmoothDamp(jumpForce,
                Vector3.zero,
                ref jumpForceVelocity,
                playerSettings.JumpTime);
    }

    private void CalculateStance(){
    	// Change the camera height depending on the stance: Stand, crouch or prone
    	var currentStance = playerStandStance;

    	if (playerStance == PlayerStance.Crouch) {
    		currentStance = playerCrouchStance;
    	} else if (playerStance == PlayerStance.Prone){
    		currentStance = playerProneStance;
    	} 


    	cameraHeight = Mathf.SmoothDamp(cameraHolder.localPosition.y, currentStance.CameraHeight, ref cameraHeightVelocity, playerStanceSmoothing);
    	cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x,cameraHeight,cameraHolder.localPosition.z);

    	characterController.height = Mathf.SmoothDamp(characterController.height, currentStance.StanceCollider.height, ref stanceColiderHeightVelocity, playerStanceSmoothing);
    	characterController.center = Vector3.SmoothDamp(characterController.center, currentStance.StanceCollider.center, ref stanceColiderCenterVelocity, playerStanceSmoothing);
    }

    private void Jump()
    {
        if (!characterController.isGrounded) return;

        jumpForce = Vector3.up * playerSettings.JumpHeight;
        playerGravity = 0;
    }

    private void Crouch()
    {
    	if (playerStance == PlayerStance.Crouch){
            if (StanceCheck(playerStandStance.StanceCollider.height)){
                return;
            }
    		playerStance = PlayerStance.Stand;
    	} else {
    		playerStance = PlayerStance.Crouch;
    	}

    	
    }

    private void Prone()
    {
        if (playerStance == PlayerStance.Prone){
            if (StanceCheck(playerStandStance.StanceCollider.height)){
                return;
            }
            playerStance = PlayerStance.Stand;
        } else {
            playerStance = PlayerStance.Prone;
        }
    	
    }

    private bool StanceCheck(float stanceCheckHeight){
        var start = new Vector3(feetTransform.position.x, feetTransform.position.y + stanceCheckErrorMargin + characterController.radius, feetTransform.position.z);
        var end = new Vector3(feetTransform.position.x, feetTransform.position.y - stanceCheckErrorMargin - characterController.radius + stanceCheckHeight, feetTransform.position.z);

        return Physics.CheckCapsule(start, end, characterController.radius, playerMask);
    }
}
