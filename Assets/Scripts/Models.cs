using System;
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
        public float viewXSensitivity;
        public float viewYSensitivity;

        public float aimingSensitivityEffector;

        public bool viewXInverted;
        public bool viewYInverted;

        [Header("Movement Settings")]
        public bool sprintingHold;
        public float movementSmoothing;

        [Header("Movement - Running")]
        public float runningForwardSpeed;
        public float runningStrafeSpeed;

        [Header("Movement - Walking")]
        public float walkingForwardSpeed;
        public float walkingBackwardSpeed;
        public float walkingStrafeSpeed;

        [Header("Jumping")]
        public float jumpingHeigh;
        public float jumpingFalloff;
        public float fallingSmoothing;

        [Header("Speed Effectors")]
        public float speedEffector = 1;
        public float crouchSpeedEffector;
        public float proneSpeedEffector;
        public float fallingSpeedEffector;
        public float aimingSpeedEffector;

        [Header("Is Grounded / Falling")]
        public float isGroundedRadius;
        public float isFallingSpeed;
    }

    [Serializable]
    public class CharacterStance
    {
        public float cameraHeight;
        public CapsuleCollider stanceCollider;
    }

    #endregion

    #region - Weapons -

    [Serializable]
    public class WeaponSettingsModel
    {
        [Header("Weapon Sway")]
        public float swayAmount;
        public bool swayYInverted;
        public bool swayXInverted;
        public float swaySmoothing;
        public float swayResetSmoothing;
        public float swayClampX;
        public float swayClampY;
        public float swayClampZ;

        [Header("Weapon Movement Sway")]
        public float movementSwayX;
        public float movementSwayY;
        public bool movementSwayYInverted;
        public bool movementSwayXInverted;
        public float movementSwaySmoothing;
    }

    #endregion
}
