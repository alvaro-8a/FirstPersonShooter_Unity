using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private float health = 50f;
    [SerializeField] private GameObject impactEffect;

    private Rigidbody rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void TakeDamage(float amount, RaycastHit hit, float force)
    {
        rigidbody.AddForce(-hit.normal * force);

        GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(impact, 1f);

        health -= amount;

        if (health <= 0f)
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
