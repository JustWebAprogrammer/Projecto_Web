using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

[RequireComponent(typeof(Animator), typeof(ThirdPersonController))]
public class CrossbowCombatSystem : MonoBehaviour
{
	[Header("Crossbow Mechanics")]
	[SerializeField, Tooltip("Damage per bolt")] private float boltDamage = 15f;
	[SerializeField, Tooltip("Time between shots (reload + fire)")] private float fireRate = 0.8f;
	[SerializeField, Tooltip("Max firing range")] private float maxRange = 25f;
	[SerializeField, Tooltip("Layer for enemies")] private LayerMask enemyLayer;

	[Header("Visuals")]
	[SerializeField, Tooltip("Where the bolt spawns (crossbow barrel)")] private Transform firePoint;
	[SerializeField, Tooltip("Bolt prefab")] private GameObject boltPrefab;
	[SerializeField, Tooltip("Marker prefab for locked target")] private GameObject targetMarkerPrefab;
	[SerializeField, Tooltip("Material with outline for target highlight")] private Material targetOutlineMaterial;

	[Header("Targeting")]
	[SerializeField, Tooltip("Height of marker above target")] private float markerHeight = 1f;

	private float lastFireTime = -1f;
	public bool isAiming = false;
	private Transform currentTarget;
	private GameObject currentMarker;
	private Renderer targetRenderer;
	private Material originalMaterial;
	private List<Transform> visibleEnemies = new List<Transform>();
	private Animator _animator;
	private ThirdPersonController _controller;
	private StarterAssetsInputs _input;
	private CombatSystem _combatSystem;
	[SerializeField, Tooltip("Rotation Snap")] private float snapRotationSpeed = 20f;
	private Vector3 snapDirection;
	private Camera _mainCamera;
	private float lastTargetSwitchTime = -1f;
	private float targetSwitchDelay = 0.3f;
	private float defaultFOV;
	private AudioSource audioSource;
	
	
	[Header("Aiming Polish")]
	[SerializeField, Tooltip("Field of view when aiming (lower = zoomed in)")] private float aimFOV = 50f;
	[SerializeField, Tooltip("Speed of FOV transition")] private float fovTransitionSpeed = 5f; // Ensure this is here
	[SerializeField, Tooltip("Sound to play when locking onto a target")] private AudioClip lockOnSound;
	
	
	
	[Header("Animation Layer Settings")]
	[SerializeField, Tooltip("Index of the upper body layer in the Animator")] 
	private int upperBodyLayerIndex = 1;
	private float upperBodyWeight = 0f;
	private bool isCrossbowEquipped = false; // Tracks if crossbow is active
	
	 


	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_controller = GetComponent<ThirdPersonController>();
		_input = GetComponent<StarterAssetsInputs>();
		_combatSystem = GetComponent<CombatSystem>();
		_mainCamera = Camera.main;
		if (_mainCamera == null)
		{
			Debug.LogError("Main Camera not found—please tag your camera as 'MainCamera'!");
		}
		else
		{
			defaultFOV = _mainCamera.fieldOfView;
		}

		audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
		}
		if (_animator.layerCount <= upperBodyLayerIndex)
		{
			Debug.LogError($"Upper body layer index {upperBodyLayerIndex} exceeds Animator layers ({_animator.layerCount})—fix this or face a *Game Over* screen!");
		}
	}









	private void Update()
	{
		if (_controller.currentWeaponType != 2)
		{
			if (isAiming || _input.aim || isCrossbowEquipped)
			{
				isAiming = false;
				_input.aim = false;
				isCrossbowEquipped = false;
				_animator.SetBool("IsAiming", false);
				_animator.SetBool("IsCrossbowEquipped", false);
				ClearTargetVisuals();
				Debug.Log("Crossbow unequipped—upper body layer off like *Samus* stowing her arm cannon!");
			}
			upperBodyWeight = Mathf.Lerp(upperBodyWeight, 0f, Time.deltaTime * 10f);
			_animator.SetLayerWeight(upperBodyLayerIndex, upperBodyWeight);
			return;
		}

		// Crossbow is equipped
		isCrossbowEquipped = true;
		HandleAiming();
		HandleFiring();

		// Adjust upper body layer weight: 1 when equipped, modulated by aiming
		float targetWeight = isCrossbowEquipped ? 1f : 0f;
		upperBodyWeight = Mathf.Lerp(upperBodyWeight, targetWeight, Time.deltaTime * 10f);
		_animator.SetLayerWeight(upperBodyLayerIndex, upperBodyWeight);
	}

	private void LateUpdate()
	{
		UpdateTargetVisuals();
	}

	private void HandleAiming()
	{
		 

		if (!isCrossbowEquipped) return;

		isAiming = _input.aim;
		_animator.SetBool("IsAiming", isAiming);
		_animator.SetBool("IsCrossbowEquipped", true); // Reinforce this state


		if (_mainCamera != null)
		{
			float targetFOV = isAiming ? aimFOV : defaultFOV;
			_mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
		}

		if (isAiming)
		{
			UpdateVisibleEnemies();

			if (visibleEnemies.Count > 0)
			{
				Vector3 cameraForward = _mainCamera.transform.forward;
				cameraForward.y = 0;
				cameraForward.Normalize();

				Transform bestTarget = FindBestTarget(cameraForward);

				if (Time.time - lastTargetSwitchTime >= targetSwitchDelay || currentTarget != bestTarget)
				{
					if (currentTarget != bestTarget)
					{
						currentTarget = bestTarget;
						lastTargetSwitchTime = Time.time;
						if (lockOnSound != null && audioSource != null)
						{
							audioSource.PlayOneShot(lockOnSound);
						}
						Debug.Log($"Locked onto new target: {currentTarget?.name ?? "null"}");
					}
				}
			}
			else
			{
				currentTarget = null;
			}

			if (currentTarget != null)
			{
				Vector3 direction = (currentTarget.position - transform.position).normalized;
				direction.y = 0;
				Quaternion targetRotation = Quaternion.LookRotation(direction);
				float rotationSpeed = Time.deltaTime * snapRotationSpeed * 20f;
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
			}
			else
			{
				Vector3 cameraForward = _mainCamera.transform.forward;
				cameraForward.y =0;
				Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
				float rotationSpeed = Time.deltaTime * snapRotationSpeed * 10f;
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
			}
		}
		else
		{
			ClearTargetVisuals();
			visibleEnemies.Clear();
		}
	}

	private void HandleFiring()
	{
		if (_input.fire && !_combatSystem.isAttacking && !_controller.isRolling && !_controller.Isblock)
		{
			if (Time.time - lastFireTime >= fireRate)
			{
				if (!isAiming)
				{
					DetermineSnapDirection();
					StartCoroutine(SnapToDirection());
				}
				else
				{
					snapDirection = currentTarget != null ? (currentTarget.position - transform.position).normalized : transform.forward;
				}

				FireBolt();
				lastFireTime = Time.time;
				_animator.SetTrigger("Fire");
				_input.fire = false;
				Debug.Log("Crossbow fired—bolt away! *THUNK*");
			}
		}
	}

	private void DetermineSnapDirection()
	{
		UpdateVisibleEnemies();
		Transform nearestEnemy = null;
		float bestScore = float.MinValue;

		Vector3 cameraForward = _mainCamera.transform.forward;
		cameraForward.y = 0;
		cameraForward.Normalize();

		foreach (Transform enemy in visibleEnemies)
		{
			Vector3 enemyDir = (enemy.position - transform.position).normalized;
			float distance = Vector3.Distance(transform.position, enemy.position);
			float angleDot = Vector3.Dot(cameraForward, enemyDir);
			float score = angleDot * 2f - (distance * 0.05f);

			if (score > bestScore)
			{
				bestScore = score;
				nearestEnemy = enemy;
			}
		}

		if (nearestEnemy != null)
		{
			snapDirection = (nearestEnemy.position - transform.position).normalized;
			snapDirection.y = 0;
			Debug.Log($"Snapping to nearest enemy: {nearestEnemy.name}");
		}
		else
		{
			snapDirection = cameraForward;
			Debug.Log("No enemies in range—snapping to camera forward direction!");
		}
	}

	private IEnumerator SnapToDirection()
	{
		Quaternion targetRotation = Quaternion.LookRotation(snapDirection);
		float elapsed = 0f;
		float snapDuration = 0.1f;

		while (elapsed < snapDuration)
		{
			elapsed += Time.deltaTime;
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, snapRotationSpeed * Time.deltaTime);
			yield return null;
		}

		transform.rotation = targetRotation;
		Debug.Log("Snap complete—locked on and ready to fire!");
	}

	private void FireBolt()
	{
		Vector3 direction;
		
		if (isAiming && currentTarget != null)
		{
			direction = (currentTarget.position - firePoint.position).normalized;
			direction.y=0;
		}
		else
		{
			direction = firePoint.forward;
			StartCoroutine(SnapToDirection());
		}

		if (!boltPrefab)
		{
			Debug.LogError("Bolt prefab missing—can’t fire! Magic’s outta juice!");
			return;
		}

		GameObject bolt = Instantiate(boltPrefab, firePoint.position, Quaternion.LookRotation(direction));
		Rigidbody rb = bolt.GetComponent<Rigidbody>();
		if (!rb)
		{
			Debug.LogWarning("Bolt prefab needs a Rigidbody—adding one, but fix this!");
			rb = bolt.AddComponent<Rigidbody>();
		}
		rb.velocity = direction * 30f;
		Destroy(bolt, 2f);

		if (Physics.Raycast(firePoint.position, direction, out RaycastHit hit, maxRange, enemyLayer))
		{
			ApplyDamage(hit.collider.gameObject, boltDamage);
			Debug.Log($"Bolt hit {hit.collider.name} for {boltDamage} damage!");
		}
		else
		{
			Debug.Log("Bolt missed—magic’s not perfect today!");
		}
	}

	private void UpdateTargetVisuals()
	{
		if (isAiming && currentTarget != null)
		{
			if (!currentMarker)
			{
				currentMarker = Instantiate(targetMarkerPrefab, currentTarget.position + Vector3.up * markerHeight, Quaternion.identity);
				if (_mainCamera != null)
				{
					currentMarker.transform.rotation = Quaternion.LookRotation(_mainCamera.transform.forward);
				}
			}
			currentMarker.transform.position = currentTarget.position + Vector3.up * markerHeight;

			if (targetRenderer == null || targetRenderer.transform != currentTarget)
			{
				ClearTargetVisuals();
				targetRenderer = currentTarget.GetComponent<Renderer>();
				if (targetRenderer && targetOutlineMaterial)
				{
					originalMaterial = targetRenderer.material;
					targetRenderer.material = targetOutlineMaterial;
				}
			}
		}
		else
		{
			ClearTargetVisuals();
		}
	}

	private void ClearTargetVisuals()
	{
		if (currentMarker)
		{
			Destroy(currentMarker);
			currentMarker = null;
		}
		if (targetRenderer)
		{
			if (originalMaterial) targetRenderer.material = originalMaterial;
			targetRenderer = null;
		}
	}

	private void UpdateVisibleEnemies()
	{
		visibleEnemies.Clear();
		Collider[] enemies = Physics.OverlapSphere(transform.position, maxRange, enemyLayer);
		foreach (Collider enemy in enemies)
		{
			Vector3 direction = (enemy.transform.position - transform.position).normalized;
			if (Vector3.Dot(_mainCamera.transform.forward, direction) > 0.9f)
			{
				visibleEnemies.Add(enemy.transform);
			}
		}
		visibleEnemies.Sort((a, b) => Vector3.Distance(transform.position, a.position).CompareTo(Vector3.Distance(transform.position, b.position)));
	}

	private Transform FindBestTarget(Vector3 aimDirection)
	{
		Transform bestTarget = null;
		float bestScore = float.MinValue;

		foreach (Transform enemy in visibleEnemies)
		{
			Vector3 enemyDir = (enemy.position - transform.position).normalized;
			float distance = Vector3.Distance(transform.position, enemy.position);
			float angleDot = Vector3.Dot(aimDirection, enemyDir);
			if (angleDot < 0.8f) continue;
			float score = angleDot * 3f - (distance * 0.05f);

			if (score > bestScore)
			{
				bestScore = score;
				bestTarget = enemy;
			}
		}
		return bestTarget;
	}

	private void ApplyDamage(GameObject enemy, float damage)
	{
		// Implement your damage logic here
	}

	private void OnDrawGizmos()
	{
		if (firePoint)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawRay(firePoint.position, transform.forward * maxRange);
		}
	}
}