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
    }

    private void Update()
    {
        CalculateView();
        CalculateMovement();
        CalculateJump();
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
        if (playerGravity > gravityMin && jumpForce.y < 0.1f)
            playerGravity -= gravity * Time.deltaTime;

        if (characterController.isGrounded && playerGravity < -1)
            playerGravity = -1f;

        if (jumpForce.y > 0.1f) playerGravity = 0;

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

    private void Jump()
    {
        if (!characterController.isGrounded) return;

        jumpForce = Vector3.up * playerSettings.JumpHeight;
    }
}
