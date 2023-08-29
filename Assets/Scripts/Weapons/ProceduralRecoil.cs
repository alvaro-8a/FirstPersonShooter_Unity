using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralRecoil : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cam;

    [Header("Recoil Settings")]
    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;
    
    [SerializeField] private float kickBackZ;

    [SerializeField] private float snappiness; 
    [SerializeField] private float returnAmount;

    private Vector3 _currentRotation;
    private Vector3 _targetRotation;
    private Vector3 _currentPosition;
    private Vector3 _targetPosition;
    private Vector3 _initialPosition;

    #region - Start / Update -

    private void Start()
    {
        _initialPosition = transform.localPosition;
    }

    private void Update()
    {
        _targetRotation = Vector3.Lerp(_targetRotation, Vector3.zero, Time.deltaTime * returnAmount);
        _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, Time.deltaTime * snappiness);
        transform.localRotation = Quaternion.Euler(_currentRotation);
        cam.localRotation = Quaternion.Euler(_currentRotation);

        Back(); // Kickback
    }

    #endregion

    #region - Recoil -

    public void Recoil()
    {
        _targetPosition -= new Vector3(0,0,kickBackZ);
        _targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }

    #endregion

    private void Back()
    {
        _targetPosition = Vector3.Lerp(_targetPosition, _initialPosition, Time.deltaTime * returnAmount);
        _currentPosition = Vector3.Lerp(_currentPosition, _targetPosition, Time.deltaTime * snappiness);
        transform.localPosition = _currentPosition;
    }
}
