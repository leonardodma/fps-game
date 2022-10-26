using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Models;

public class playerController : MonoBehaviour
{
    private CharacterController characterController;

    private DefaultInput defaultInput;

    public Vector2 inputMovement;

    public Vector2 inputView;

    private Vector3 newCameraRotation;

    private Vector3 newPlayerRotation;

    [Header("References")]
    public Transform cameraHolder;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;

    public float viewClampYMin = -70;

    public float viewClampYMax = 80;

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

    private float cameraHeight;
    private float cameraHeightVelocity;

    private Vector3 stanceColiderCenter;
    private Vector3 stanceColiderCenterVelocity;

    private float stanceColiderHeight;
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
}
