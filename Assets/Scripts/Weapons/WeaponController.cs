using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Models;

public class WeaponController : MonoBehaviour
{
    #region - Variables -

    private const string JUMP = "Jump";
    private const string LAND = "Land";
    private const string FALLING = "Falling";
    private const string IS_SPRINTING = "IsSprinting";
    private const string WEAPON_ANIMATION_SPEED = "WeaponAnimationSpeed";
    private const string RELOAD = "Reload";

    [Header("References")]
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private ProceduralRecoil proceduralRecoil;
        
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

    [Header("Weapon Sway / Breathing")]
    [SerializeField] private Transform weaponSwayObject;

    [SerializeField] private float swayAmountA = 1;
    [SerializeField] private float swayAmountB = 2;
    [SerializeField] private float swayScale = 600;
    [SerializeField] private float swayLerpSpeed = 14;

    private float swayTime;
    private Vector3 swayPosition;

    private Vector3 _weaponSwayPosition;
    private Vector3 _weaponSwayPositionVelocity;

    [Header("Sights")]
    [SerializeField] private Transform sightTarget;
    [SerializeField] private float sightOffset;
    [SerializeField] private float aimingInTime;

    [HideInInspector] public bool isAimingIn;

    [Header("Shooting")]
    [SerializeField] private List<WeaponFireType> allowedFireTypes;
    [SerializeField] private WeaponFireType currentFireType;
    [HideInInspector] public bool isShooting;
    [HideInInspector] public bool isReloading;

    private float _currentFireRate;
    
    private int _bulletsLeft;
    private int _bulletsShot; 

    private bool _isReadyToShoot;
    private bool _isReloading;

    [Header("Sounds")]
    [SerializeField] private AudioClip shotSound;
    [SerializeField] private AudioClip reloadSound;

    private AudioSource _audioSource;

    [Header("Graphics")]
    [SerializeField] private TextMeshProUGUI ammunitionDisplay;

    #endregion

    // Bug fixing
    public bool canInvoke = true;

