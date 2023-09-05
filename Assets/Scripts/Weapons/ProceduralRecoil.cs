using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralRecoil : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cam;
    [SerializeField] private WeaponController weaponController;

    [Header("Hipfire Recoil")]
    [SerializeField] private float hipRecoilX;
    [SerializeField] private float hipRecoilY;
    [SerializeField] private float hipRecoilZ;

    [Header("Aimfire Recoil")]
    [SerializeField] private float aimRecoilX;
    [SerializeField] private float aimRecoilY;
    [SerializeField] private float aimRecoilZ;

    [SerializeField] private float kickBackZ;

    [SerializeField] private float snappiness; 
    [SerializeField] private float returnSpeed;

    // Object rotations
    private Vector3 _currentRotation;
    private Vector3 _targetRotation;

    // Object positions
    private Vector3 _currentPosition;
    private Vector3 _targetPosition;
    private Vector3 _initialPosition;

    private bool _isAiming;

    #region - Start / Update -

    private void Start()
    {
        _initialPosition = transform.localPosition;
    }

    private void Update()
    {
        _isAiming = weaponController.isAimingIn;

        _targetRotation = Vector3.Lerp(_targetRotation, Vector3.zero, Time.deltaTime * returnSpeed);
        _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, Time.deltaTime * snappiness);
        transform.localRotation = Quaternion.Euler(_currentRotation);
        cam.localRotation = Quaternion.Euler(_currentRotation);

        Back();
    }

    #endregion

    #region - Recoil -

    public void Recoil()
    {
        _targetPosition -= new Vector3(0,0,kickBackZ);

        if(_isAiming)
            _targetRotation += new Vector3(aimRecoilX, Random.Range(-aimRecoilY, aimRecoilY), Random.Range(-aimRecoilZ, aimRecoilZ));
        else
            _targetRotation += new Vector3(hipRecoilX, Random.Range(-hipRecoilY, hipRecoilY), Random.Range(-hipRecoilZ, hipRecoilZ));
    }

    #endregion

    private void Back()
    {
        _targetPosition = Vector3.Lerp(_targetPosition, _initialPosition, Time.deltaTime * returnSpeed);
        _currentPosition = Vector3.Lerp(_currentPosition, _targetPosition, Time.deltaTime * snappiness);
        transform.localPosition = _currentPosition;
    }
}
