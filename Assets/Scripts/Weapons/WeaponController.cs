using UnityEngine;
using static Models;

public class WeaponController : MonoBehaviour
{
    private const string JUMP = "Jump";
    private const string LAND = "Land";
    private const string FALLING = "Falling";
    private const string IS_SPRINTING = "IsSprinting";
    private const string WEAPON_ANIMATION_SPEED = "WeaponAnimationSpeed";

    [Header("References")]
    [SerializeField] private Animator weaponAnimator;

    [Header("Settings")]
    [SerializeField] private WeaponSettingsModel settings;



    private PlayerController _characterController;

    private bool _isInitialized;

    private Vector3 _newWeaponRotation;
    private Vector3 _newWeaponRotationVelocity;

    private Vector3 _targetWeaponRotation;
    private Vector3 _targetWeaponRotationVelocity;

    private Vector3 _newWeaponMovementRotation;
    private Vector3 _newWeaponMovementRotationVelocity;

    private Vector3 _targetWeaponMovementRotation;
    private Vector3 _targetWeaponMovementRotationVelocity;

    private bool _isGroundedTrigger;
    private float _fallingDelay;

    private void Start()
    {
        _newWeaponRotation = transform.localRotation.eulerAngles;
    }

    private void Update()
    {
        if (!_isInitialized)
            return;

        HandleWeaponRotation();
        SetWeaponAnimations();


    }

    public void Initialize(PlayerController characterController)
    {
        _characterController = characterController;
        _isInitialized = true;
    }

    public void TriggerJump()
    {
        _isGroundedTrigger = false;
        weaponAnimator.SetTrigger(JUMP);
    }

    private void HandleWeaponRotation()
    {
        // Weapon rotation when there's mouse movement
        _targetWeaponRotation.y += settings.swayAmount * (settings.swayXInverted ? -1 : 1) * _characterController.input_View.x * Time.deltaTime;
        _targetWeaponRotation.x += settings.swayAmount * (settings.swayYInverted ? 1 : -1) * _characterController.input_View.y * Time.deltaTime;

        _targetWeaponRotation.x = Mathf.Clamp(_targetWeaponRotation.x, -settings.swayClampX, settings.swayClampX);
        _targetWeaponRotation.y = Mathf.Clamp(_targetWeaponRotation.y, -settings.swayClampY, settings.swayClampY);
        _targetWeaponRotation.z = _targetWeaponRotation.y * settings.swayClampZ;

        _targetWeaponRotation = Vector3.SmoothDamp(_targetWeaponRotation, Vector3.zero, ref _targetWeaponRotationVelocity, settings.swayResetSmoothing);
        _newWeaponRotation = Vector3.SmoothDamp(_newWeaponRotation, _targetWeaponRotation, ref _newWeaponRotationVelocity, settings.swaySmoothing);

        // Weapon rotation when there's character movement
        _targetWeaponMovementRotation.z = settings.movementSwayX * (settings.movementSwayXInverted ? -1 : 1) * _characterController.input_Movement.x;
        _targetWeaponMovementRotation.x = settings.movementSwayY * (settings.movementSwayYInverted ? -1 : 1) * _characterController.input_Movement.y;

        _targetWeaponMovementRotation = Vector3.SmoothDamp(_targetWeaponMovementRotation, Vector3.zero, ref _targetWeaponMovementRotationVelocity, settings.movementSwaySmoothing);
        _newWeaponMovementRotation = Vector3.SmoothDamp(_newWeaponMovementRotation, _targetWeaponMovementRotation, ref _newWeaponMovementRotationVelocity, settings.movementSwaySmoothing);

        transform.localRotation = Quaternion.Euler(_newWeaponRotation + _newWeaponMovementRotation);
    }

    private void SetWeaponAnimations()
    {
        if (_isGroundedTrigger)
        {
            _fallingDelay = 0;
        }
        else
        {
            _fallingDelay += Time.deltaTime;
        }

        if (_characterController.isGrounded && !_isGroundedTrigger && _fallingDelay > 0.1f)
        {
            Debug.Log("Trigger Land");
            weaponAnimator.SetTrigger(LAND);
            _isGroundedTrigger = true;
        }
        else if (!_characterController.isGrounded && _isGroundedTrigger)
        {
            Debug.Log("Trigger Falling");
            weaponAnimator.SetTrigger(FALLING);
            _isGroundedTrigger = false;
        }

        weaponAnimator.SetBool(IS_SPRINTING, _characterController.isSprinting);
        weaponAnimator.SetFloat(WEAPON_ANIMATION_SPEED, _characterController.weaponAnimationSpeed);
    }
}
