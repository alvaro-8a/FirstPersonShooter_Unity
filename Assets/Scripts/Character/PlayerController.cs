using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Models;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraHolder;
    [SerializeField] private Transform feetTransform;

    [Header("Settings")]
    [SerializeField] private PlayerSettingsModel playerSettings;
    [SerializeField] private float viewClampYMin = -70f;
    [SerializeField] private float viewClampYMax = 80f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask groundMask;

    [Header("Gravity")]
    [SerializeField] private float gravityAmount;
    [SerializeField] private float gravityMin;

    private float _playerGravity;
    private Vector3 _jumpingForce;
    private Vector3 _jumpingForceVelocity;


    [Header("Stance")]
    [SerializeField] private PlayerStance playerStance;
    [SerializeField] private float playerStanceSmoothing;
    [SerializeField] private CharacterStance playerStandStance;
    [SerializeField] private CharacterStance playerCrouchStance;
    [SerializeField] private CharacterStance playerProneStance;

    private float stanceCheckErrorMargin = 0.05f;
    private float _cameraHeight;
    private float _cameraHeightVelocity;

    private Vector3 _stanceCapsuleCenterVelocity;
    private float _stanceCapsuleHeightVelocity;


    private CharacterController _characterController;

    private DefaultInput _inputActions;
    [HideInInspector] public Vector2 input_Movement;
    [HideInInspector] public Vector2 input_View;

    private Vector3 _newCameraRotation;
    private Vector3 _newPlayerRotation;

    [HideInInspector] public bool isSprinting;
    private Vector3 _newMovementSpeed;
    private Vector3 _newMovementVelocity;

    [Header("Weapon")]
    [SerializeField] private WeaponController currentWeapon;
    [HideInInspector] public float weaponAnimationSpeed;

    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isFalling;

    [Header("Aiming In")]
    [SerializeField] private bool isAimingIn;

    #region - Awake -

    private void Awake()
    {
        _inputActions = new DefaultInput();
        _characterController = GetComponent<CharacterController>();

        _inputActions.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        _inputActions.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        _inputActions.Character.Jump.performed += e => Jump();

        _inputActions.Character.Crouch.performed += e => Crouch();
        _inputActions.Character.Prone.performed += e => Prone();

        _inputActions.Character.Sprint.performed += e => ToggleSprint();
        _inputActions.Character.SprintReleased.performed += e => StopSprint();

        _inputActions.Weapon.Fire2Pressed.performed += e => AimingInPressed();
        _inputActions.Weapon.Fire2Released.performed += e => AimingInReleased();

        _inputActions.Enable();

        _newCameraRotation = cameraHolder.localRotation.eulerAngles;
        _newPlayerRotation = transform.localRotation.eulerAngles;

        _cameraHeight = cameraHolder.localPosition.y;

        if (currentWeapon)
        {
            currentWeapon.Initialize(this);
        }
    }

    #endregion

    #region - Update -

    private void Update()
    {
        SetIsGrounded();
        SetIsFalling();

        HandleMovement();
        HandleView();
        HandleJump();
        HandleStance();
        HandleAimingIn();
    }

    #endregion

    #region - Aiming In -

    private void AimingInPressed()
    {
        isAimingIn = true;
    }

    private void AimingInReleased()
    {
        isAimingIn = false;
    }

    private void HandleAimingIn()
    {
        if (!currentWeapon)
        {
            return;
        }

        currentWeapon.isAimingIn = isAimingIn;
    }

    #endregion

    #region - IsFalling / IsGrounded -

    private void SetIsGrounded()
    {
        isGrounded = Physics.CheckSphere(feetTransform.position, playerSettings.isGroundedRadius, groundMask);
    }

    private void SetIsFalling()
    {
        isFalling = !isGrounded && _characterController.velocity.magnitude >= playerSettings.isFallingSpeed;

    }

    #endregion

    #region - View / Movement -
    private void HandleMovement()
    {
        if (input_Movement.y <= 0.2f)
            isSprinting = false;

        float verticalSpeed = playerSettings.walkingForwardSpeed;
        float horizontalSpeed = playerSettings.walkingStrafeSpeed;

        if (isSprinting)
        {
            verticalSpeed = playerSettings.runningForwardSpeed;
            horizontalSpeed = playerSettings.runningStrafeSpeed;
        }

        /* EFFECTORS */
        if (!isGrounded)
        {
            playerSettings.speedEffector = playerSettings.fallingSpeedEffector;
        }
        else if (playerStance == PlayerStance.Crouch)
        {
            playerSettings.speedEffector = playerSettings.crouchSpeedEffector;
        }
        else if (playerStance == PlayerStance.Prone)
        {
            playerSettings.speedEffector = playerSettings.proneSpeedEffector;
        }
        else
        {
            playerSettings.speedEffector = 1;
        }

        weaponAnimationSpeed = _characterController.velocity.magnitude / (playerSettings.walkingForwardSpeed * playerSettings.speedEffector);

        if (weaponAnimationSpeed > 1)
        {
            weaponAnimationSpeed = 1;
        }

        verticalSpeed *= playerSettings.speedEffector;
        horizontalSpeed *= playerSettings.speedEffector;

        _newMovementSpeed = Vector3.SmoothDamp(
            _newMovementSpeed,
            new Vector3(horizontalSpeed * input_Movement.x * Time.deltaTime, 0, verticalSpeed * input_Movement.y * Time.deltaTime),
            ref _newMovementVelocity,
            isGrounded ? playerSettings.movementSmoothing : playerSettings.fallingSmoothing
        );

        var movementSpeed = transform.TransformDirection(_newMovementSpeed);

        if (_playerGravity > gravityMin)
            _playerGravity -= gravityAmount * Time.deltaTime;

        if (_playerGravity < -.1f && isGrounded)
            _playerGravity = -.1f;

        movementSpeed.y += _playerGravity;
        movementSpeed += _jumpingForce * Time.deltaTime;

        _characterController.Move(movementSpeed);
    }

    private void HandleView()
    {
        // Horizontal rotation => rotation on the Y axis
        _newPlayerRotation.y += (playerSettings.viewXInverted ? -1 : 1) * input_View.x * playerSettings.viewXSensitivity * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(_newPlayerRotation);

        // Vertical camera rotation => rotation on the X axis
        _newCameraRotation.x += (playerSettings.viewYInverted ? 1 : -1) * input_View.y * playerSettings.viewYSensitivity * Time.deltaTime;
        _newCameraRotation.x = Mathf.Clamp(_newCameraRotation.x, viewClampYMin, viewClampYMax);

        cameraHolder.localRotation = Quaternion.Euler(_newCameraRotation);
    }

    #endregion

    #region - Jumping -
    private void HandleJump()
    {
        _jumpingForce = Vector3.SmoothDamp(_jumpingForce, Vector3.zero, ref _jumpingForceVelocity, playerSettings.jumpingFalloff);
    }

    private void Jump()
    {
        if (!isGrounded || playerStance == PlayerStance.Prone)
            return;

        if (playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.stanceCollider.height))
                return;

            playerStance = PlayerStance.Stand;
            return;
        }

        // JUMP
        _jumpingForce = Vector3.up * playerSettings.jumpingHeigh;
        _playerGravity = 0;
        currentWeapon.TriggerJump();
    }

    #endregion

    #region - Stance -
    private void HandleStance()
    {
        CharacterStance currentStance = playerStandStance;

        if (playerStance == PlayerStance.Crouch)
            currentStance = playerCrouchStance;
        else if (playerStance == PlayerStance.Prone)
            currentStance = playerProneStance;

        _cameraHeight = Mathf.SmoothDamp(cameraHolder.localPosition.y, currentStance.cameraHeight, ref _cameraHeightVelocity, playerStanceSmoothing);
        cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x, _cameraHeight, cameraHolder.localPosition.z);

        _characterController.height = Mathf.SmoothDamp(_characterController.height, currentStance.stanceCollider.height, ref _stanceCapsuleHeightVelocity, playerStanceSmoothing);
        _characterController.center = Vector3.SmoothDamp(_characterController.center, currentStance.stanceCollider.center, ref _stanceCapsuleCenterVelocity, playerStanceSmoothing);
    }

    private void Crouch()
    {
        if (playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.stanceCollider.height))
                return;

            playerStance = PlayerStance.Stand;
            return;
        }

        if (StanceCheck(playerCrouchStance.stanceCollider.height))
            return;

        playerStance = PlayerStance.Crouch;
    }

    private void Prone()
    {
        if (playerStance == PlayerStance.Prone)
        {
            if (StanceCheck(playerStandStance.stanceCollider.height))
                return;

            playerStance = PlayerStance.Stand;
            return;
        }

        playerStance = PlayerStance.Prone;
    }

    private bool StanceCheck(float stanceCheckHeight)
    {
        var start = new Vector3(feetTransform.position.x, feetTransform.position.y + _characterController.radius + stanceCheckErrorMargin, feetTransform.position.z);
        var end = new Vector3(feetTransform.position.x, feetTransform.position.y - _characterController.radius - stanceCheckErrorMargin + stanceCheckHeight, feetTransform.position.z);

        return Physics.CheckCapsule(start, end, _characterController.radius, playerMask);
    }

    #endregion

    #region - Sprinting -
    private void ToggleSprint()
    {
        if (input_Movement.y <= 0.2f)
        {
            isSprinting = false;
            return;
        }

        isSprinting = !isSprinting;
    }

    private void StopSprint()
    {
        if (playerSettings.sprintingHold)
        {
            isSprinting = false;
        }
    }

    #endregion

    #region - Gizmos -

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(feetTransform.position, playerSettings.isGroundedRadius);
    }

    #endregion
}