    #region - Awake / Start / Update -

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        _bulletsLeft = settings.magazineSize;
        _isReadyToShoot = true;
    }

    private void Start()
    {
        _newWeaponRotation = transform.localRotation.eulerAngles;

        //currentFireType = allowedFireTypes.First();
    }

    private void Update()
    {
        if (!_isInitialized)
            return;

        if(ammunitionDisplay != null )
        {
            ammunitionDisplay.SetText(_bulletsLeft / settings.bulletsPerShot + " | " + settings.magazineSize / settings.bulletsPerShot);
        }

        HandleWeaponRotation();
        SetWeaponAnimations();
        HandleWeaponSway();
        HandleWeaponAimingIn();
        HandleShooting();
        HandleReloading();
    }
    #endregion

    #region - Initialize -
    public void Initialize(PlayerController characterController)
    {
        _characterController = characterController;
        _isInitialized = true;
    }

    #endregion

    #region - Aiming In -

    private void HandleWeaponAimingIn()
    {
        var targetPos = transform.position;

        if (isAimingIn)
        {
            targetPos = _characterController.camera.transform.position
                        + (weaponSwayObject.position - sightTarget.position)
                        + (_characterController.camera.transform.forward * sightOffset);
        }

        _weaponSwayPosition = weaponSwayObject.transform.position;
        _weaponSwayPosition = Vector3.SmoothDamp(_weaponSwayPosition, targetPos, ref _weaponSwayPositionVelocity, aimingInTime);
        weaponSwayObject.transform.position = _weaponSwayPosition + swayPosition;
    }

    #endregion

    #region - Weapon Rotation Sway -

    private void HandleWeaponRotation()
    {
        // Weapon rotation when there's mouse movement
        _targetWeaponRotation.y += (isAimingIn ? settings.swayAmount / 3 : settings.swayAmount)
                                    * (settings.swayXInverted ? -1 : 1) * _characterController.input_View.x * Time.deltaTime;
        _targetWeaponRotation.x += (isAimingIn ? settings.swayAmount / 3 : settings.swayAmount)
                                    * (settings.swayYInverted ? 1 : -1) * _characterController.input_View.y * Time.deltaTime;

        _targetWeaponRotation.x = Mathf.Clamp(_targetWeaponRotation.x, -settings.swayClampX, settings.swayClampX);
        _targetWeaponRotation.y = Mathf.Clamp(_targetWeaponRotation.y, -settings.swayClampY, settings.swayClampY);
        _targetWeaponRotation.z = isAimingIn ? 0 : _targetWeaponRotation.y * settings.swayClampZ;

        _targetWeaponRotation = Vector3.SmoothDamp(_targetWeaponRotation, Vector3.zero, ref _targetWeaponRotationVelocity, settings.swayResetSmoothing);
        _newWeaponRotation = Vector3.SmoothDamp(_newWeaponRotation, _targetWeaponRotation, ref _newWeaponRotationVelocity, settings.swaySmoothing);

        // Weapon rotation when there's character movement
        _targetWeaponMovementRotation.z = (isAimingIn ? settings.movementSwayX / 3 : settings.movementSwayX)
                                            * (settings.movementSwayXInverted ? -1 : 1) * _characterController.input_Movement.x;
        _targetWeaponMovementRotation.x = (isAimingIn ? settings.movementSwayY / 3 : settings.movementSwayY)
                                            * (settings.movementSwayYInverted ? -1 : 1) * _characterController.input_Movement.y;

        _targetWeaponMovementRotation = Vector3.SmoothDamp(_targetWeaponMovementRotation, Vector3.zero, ref _targetWeaponMovementRotationVelocity, settings.movementSwaySmoothing);
        _newWeaponMovementRotation = Vector3.SmoothDamp(_newWeaponMovementRotation, _targetWeaponMovementRotation, ref _newWeaponMovementRotationVelocity, settings.movementSwaySmoothing);

        transform.localRotation = Quaternion.Euler(_newWeaponRotation + _newWeaponMovementRotation);
    }

    #endregion

    #region - Weapon Animations -
    
    public void TriggerJump()
    {
        _isGroundedTrigger = false;
        weaponAnimator.SetTrigger(JUMP);
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
            weaponAnimator.SetTrigger(LAND);
            _isGroundedTrigger = true;
        }
        else if (!_characterController.isGrounded && _isGroundedTrigger)
        {
            weaponAnimator.SetTrigger(FALLING);
            _isGroundedTrigger = false;
        }

        weaponAnimator.SetBool(IS_SPRINTING, _characterController.isSprinting);
        weaponAnimator.SetFloat(WEAPON_ANIMATION_SPEED, _characterController.weaponAnimationSpeed);
    }

    private void TriggerReload()
    {
        weaponAnimator.SetTrigger(RELOAD);
    }

    #endregion

    #region - Weapon Sway / Breath -

    private void HandleWeaponSway()
    {
        var targetPos = LissajousCurve(swayTime, swayAmountA, swayAmountB) / (isAimingIn ? swayScale * 2 : swayScale);

        swayPosition = Vector3.Lerp(swayPosition, targetPos, Time.smoothDeltaTime * swayLerpSpeed);
        swayTime += Time.deltaTime;

        if (swayTime > 6.3f)
        {
            swayTime = 0;
        }
    }

    // Breathing / Sway curve
    private Vector3 LissajousCurve(float time, float a, float b)
    {
        return new Vector3(Mathf.Sin(time), a * Mathf.Sin(b * time + Mathf.PI));
    }

    #endregion

    #region - Shooting -

    private void HandleShooting()
    {
        if(isShooting && _isReadyToShoot && !isReloading && _bulletsLeft > 0)
        {
            _bulletsShot = 0;

            Shoot();

            if(currentFireType == WeaponFireType.SemiAuto)
            {
                Debug.Log("SemiAuto");
                isShooting = false;
            }
        }
    }

    private void Shoot()
    {

        _isReadyToShoot = false;

        // Find exact hit position with raycast
        //Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0));
        Ray ray = new Ray(bulletSpawn.position, bulletSpawn.forward);
        RaycastHit hit;

        // Check if ray hits something
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(7);

        // Calculate direction from bulletSpawn to targetPoint
        Vector3 diretionWithoutSpread = targetPoint - bulletSpawn.position;

        // Calculate spread
        float x = Random.Range(-settings.spread, settings.spread);
        float y = Random.Range(-settings.spread, settings.spread);

        // Calculate new direction with spread
        Vector3 directionWithSpread = diretionWithoutSpread + new Vector3(x, y, 0);

        // Instantiate bullet
        var currentBullet = Instantiate(bulletPrefab, bulletSpawn.position, transform.rotation);
        // Rotate bullet to shoot direction
        currentBullet.transform.forward = directionWithSpread.normalized;

        // Add force to bullet
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * settings.shootForce, ForceMode.Impulse);

        // For graneades 
        //currentBullet.GetComponent<Rigidbody>().AddForce(Camera.main.transform.up * settings.upwardForce, ForceMode.Impulse);

        // Shot sound
        PlayShot();
        proceduralRecoil.Recoil();

        if (muzzleFlash != null)
        {
            var muzzleFlashObject = Instantiate(muzzleFlash, bulletSpawn);
            Destroy(muzzleFlashObject, 1f);
        }

        _bulletsLeft--;
        _bulletsShot++;

        // Invoke resetShot function
        if(canInvoke)
        {
            Invoke("ResetShot", settings.timeBetweenShooting);
            canInvoke = false;
        }

        // If more than one bulletPerShot repeat shoot function
        if(_bulletsLeft < settings.bulletsPerShot && _bulletsLeft > 0)
        {
            Invoke("Shoot", settings.timeBetweenShots);
        }
    }

    private void ResetShot()
    {
        _isReadyToShoot = true;
        canInvoke = true;
    }

    #endregion

    #region - Reloading -

    private void HandleReloading()
    {
        // Reload with input
        if (isReloading && !_isReloading && _bulletsLeft < settings.magazineSize)
            Reload();
        else
            isReloading = false;

        // Reload automatically when trying to shoot without ammo left
        if (_isReadyToShoot && isShooting && !isReloading && _bulletsLeft <= 0)
            Reload();
    }

    private void Reload()
    {
        isShooting = false;
        _isReloading = true;
        _isReadyToShoot = false;
        TriggerReload();
        PlayReload();
        Invoke("ReloadFinished", settings.reloadTime);
    }

    private void ReloadFinished()
    {
        _bulletsLeft = settings.magazineSize;
        _isReloading = false;
        _isReadyToShoot = true;
        isReloading = false;
    }

    #endregion

    #region - Sounds -

    private void PlayShot()
    {
        _audioSource.PlayOneShot(shotSound);
    }

    private void PlayReload()
    {
        //_audioSource.PlayOneShot(reloadSound);
    }

    public void PlaySound(AudioClip audio, float volume = 1f)
    {

        _audioSource.PlayOneShot(audio, volume);
    }

    #endregion
}
