using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipPrevention : MonoBehaviour
{
    [SerializeField] GameObject clipProjector;
    [SerializeField] float checkDistance;
    [SerializeField] Vector3 newDirection;

    private float _lerpPos;
    private RaycastHit _hit;

    private void Update()
    {
        if(Physics.Raycast(clipProjector.transform.position, clipProjector.transform.forward, out _hit, checkDistance))
        {
            // Get a percentage from 0 to max distance
            _lerpPos = 1 - (_hit.distance / checkDistance);
        }
        else
        {
            _lerpPos = 0;
        }

        Debug.DrawRay(transform.position, clipProjector.transform.forward * checkDistance, Color.red);


        Mathf.Clamp01(_lerpPos);
        
        Debug.Log("LerpPos: " + _lerpPos);

        transform.localRotation = Quaternion.Lerp(
            Quaternion.Euler(Vector3.zero), // Pointing straight
            Quaternion.Euler(newDirection), // Pointing off to the side
            _lerpPos // Percent position between the two
            );
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawLine(clipProjector.transform.position, clipProjector.transform.forward);
    //}
}
