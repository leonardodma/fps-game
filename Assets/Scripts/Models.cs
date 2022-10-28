using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Models
{

#region - Player -
    public enum PlayerStance
    {
        Stand,
        Crouch,
        Prone
    }

    [Serializable]
    public class PlayerSettingsModel
    {
        [Header("View Settings")]
        public float ViewXSensitivity;

        public float ViewYSensitivity;

        public bool ViewXInverted;

        public bool ViewYInverted;

        [Header("Movement Settings")]
        public float MovementForwardSpeed;

        public float MovementBackwardSpeed;

        public float MovementStrafeSpeed;

        [Header("Running Settings")]
        public float RunningFowardSpeed;

        public float RunningStrafeSpeed;

        [Header("Jump Settings")]
        public float JumpHeight;

        public float JumpTime;
    }

    [Serializable]
    public class PlayerStanceSettings
    {
        public float CameraHeight;

        public CapsuleCollider StanceCollider;
    }
#endregion
}
