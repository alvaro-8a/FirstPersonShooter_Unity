using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifeTime = 1;

    #region - Awake -

    private void Awake()
    {
        Destroy(gameObject, lifeTime);
    }

    #endregion
}
