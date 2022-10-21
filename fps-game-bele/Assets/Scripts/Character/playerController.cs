using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Models;

public class playerController : MonoBehaviour
{
    private DefaultInput defaultInput;
    public Vector2 inputMovement;
    public Vector2 inputView;

    private Vector2 newCameraRotation;
    
    [Header("References")]
    public Transform cameraHolder;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70;
    public float viewClampYMax = 80;

    private void Awake()                 
    {
        defaultInput = new DefaultInput();

        defaultInput.Character.Movement.performed += e => inputMovement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => inputView = e.ReadValue<Vector2>();

        defaultInput.Enable();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;

    }

    private void Update(){
        CalculateView();
        CalculateMovement();
    }

    private void CalculateView(){
        newCameraRotation.x += (inputView.y) * (playerSettings.ViewYSensitivity) * (Time.deltaTime);
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);

        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }

    private void CalculateMovement(){

    }

}
