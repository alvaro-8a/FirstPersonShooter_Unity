using UnityEngine;

public class Gun : MonoBehaviour
{
	private const string SHOOT_TRIGGER = "Shoot";

	[SerializeField] private float damage = 10f;
	[SerializeField] private float range = 100f;
	[SerializeField] private float impactForce = 30f;
	[SerializeField] private float fireRate = 15f;

	[SerializeField] private Camera fpsCam;
	[SerializeField] private ParticleSystem muzzleFlash;

	private AudioSource audioSource;
	private Animator animator;
	private float nextTimetoFire = 0f;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		animator = GetComponent<Animator>();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetButton("Fire1") && Time.time >= nextTimetoFire)
		{
			nextTimetoFire = Time.time + 1f / fireRate;
			Shoot();
		}
	}

	void Shoot()
	{
		animator.SetTrigger(SHOOT_TRIGGER);
		CameraShake.Instance.shakeDuration = 0.5f;
		muzzleFlash.Play();
		// audioSource.pitch = 1 + Random.Range(-0.1f, 0.1f);
		audioSource.Play();


		RaycastHit hit;
		if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
		{
			// Target target = hit.transform.GetComponent<Target>();
			// if (target != null)
			// {
			// 	target.TakeDamage(damage, hit, impactForce);
			// }
		}
	}
}
