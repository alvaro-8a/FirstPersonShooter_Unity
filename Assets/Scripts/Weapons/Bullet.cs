using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject explosion;

    [Header("Settings")]
    [SerializeField] private LayerMask enemyMask;
    [SerializeField, Range(0f, 1f)] private float bounciness;
    [SerializeField] private bool useGravity;

    [Header("Damage")]
    [SerializeField] private int explosionDamage;
    [SerializeField] private float explosionRange;

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 1;
    [SerializeField] private int maxCollisions;
    [SerializeField] private float maxLifeTime;
    [SerializeField] private bool explodeOnTouch = true;

    private Rigidbody rb;
    private PhysicMaterial _physics_mat;
    private int _collisions;

    #region - Awake / Start / Update -

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //Destroy(gameObject, lifeTime);
    }

    private void Start()
    {
        _physics_mat = new PhysicMaterial();
        _physics_mat.bounciness = bounciness;
        _physics_mat.frictionCombine = PhysicMaterialCombine.Minimum;
        _physics_mat.bounceCombine = PhysicMaterialCombine.Maximum;

        GetComponent<CapsuleCollider>().material = _physics_mat;

        rb.useGravity = useGravity;
    }

    private void Update()
    {
        if (_collisions > maxCollisions)
            Explode();

        maxLifeTime -= Time.deltaTime;
        if (maxLifeTime <= 0)
            Explode();
    }

    #endregion

    #region - OnCollision -

    private void OnCollisionEnter(Collision collision)
    {
        _collisions++;

        //Enemy enemy;

        if(explodeOnTouch)
        {
            Explode();
        }
    }

    #endregion

    #region - Explode -

    private void Explode()
    {
        if (explosion != null)
        {
            var explosionGO = Instantiate(explosion, transform.position, Quaternion.identity);
            explosionGO.transform.rotation = Quaternion.Euler(-transform.forward);
            Destroy(explosionGO, 2f);
        }

        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, enemyMask);

        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].GetComponent<Enemy>().TakeDamage(explosionDamage);
        }

        Invoke("DelayDestroy", 0.01f);
    }

    private void DelayDestroy()
    {
        Destroy(gameObject);
    }

    #endregion

    #region - Gizmos -

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }

    #endregion
}
