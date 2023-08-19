using UnityEngine;
using static Models;

public class WeaponController : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private WeaponSettingsModel settings;

    private PlayerController _characterController;

    private bool _isInitialized;

    private Vector3 _newWeaponRotation;
    private Vector3 _newWeaponRotationVelocity;

    private Vector3 _targetWeaponRotation;
    private Vector3 _targetWeaponRotationVelocity;

    private void Start()
    {
        _newWeaponRotation = transform.localRotation.eulerAngles;
    }

    private void Update()
    {
        if (!_isInitialized)
            return;

        _targetWeaponRotation.y += settings.swayAmount * (settings.swayXInverted ? -1 : 1) * _characterController._input_View.x * Time.deltaTime;
        _targetWeaponRotation.x += settings.swayAmount * (settings.swayYInverted ? 1 : -1) * _characterController._input_View.y * Time.deltaTime;

        _targetWeaponRotation.x = Mathf.Clamp(_targetWeaponRotation.x, -settings.swayClampX, settings.swayClampX);
        _targetWeaponRotation.y = Mathf.Clamp(_targetWeaponRotation.y, -settings.swayClampY, settings.swayClampY);

        _targetWeaponRotation = Vector3.SmoothDamp(_targetWeaponRotation, Vector3.zero, ref _targetWeaponRotationVelocity, settings.swayResetSmoothing);
        _newWeaponRotation = Vector3.SmoothDamp(_newWeaponRotation, _targetWeaponRotation, ref _newWeaponRotationVelocity, settings.swaySmoothing);

        transform.localRotation = Quaternion.Euler(_newWeaponRotation);
    }

    public void Initialize(PlayerController characterController)
    {
        _characterController = characterController;
        _isInitialized = true;
    }
}
