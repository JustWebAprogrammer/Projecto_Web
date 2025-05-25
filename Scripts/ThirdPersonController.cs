using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;

#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
	    public float MoveSpeed = 2.0f;
        
	    private bool _rootMotionActive = false;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
	    public LayerMask GroundLayers;
        
        
	    [Header("Combat System")]
	    public CombatSystem combatSystem;
      
	    private CrossbowCombatSystem _bowCombatSystem;
	    private string lastStateSubmachine = "Locomotion"; // Default to base locomotion
      
	    [Header("BlockSystem")]
	    [Tooltip("If he is blocking dumbass")]
	    public bool Isblock= false;        
        
	    [Tooltip("The amount of block defense")]
	    public float bloackreductiondamage=0.5f;
        
	    [Header("Attack Feedback")]
	    public float lightAttackFreezeFrameDuration = 0.03f;
	    public float heavyAttackFreezeFrameDuration = 0.06f;
	    
    	[Header("Weapon System")]
    	public int currentWeaponType = 0;
    	private int _animIDWeaponType;
	    
	    public GameObject SwordSheathed;
	    public GameObject SwordEquipped; 
	    public GameObject BowStowed;
	    public GameObject BowEquipped;	  
	    
	    private float weaponSwitchCooldown = 0.25f; // Half second cooldown between switches
	    private float lastWeaponSwitchTime = -1f;
	    
	    [Header ("Shield Settings")]
	    public GameObject shield;
	    public Transform shieldLoweredPosition;
	    public Transform shieldRaisedPosition;
	    public float shieldTransitionSpeed = 8.0f;
	    private bool shouldRaisedShield = false;
	    
	    
	    
	    [Header ("Roll Settings")]
	    [Tooltip("Roll Movement Speed Multiplier")]
	    public float rollSpeedMultiplier = 1.5f;
		
	    public bool invincibleDuringRoll = true;
	    
	    [Tooltip("Animation curve for roll speed")]
	    public AnimationCurve rollSpeedCurve;
		
	    [Tooltip("Total distance the roll should cover")]
	    public float rollDistance = 5.0f;

	    [Tooltip("Base roll speed")] 
	    public float rollSpeed = 6f;

	    public float rollRotationSpeed = 10f;

	    [Tooltip("Duration of roll in seconds")]
	    public float rollDuration= 0.6f;
	    
	    [Tooltip("Minimum input magnitude to trigger roll")]
	    public float rollInputThreshold = 0.7f;
	    
	    [Tooltip("If player is currently rolling")]
	    public bool isRolling = false;
	    public float rollCooldown = 1.0f;
	    private float rollTimer;
	    private Vector3 rollDirection;
	    private bool blockedHeldduringRoll;




        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;
	    private Vector3 localHitDirection;
        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
	        _input = GetComponent<StarterAssetsInputs>();
	        _bowCombatSystem = GetComponent<CrossbowCombatSystem>();
            
	        // Initialize root motion based on starting state
	        if (_hasAnimator)
	        {
		        _animator.applyRootMotion = false; // Start with locomotion (root motion OFF)
		        Debug.Log("Root Motion: OFFLINE—locomotion’s in the driver’s seat! 🚗");
	        }
            
            
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	        _playerInput = GetComponent<PlayerInput>();
	      
	        if (combatSystem == null)
	        {
		        combatSystem = GetComponent<CombatSystem>();
	        }
	      
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
	        _fallTimeoutDelta = FallTimeout;
            
	        UpdateWeaponVisuals();
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
	        GroundedCheck();
	        HandleRoll();
	        HandleWeaponSwitching();
	        HandleBlocking();
	        // Update root motion based on current state
	        UpdateRootMotion();
	          
	        CrossbowCombatSystem bowCombat = GetComponent<CrossbowCombatSystem>();
	        bool isAiming = bowCombat != null && bowCombat.isAiming; // Access isAiming (make it public)

	        if (_bowCombatSystem != null && currentWeaponType == 2)
	        {
		        _animator.SetBool("IsAiming", _bowCombatSystem.isAiming); // Sync every frame
	        }


	        if (_hasAnimator && !isRolling)
	        {
		        AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
		        if (currentState.IsTag("Locomotion"))
			        lastStateSubmachine = "Locomotion";
		        else if (currentState.IsTag("LightAttack") || currentState.IsTag("HeavyAttack"))
			        lastStateSubmachine = "Combat";
	        }




	        if (!isRolling && !Isblock && !combatSystem.isAttacking && !isAiming)
	        {
		        Move(); // Only move if not aiming
	        }
	        else if (combatSystem.isAttacking)
	        {
		        ApplyAnimationMovement();
	        }
	      
        }
        
        
        
        
	    private void ApplyAnimationMovement()
	    {
	    	
	    	// Keep applying gravity
		    if (_verticalVelocity < _terminalVelocity)
		    {
			    _verticalVelocity += Gravity * Time.deltaTime;
		    }
    
		    // Apply only vertical movement from gravity
		    _controller.Move(new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    
		    // Keep animator updated
		    if (_hasAnimator)
		    {
			    _animator.SetFloat(_animIDSpeed, 0);
			    _animator.SetFloat(_animIDMotionSpeed, 0);
		    }
	    	
	    	
	    }
        
	    private void UpdateRootMotion()
	    {
		    if (!_hasAnimator) return;

		    // Check the current animator state
		    AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);

		    // Enable root motion for attacks, rolls, or specific weapon types
		    bool shouldUseRootMotion = isRolling || 
		    (currentWeaponType == 1 && combatSystem.isAttacking) || 
			    currentState.IsTag("LightAttack") || 
			    currentState.IsTag("HeavyAttack") || 
			    currentState.IsTag("Roll");

		    // Disable root motion only when transitioning to a locomotion state
		    if (!shouldUseRootMotion && currentState.IsTag("Locomotion"))
		    {
			    _rootMotionActive = false;
			    _animator.applyRootMotion = false;
			   
		    }
		    else if (shouldUseRootMotion != _rootMotionActive)
		    {
			    _rootMotionActive = shouldUseRootMotion;
			    _animator.applyRootMotion = _rootMotionActive;
			    Debug.Log(_rootMotionActive 
				    ? $"Root Motion: ACTIVATED—Rolling or Attacking (Type: {combatSystem.currentAttackType})! 🎬 | Time: {Time.time}" 
		    : $"Root Motion: DEACTIVATED—Locomotion’s back! 🏃‍♂️ | Time: {Time.time}");
		    }
	    }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
	        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
	        _animIDWeaponType = Animator.StringToHash("WeaponType");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }



	    private void HandleWeaponSwitching()
	    {
	    	if (Time.time < lastWeaponSwitchTime + weaponSwitchCooldown) return;

		    if (_input.weaponSwitch1 && !combatSystem.isAttacking && !isRolling)
		    {
			    if (currentWeaponType == 0) // Already unarmed, do nothing
			    {
				    _input.weaponSwitch1 = false;
				    return;
			    }
			    else
			    {
				    SwitchWeapon(0); // Switch to unarmed
				    lastWeaponSwitchTime = Time.time;
			    }
		    }
		    else if (_input.weaponSwitch2 && !combatSystem.isAttacking && !isRolling)
		    {
			    if (currentWeaponType == 1) // Sword active, toggle to unarmed
			    {
				    SwitchWeapon(0);
			    }
			    else // Switch to sword
			    {
				    SwitchWeapon(1);
			    }
			    lastWeaponSwitchTime = Time.time;
		    }
		    else if (_input.weaponSwitch3 && !combatSystem.isAttacking && !isRolling)
		    {
			    if (currentWeaponType == 2) // Crossbow active, toggle to unarmed
			    {
				    SwitchWeapon(0);
			    }
			    else // Switch to crossbow
			    {
				    SwitchWeapon(2);
			    }
			    lastWeaponSwitchTime = Time.time;
		    }
	    }
	    
	    private void SwitchWeapon(int weaponType)
	    {
	    	if (currentWeaponType == weaponType) return;

		    // Reset combat state
		    combatSystem.isAttacking = false;
		    combatSystem.ResetCombo();
		    Isblock = false;

		    if (_hasAnimator)
		    {
			    _animator.SetBool("IsAttacking", false);
			    _animator.SetBool("IsBlocking", false);
			    _animator.SetBool("IsAiming", false);
		    }

		    int previousWeapon = currentWeaponType;
		    currentWeaponType = weaponType;

		    if (_bowCombatSystem != null)
		    {
			    _bowCombatSystem.isAiming = false; // Reset crossbow aiming
		    }


		    // Clear all weapon switch inputs
		    _input.weaponSwitch1 = _input.weaponSwitch2 = _input.weaponSwitch3 = false;

		    if (_hasAnimator)
		    {
			    // Trigger unsheathing animation directly
			    _animator.SetInteger(_animIDWeaponType, currentWeaponType);
			    _animator.SetInteger("PreviousWeapon", previousWeapon);
			    _animator.SetTrigger("WeaponSwitch");

			   
		    }

		    // Update root motion based on new weapon state
		    UpdateRootMotion();
            
	    }
	    
	    
	    public void EquipSword()
	    {
		    if (SwordSheathed != null) SwordSheathed.SetActive(false);
		    if (SwordEquipped != null) SwordEquipped.SetActive(true);
		    if (BowStowed != null) BowStowed.SetActive(true);
		    if (BowEquipped != null) BowEquipped.SetActive(false);
	    }

	    public void UnequipSword()
	    {
		    if (SwordSheathed != null) SwordSheathed.SetActive(true);
		    if (SwordEquipped != null) SwordEquipped.SetActive(false);
	    }

	    public void EquipBow()
	    {
		    if (BowStowed != null) BowStowed.SetActive(false);
		    if (BowEquipped != null) BowEquipped.SetActive(true);
		    if (SwordSheathed != null) SwordSheathed.SetActive(true);
		    if (SwordEquipped != null) SwordEquipped.SetActive(false);
	    }

	    public void UnequipBow()
	    {
		    if (BowStowed != null) BowStowed.SetActive(true);
		    if (BowEquipped != null) BowEquipped.SetActive(false);
	    }

	    public void RaiseShield()
	    {
		    shouldRaisedShield = true;
	    }

	    public void LowerShield()
	    {
		    shouldRaisedShield = false;
	    }

	    private void UpdateShieldPosition()
	    {
		    if (shield == null) return;
            
		    Transform targetTransform = shouldRaisedShield ? shieldRaisedPosition : shieldLoweredPosition;
		    if (targetTransform == null) return;
            
		    // Smoothly interpolate position and rotation
		    shield.transform.localPosition = Vector3.Lerp(
			    shield.transform.localPosition, 
			    targetTransform.localPosition, 
			    Time.deltaTime * shieldTransitionSpeed);
                
		    shield.transform.localRotation = Quaternion.Slerp(
			    shield.transform.localRotation,
			    targetTransform.localRotation,
			    Time.deltaTime * shieldTransitionSpeed);
	    }
        
	    private void UpdateWeaponVisuals()
	    {
		    // Set initial states for all weapons
		    if (SwordSheathed != null) SwordSheathed.SetActive(currentWeaponType != 1);
		    if (SwordEquipped != null) SwordEquipped.SetActive(currentWeaponType == 1);
		    if (BowStowed != null) BowStowed.SetActive(currentWeaponType != 2);
		    if (BowEquipped != null) BowEquipped.SetActive(currentWeaponType == 2);
            
		    // Shield position is handled by UpdateShieldPosition method
	    }

	    

	   
	  


	    private void HandleRoll()
	    {
	    	if (rollTimer > 0)
	    	{
		    	rollTimer -= Time.deltaTime;
		    	return;
	    	}

		    if (_input.roll && !isRolling && CanRollInterrupt())
		    {
			    Debug.Log("STARTING ROLL—dodging like *Link* evading a Lynel charge!");
			    combatSystem._nextAttackQueued = false; // Clear queued attacks—like canceling a *Smash Bros.* combo!
			    if (combatSystem.isAttacking)
			    {
				    combatSystem.EndAttack(); // End attack cleanly
				    Debug.Log("Attack interrupted—roll takes over like a *Sekiro* mikiri counter!");
			    }
			    StartRoll();
			    _input.roll = false; // Prevent roll spam—like a cooldown on a *Monster Hunter* dodge!
		    }
		    else
		    {
			    Debug.Log($"Roll Blocked - Input: {_input.roll}, IsRolling: {isRolling}, CanRollInterrupt: {CanRollInterrupt()}");
		    }
	    }

	    private bool CanRollInterrupt()
	    {
		    if (combatSystem == null || !_hasAnimator)
		    {
			    Debug.LogWarning("CanRollInterrupt FAILED—missing combatSystem or animator! It’s like forgetting the Master Sword!");
			    return true; // Fallback—don’t punish missing setup!
		    }

		    if (!combatSystem.isAttacking)
		    {
			    Debug.Log("No attack—roll free like Sonic on a speed pad!");
			    return true;
		    }

		    // Rely on input blocking—roll only triggers if CanInterruptWithRoll was true
		    return true; // Always true here, since input is already filtered
	    }
	    
	    private void StartRoll()
	    {
	    	
	    	isRolling = true;
		    rollTimer = rollCooldown;
		    Debug.Log($"Roll Started with WeaponType: {currentWeaponType}—dodging like Samus in a morph ball!");

			    Vector2 input = _input.move.normalized;
			    Vector3 worldDirection = _mainCamera.transform.TransformDirection(new Vector3(input.x, 0, input.y));
			    worldDirection.y = 0;
		    worldDirection.Normalize();
		    rollDirection = worldDirection.magnitude > 0.1f ? worldDirection : transform.forward; // Default to forward if no input

		    _animator.SetBool("IsRolling", true);
		    _animator.SetFloat("RollX", rollDirection.x);
		    _animator.SetFloat("RollZ", rollDirection.z);
		    _animator.SetTrigger("Roll");
		    _animator.SetInteger("WeaponType", currentWeaponType);

		    if (_hasAnimator)
		    {
			    _animator.applyRootMotion = true;
			    _rootMotionActive = true;
			    Debug.Log("Rollin’ with ROOT MOTION—like a Sekiro shinobi flip!");
		    }

		    StartCoroutine(PerformRoll());
	    }

	    private IEnumerator PerformRoll()
	    {
		    float elapsed = 0f;
		    while (elapsed < rollDuration)
		    {
			    elapsed += Time.deltaTime;
			    yield return null;
		    }

		    isRolling = false;
		    _animator.SetBool("IsRolling", false);
		    Debug.Log($"Roll Ended! Back to WeaponType {currentWeaponType}—smooth as Mario exiting a pipe!");

		    if (invincibleDuringRoll) gameObject.layer = LayerMask.NameToLayer("Character"); // Reset layer
		    _animator.SetTrigger("ReturnToLocomotion");
		    _animator.SetInteger("WeaponType", currentWeaponType);
		    UpdateRootMotion();
	    }
	    
	    
	    
	    // Helper method to name attacks for debug
	    private string GetAttackName(int attackType)
	    {
		    switch (attackType)
		    {
		    case 0: return "LightAttack1";
		    case 1: return "LightAttack2";
		    case 2: return "LightAttack3";
		    case 3: return "HeavyAttack1";
		    case 4: return "HeavyAttack2";
		    default: return "Unknown Attack";
		    }
	  
	    }
	    private void HandleBlocking()
	    {
		    if (!isRolling)
		    {
			    bool wantsToBlock = _input.block;
        
			    // Only change state if input state doesn't match current block state
			    if (wantsToBlock != Isblock)
			    {
				    if (wantsToBlock) 
				    {
					    StartBlocking();
				    }
				    else 
				    {
					    StopBlocking();
	    	
	    	
	    	}
	    	
	    	
	    }
    
		    }
	    }
	    private void StartBlocking()
	    {
	    	
	    	Isblock = true;
	    	RaiseShield();
	    	
	    	
		    _animator.SetBool("IsBlocking", true);
	    	
	    	
	    	
	    }
	    
	    private void StopBlocking()
	    {
	    	Isblock = false;
	    	
		    LowerShield();
	    	
	    	_animator.SetBool("IsBlocking",false);
	    	
	    	
	    }



	    public void TakeHit(Vector3 hitDirection, float damage)
	    {
	    	
	    	int hitDirIndex = GetHitDirectionIndex(hitDirection);
		    _animator.SetInteger("HitDirection", hitDirIndex);
		    _animator.SetTrigger("TakeHit");

		    if (Isblock)
		    {
			    damage *= bloackreductiondamage;
			    _animator.SetTrigger("ShieldRecoil");
			    Debug.Log("Shield recoil!");
		    }
		    else
		    {
			    // Reset combo if not blocking
			    combatSystem.ResetCombo();
		    }

		    Debug.Log($"Player took {damage} damage!");
	    	
	    }
	    
	    
	    private int GetHitDirectionIndex(Vector3 localHitDirection)
	    {
		    if (localHitDirection.z > 0.5f) return 0; // Front
		    if (localHitDirection.z < -0.5f) return 1; // Back
		    if (localHitDirection.x > 0.5f) return 2; // Right
		    if (localHitDirection.x < -0.5f) return 3; // Left
		    return 0; // Default to front
	    }

	    
	    public void DisableRootMotionSmoothly()
	    {
		    StartCoroutine(DisableRootMotionDelayed());
	    }

	    private IEnumerator DisableRootMotionDelayed()
	    {
		    yield return null; // Wait one frame for smoothness
		    if (!isRolling && currentWeaponType != 1)
		    {
			    _animator.applyRootMotion = false;
			    Debug.Log("Root Motion OFF—smooth as a Moogle’s glide after a Kupo Nut snack! 🏃‍♂️");
		    }
	    }

	
	private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}