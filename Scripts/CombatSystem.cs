using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using System;
public class CombatSystem : MonoBehaviour
{
	[Header("Attack Configuration")]
	[SerializeField] private float lightAttack1Damage = 10f;
	[SerializeField] private float lightAttack2Damage = 15f;
	[SerializeField] private float lightAttack3Damage = 20f;
	[SerializeField] private float heavyAttack1Damage = 25f;
	[SerializeField] private float heavyAttack2Damage = 35f;
    
	[Header("Combo System")]
	[SerializeField] public float comboWindow = 1.5f;
	private float defaultComboWindow = 1.5f; // Store default for reference
	[SerializeField] public int maxComboCount = 3;
	[SerializeField] private float inputBufferTime = 0.25f;
	[SerializeField] private float attackTransitionDelay = 0.033f; // Delay between attack animations
	public bool _isLastComboCountReached = false;
	public bool _inputAcceptedThisAttack = false; // Tracks if an input was accepted for this attack
    
	[Header("Attack Animation Durations")]
	public float lightAttack1Duration = 0.68f; // 1.23 / 1.8
	public float lightAttack2Duration = 0.70f; // 1.27 / 1.5
	public float lightAttack3Duration = 2.32f; // 3.25 / 1.5
	public float heavyAttack1Duration = 1.69f; // 1.35 / 1.5
	public float heavyAttack2Duration = 2.59f; // 2.07 / 1.5
    
	// Define delegate for queue check before exit
	public delegate void QueueCheckedBeforeExitHandler(bool isQueued);

	// Event to trigger when queue is checked before locomotion
	public event QueueCheckedBeforeExitHandler OnQueueCheckedBeforeExit;
    
	private float attackCooldown = 0.05f; // Adjust as needed
	private float lastSuccessfulAttackTime = -1f;
	
	[Header("Combo Threshold Events")]
	[SerializeField] private float lightComboThresholdStart = 0.2f; // Quick as a Deku Scrub dodging a boulder!
	[SerializeField] private float lightComboThresholdEnd = 0.6f;   // Tight like Samus rolling into a ball!
	[SerializeField] private float heavyComboThresholdStart = 0.3f; // Slower, like a Goron rolling downhill!
	[SerializeField] private float heavyComboThresholdEnd = 0.8f;
	
	[SerializeField] private float lightComboGracePeriod = 0.2f; // Extra time to keep the combo alive, like a Mario star power boost!
	[SerializeField] private float heavyComboGracePeriod = 0.3f; // More leeway for heavy swings, like a Goron catching a rolling boulder!
	private float comboThresholdCloseTime = -1f; // When the combo window officially shuts—like a dungeon door slamming!
	
	// Define custom delegates without using Action
	public delegate void ComboThresholdEnteredHandler(int attackType);
	public delegate void ComboThresholdExitedHandler();
    
	// Events
	public event ComboThresholdEnteredHandler OnComboThresholdEntered;
	public event ComboThresholdExitedHandler OnComboThresholdExited;
	private bool inComboThreshold = false;
    
	public bool DontRoll = false;
	 
	[Header("Combo Paths")]
	// Define possible next attacks from each attack type
	// 0=none, 1=light, 2=heavy (using 0 instead of -1 for clarity)
	[SerializeField] private int[] lightAttack1NextValid = { 1, 2 }; // Can go to light2 or heavy1
	[SerializeField] private int[] lightAttack2NextValid = { 1, 2 }; // Can go to light3 or heavy2
	[SerializeField] private int[] lightAttack3NextValid = { 0 };    // End of combo, no followup
	[SerializeField] private int[] heavyAttack1NextValid = { 1, 2 }; // Can go to light3 or heavy2
	[SerializeField] private int[] heavyAttack2NextValid = { 0 };    // End of combo, no followup
    
	// Current attack state tracking
	public int lastAttackTypePerformed = 0; // 0=none, 1=light, 2=heavy
	public int lastAttackIndexPerformed = -1; // -1=none, 0/1/2=light index, 0/1=heavy index
    
    
	// Attack state tracking
	private float currentAttackDuration;
	public float attackStartTime;
	public int currentAttackType = -1;
    
	// Combo tracking
	public int currentComboCount = 0;
	private float lastAttackTime = -100f;
	public bool _nextAttackQueued = false;
	public int _nextAttackType = 0; // 0 for light, 1 for heavy
	public bool IsNextAttackQueued => _nextAttackQueued;
    
	// Animation tracking
	public int currentLightAttackIndex = 0;
	public int currentHeavyAttackIndex = 0;
	public bool isAttacking = false;
    
	// Input buffer
	public float lastLightAttackInputTime = -1f;
	public float lastHeavyAttackInputTime = -1f;
    
	// Components
	private Animator _animator;
	private ThirdPersonController _controller;
	private StarterAssetsInputs _input;
    
	private static bool _canInterruptWithRoll = true;
	public static bool CanInterruptWithRoll => _canInterruptWithRoll; // Read-only property
    
    
    
	public void Awake()
	{
		_animator = GetComponent<Animator>();
		_controller = GetComponent<ThirdPersonController>();
		_input = GetComponent<StarterAssetsInputs>();
		SetCanInterruptWithRoll(true); // Reset on awake—like a *Mario* 1-UP!	
	}
    
	private void Start()
	{
		// Ensure all animation states are properly reset
		if (_animator)
		{
			_animator.SetBool("IsAttacking", false);
			_animator.SetInteger("ComboCount", 0);
			_animator.SetInteger("AttackType", -1);
			
			// Setup combo transitions
			SetupComboTransitions();
		}
	}
    
	private void Update()
	{
		
		if (isAttacking && currentAttackType >= 3) // Heavy attacks are types 3 and 4
		{
			float heavyAttackTimeout = currentAttackType == 3 ? heavyAttack1Duration : heavyAttack2Duration;
    
			// Use a slightly shorter timeout for heavy attacks to ensure they don't stick
			if (Time.time - attackStartTime > heavyAttackTimeout * 0.95f)
			{
				Debug.Log("Heavy attack ended by timeout");
				EndAttack();
			}
		}
		
		
		// Reset combo after window expires
		if (Time.time - lastAttackTime > comboWindow && !isAttacking)
		{
			ResetCombo();
		}
            
		if (isAttacking && _animator)
			
			if (isAttacking && _animator)
			{
    AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
    bool inAttackState = stateInfo.IsTag("LightAttack") || stateInfo.IsTag("HeavyAttack");

    if (inAttackState)
    {
	    float normalizedTime = stateInfo.normalizedTime;
	    float thresholdStart = currentAttackType >= 3 ? heavyComboThresholdStart : lightComboThresholdStart;
	    float thresholdEnd = currentAttackType >= 3 ? heavyComboThresholdEnd : lightComboThresholdEnd;

	    if (!inComboThreshold && normalizedTime >= thresholdStart && normalizedTime <= thresholdEnd)
	    {
		    inComboThreshold = true;
		    comboThresholdCloseTime = -1f; // Reset when entering—fresh start, like a new *Hades* run!
		    if (OnComboThresholdEntered != null)
			    OnComboThresholdEntered(currentAttackType);
		    Debug.Log($"Combo threshold ENTERED for attack type {currentAttackType}");
	    }
	    else if (inComboThreshold && (normalizedTime < thresholdStart || normalizedTime > thresholdEnd))
	    {
		    inComboThreshold = false;
		    comboThresholdCloseTime = Time.time; // Mark when it closes—like a shopkeeper shutting down for the night!
		    if (OnComboThresholdExited != null)
			    OnComboThresholdExited();
		    Debug.Log("Combo threshold EXITED—grace period starting!");
	    }
    }
			}	
		
		
		
		
		if (isAttacking && _animator)
		{
			AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
			if (stateInfo.IsTag("HeavyAttack"))
			{
				Debug.Log($"Heavy attack progress: {stateInfo.normalizedTime:F2}, State: {stateInfo.fullPathHash}");
			}
		}
            
          
			// Force end attack if it's been running too long
			float attackDuration = GetCurrentAttackDuration();
			if (Time.time - attackStartTime > attackDuration * 1.2f)
			{
				Debug.Log("Attack ended by timeout - animation may be stuck");
				EndAttack();
			}
		
        
		// Process player inputs
		HandleAttackInputs();
	}
    
	private float GetCurrentAttackDuration()
	{
		float baseDuration;
		// Return the appropriate duration based on the current attack type
		switch (currentAttackType)
		{
		case 0: return lightAttack1Duration;
		case 1: return lightAttack2Duration;
		case 2: return lightAttack3Duration;
		case 3: return heavyAttack1Duration;
		case 4: return heavyAttack2Duration;
		default: return 1.0f; // Default fallback
		}
		float animSpeed = _animator.GetCurrentAnimatorStateInfo(0).speed; // Get current speed
		return baseDuration / (animSpeed > 0 ? animSpeed : 1f); // Adjust for speed
	}
    
    
    
    
	public static void SetCanInterruptWithRoll(bool value)
	{
		_canInterruptWithRoll = value;
		Debug.Log($"CanInterruptWithRoll set to: {value}—like toggling a *Zelda* switch puzzle!");
	}
    
    
	public void ExtendComboWindowAfterRoll()
	{
		float rollComboExtension = 2.0f; // Extra 2 seconds—like a *Final Fantasy* ATB gauge boost!
		comboWindow = Mathf.Max(comboWindow, rollComboExtension);
		lastAttackTime = Time.time; // Reset timer to now, keeping the combo alive
		Debug.Log($"Combo window extended to {comboWindow}s post-roll—strike back like Dante after a dodge!");
	}
    
    
    
    
	private void HandleAttackInputs()
	{
		if (_controller.currentWeaponType == 2) return;
		if (_controller.currentWeaponType == 2) return;

		AnimatorTransitionInfo transitionInfo = _animator.GetAnimatorTransitionInfo(0);
		AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
		AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(0);

		if (transitionInfo.duration > 0 && 
		(currentState.IsTag("LightAttack") || currentState.IsTag("HeavyAttack")) && 
			nextState.IsTag("Locomotion"))
		{
			Debug.Log("Inputs LOCKED: Transitioning from attack to locomotion—like *Link* sheathing his sword! 🛡️");
			_input.lightAttack = false;
			_input.heavyAttack = false;
			return;
		}

		bool lightAttackRequested = _input.lightAttack;
		bool heavyAttackRequested = _input.heavyAttack;

		Debug.Log($"Attack Inputs - Light: {lightAttackRequested}, Heavy: {heavyAttackRequested}, Weapon: {_controller.currentWeaponType}");
		// Store timestamps
		if (lightAttackRequested) lastLightAttackInputTime = Time.time;
		if (heavyAttackRequested) lastHeavyAttackInputTime = Time.time;
		
		// If attacking, check animation progress
		if (isAttacking)
		{
			float adjustedDuration = GetCurrentAttackDuration();
			float attackProgress = (Time.time - attackStartTime) / adjustedDuration;
			float timeSinceStart = Time.time - attackStartTime;
			float bufferThreshold = adjustedDuration * 0.9f; // 90% mark
			float bufferWindow = 0.1f; // Buffer time after 90%

			if (attackProgress < 0.67f) // Block inputs before 67%
			{
				Debug.Log($"Inputs locked—animation at {attackProgress:P0} (Duration: {adjustedDuration:F2}s), wait like a *Zelda* cooldown! ⏳");
					return;
			}
			else if (timeSinceStart >= bufferThreshold && timeSinceStart < (bufferThreshold + bufferWindow))
			{
				Debug.Log($"Inputs blocked in buffer zone—animation at {attackProgress:P0}, Time: {timeSinceStart:F2}s, easing into locomotion like a *Mario* slide! 🚪");
				_input.lightAttack = false;
				_input.heavyAttack = false;
				return;
			}
		}

		// Player is not currently attacking
		if (!isAttacking)
		{
			bool bufferedLightAttack = !lightAttackRequested && 
			(Time.time - lastLightAttackInputTime < inputBufferTime);
			bool bufferedHeavyAttack = !heavyAttackRequested && 
			(Time.time - lastHeavyAttackInputTime < inputBufferTime);

			if ((lightAttackRequested || bufferedLightAttack) && CanStartNextAttack(1))
			{
				PerformNextLightAttack();
				lastSuccessfulAttackTime = Time.time;
				lastLightAttackInputTime = -1f;
				lastHeavyAttackInputTime = -1f; // Clear both buffers
				_input.lightAttack = false;
				_input.heavyAttack = false; // Reset all inputs
			}
			else if ((heavyAttackRequested || bufferedHeavyAttack) && CanStartNextAttack(2))
			{
				PerformNextHeavyAttack();
				lastSuccessfulAttackTime = Time.time;
				lastHeavyAttackInputTime = -1f;
				lastLightAttackInputTime = -1f; // Clear both buffers
				_input.heavyAttack = false;
				_input.lightAttack = false;
			}
		}
		// Player is attacking but might queue the next attack
		else if ((lightAttackRequested || heavyAttackRequested) && !_nextAttackQueued && !_inputAcceptedThisAttack)
		{
			float gracePeriod = currentAttackType >= 3 ? heavyComboGracePeriod : lightComboGracePeriod;
			bool inGracePeriod = !inComboThreshold && comboThresholdCloseTime >= 0f && 
			(Time.time - comboThresholdCloseTime) <= gracePeriod;

			if (inComboThreshold || inGracePeriod)
			{
				int requestedType = lightAttackRequested ? 1 : 2;

				if (IsValidComboSequence(requestedType))
				{
					_nextAttackQueued = true;
					_nextAttackType = requestedType;
					_inputAcceptedThisAttack = true;
					Debug.Log($"Next attack queued: {(_nextAttackType == 1 ? "Light" : "Heavy")}, " +
				$"Combo: {currentComboCount}, Type: {currentAttackType}, Time: {Time.time}");
					lastLightAttackInputTime = -1f; // Clear buffers after queuing
					lastHeavyAttackInputTime = -1f;
					_input.lightAttack = false;
					_input.heavyAttack = false;
				}
				else
				{
					Debug.Log($"Invalid combo sequence: Can't follow {GetAttackName()} with {(requestedType == 1 ? "Light" : "Heavy")}");
				}
			}
			else
			{
				Debug.Log("Input ignored—outside combo window!");
				lastLightAttackInputTime = -1f; // Clear buffers anyway
				lastHeavyAttackInputTime = -1f;
				_input.lightAttack = false;
				_input.heavyAttack = false;
			}
		}
	}

    
	private void PerformLightAttack()
	{
		if (!_animator || isAttacking) return; // Safety check
    
		isAttacking = true;
		attackStartTime = Time.time;
		lastAttackTime = Time.time;
		lastSuccessfulAttackTime = Time.time;
		currentComboCount++;
    
		// Cycle through light attack animations (0, 1, 2)
		int attackIndex = currentLightAttackIndex;
		currentLightAttackIndex = (currentLightAttackIndex + 1) % 3;
    
		// Set the current attack type
		currentAttackType = attackIndex;
    
		// Set animation parameters
		_animator.SetBool("IsAttacking", true);
		_animator.SetInteger("AttackType", attackIndex);
		_animator.SetInteger("ComboCount", currentComboCount);
		_animator.SetTrigger("Attack"); // Add a trigger to ensure animation transition
    
		Debug.Log($"Performing Light Attack {attackIndex+1}, Combo: {currentComboCount}");
    
			// Face the direction of attack
		FaceAttackDirection();
	}
    
	private void PerformHeavyAttack()
	{
		
		if (!_animator || isAttacking) return;
        
		isAttacking = true;
		attackStartTime = Time.time;
		lastAttackTime = Time.time;
		lastSuccessfulAttackTime = Time.time;
		currentComboCount++;
        
		// Cycle through heavy attack animations (0, 1)
		int attackIndex = currentHeavyAttackIndex;
		currentHeavyAttackIndex = (currentHeavyAttackIndex + 1) % 2;
        
		// Store the animation index for heavy attacks (3, 4)
		int animationIndex = attackIndex + 3;
		currentAttackType = animationIndex;
        
		// Set animation parameters
		_animator.SetBool("IsAttacking", true);
		_animator.SetInteger("AttackType", animationIndex);
		_animator.SetInteger("ComboCount", currentComboCount);
		_animator.SetTrigger("Attack"); // Add a trigger to ensure animation transition
        
		Debug.Log($"Performing Heavy Attack {attackIndex+1}, Combo: {currentComboCount}");
        
			// Face the direction of attack
		FaceAttackDirection();
	}
    
    
    
	// Check if attack type is valid as next in combo
	private bool IsValidComboSequence(int requestedAttackType)
	{
		// Get valid next attacks based on current attack
		int[] validNextAttacks = GetValidNextAttacks();
    
		// Check if requested attack type is in the valid list
		return System.Array.IndexOf(validNextAttacks, requestedAttackType) != -1;
	}

	// Get valid next attacks based on current attack state
	private int[] GetValidNextAttacks()
	{
		if (lastAttackTypePerformed == 1) // Light Attack
		{
			switch (lastAttackIndexPerformed)
			{
			case 0: return lightAttack1NextValid;  // From Light1
			case 1: return lightAttack2NextValid;  // From Light2
			case 2: return lightAttack3NextValid;  // From Light3
			}
		}
		else if (lastAttackTypePerformed == 2) // Heavy Attack
		{
			switch (lastAttackIndexPerformed)
			{
			case 0: return heavyAttack1NextValid;  // From Heavy1
			case 1: return heavyAttack2NextValid;  // From Heavy2
			}
		}
    
		// If starting a new combo, both light and heavy are valid
		return new int[] { 1, 2 };
	}

	// Can we start the next attack of requested type?
	private bool CanStartNextAttack(int attackType)
	{
		// If not in a combo or combo expired, any attack is valid
		if (currentComboCount == 0 || Time.time - lastAttackTime > comboWindow)
		{
			return true;
		}
    
		// Otherwise, check combo sequence validity
		return IsValidComboSequence(attackType);
	}

	// Get current attack name for debugging
	private string GetAttackName()
	{
		if (lastAttackTypePerformed == 1) // Light
		{
			return $"Light Attack {lastAttackIndexPerformed + 1}";
		}
		else if (lastAttackTypePerformed == 2) // Heavy
		{
			return $"Heavy Attack {lastAttackIndexPerformed + 1}";
		}
		return "No Attack";
	}
    
    
	public void PerformNextLightAttack()
	{
		if (!_animator || isAttacking) return;

		// Only proceed if this is a fresh attack or a queued Light attack
		if (_nextAttackQueued && _nextAttackType != 1 && currentComboCount > 0)
		{
			Debug.Log("Light Attack skipped—queued type mismatch after Heavy! ⛔");
			return;
		}

		bool isSwordAndShield = _controller.currentWeaponType == 1;
		if (isSwordAndShield)
		{
			_animator.applyRootMotion = true;
			Debug.Log("Root Motion ON—slash incoming! ⚔️");
		}
		isAttacking = true;
		attackStartTime = Time.time;
		lastAttackTime = Time.time;
				_inputAcceptedThisAttack = false; // Reset for this new attack
		int attackIndex;
		if (currentComboCount == 0) // Starting a new combo
		{
			attackIndex = 0; // Start with Light1
			currentComboCount = 1;
		}
		else if (lastAttackTypePerformed == 1) // Coming from light attack
		{
			if (lastAttackIndexPerformed == 0) // From Light1
				attackIndex = 1; // Go to Light2
			else if (lastAttackIndexPerformed == 1) // From Light2
				attackIndex = 2; // Go to Light3
			else
				return; // Light3 has no light attack follow-up
			currentComboCount++;
		}
		else if (lastAttackTypePerformed == 2) // Coming from heavy attack
		{
			if (lastAttackIndexPerformed == 0) // From Heavy1
				attackIndex = 1; // Go to Light2
			else
				return; // Heavy2 has no light attack follow-up
			currentComboCount++;
		}
		else
		{
			return; // Invalid state
		}

		// Update tracking variables
		lastAttackTypePerformed = 1; // Light attack
		lastAttackIndexPerformed = attackIndex;
		currentAttackType = attackIndex; // For animation index

		// Set the flag if this is the last combo count
		if (currentComboCount >= maxComboCount)
		{
			_isLastComboCountReached = true;
			Debug.Log("Last combo count reached—blocking further attack inputs! ⛔");
		}

		// Toggle root motion ON for Weapon 1
		if (isSwordAndShield)
		{
			_animator.applyRootMotion = true;
			Debug.Log("Sword & Shield attack—Root Motion ON, slashing with precision! ⚔️");
		}

		_animator.SetBool("IsAttacking", true);
		_animator.SetInteger("AttackType", attackIndex);
		_animator.SetInteger("ComboCount", currentComboCount);
		_animator.SetTrigger("Attack");

		Debug.Log($"Performing Light Attack {attackIndex + 1}, Combo: {currentComboCount}");
		FaceAttackDirection();
	}

	public void PerformNextHeavyAttack()
	{
		if (!_animator || isAttacking) return;
		
		bool isSwordAndShield = _controller.currentWeaponType == 1;
		if (isSwordAndShield)
		{
			_animator.applyRootMotion = true; // Set BEFORE attack starts
			Debug.Log("Root Motion ON—slash incoming faster than a Chocobo sprint! ⚔️");
		}
		isAttacking = true;
		attackStartTime = Time.time;
		lastAttackTime = Time.time;
				_inputAcceptedThisAttack = false; // Reset for this new attack
		
		int attackIndex;
		if (currentComboCount == 0) // Starting a new combo
		{
			attackIndex = 0; // Start with Heavy1
			currentComboCount = 1;
		}
		else if (lastAttackTypePerformed == 1) // Coming from light attack
		{
			if (lastAttackIndexPerformed == 0) // From Light1
				attackIndex = 0; // Go to Heavy1
			else if (lastAttackIndexPerformed == 1) // From Light2
				attackIndex = 1; // Go to Heavy2
			else
				return; // Light3 has no heavy follow-up
			currentComboCount++;
		}
		else if (lastAttackTypePerformed == 2) // Coming from heavy attack
		{
			if (lastAttackIndexPerformed == 0) // From Heavy1
				attackIndex = 1; // Go to Heavy2
			else
				return; // Heavy2 has no heavy follow-up
			currentComboCount++;
		}
		else
		{
			Debug.Log("Invalid heavy attack transition—combo aborted!");
			return;
		}

		lastAttackTypePerformed = 2; // Heavy attack
		lastAttackIndexPerformed = attackIndex;
		currentAttackType = attackIndex + 3; // Heavy1 = 3, Heavy2 = 4

		// Debug to confirm the transition
		Debug.Log($"Heavy Attack Transition: Index={attackIndex}, Type={currentAttackType}, ComboCount={currentComboCount}");

			// Set the flag if this is the last combo count
		if (currentComboCount >= maxComboCount)
		{
			_isLastComboCountReached = true;
			Debug.Log("Last combo count reached—blocking further attack inputs! ⛔");
		}

		// Toggle root motion ON for Weapon 1
		if (isSwordAndShield)
		{
			_animator.applyRootMotion = true;
			Debug.Log($"Heavy Attack {attackIndex + 1} START—Root Motion ON, Weapon 1 ACTIVE! ⚔️ | Time: {Time.time}");
		}

		_animator.SetBool("IsAttacking", true);
		_animator.SetInteger("AttackType", currentAttackType); // Use currentAttackType directly
		_animator.SetInteger("ComboCount", currentComboCount);
		_animator.SetTrigger("Attack");

		Debug.Log($"Performing Heavy Attack {attackIndex + 1}, Combo: {currentComboCount}, Type: {currentAttackType}");
		FaceAttackDirection();
	}

    
	
	private void OnEnable()
	{
		SingleTriggerAttackHandler.OnAttackTriggered += HandleTriggeredAttack;
	}

	private void OnDisable()
	{
		SingleTriggerAttackHandler.OnAttackTriggered -= HandleTriggeredAttack;
	}

	private void HandleTriggeredAttack(int attackType)
	{
		if (attackType == 1) PerformNextLightAttack();
		else PerformNextHeavyAttack();
	}
    
	public void CheckAndResetOnExit()
	{
		bool isQueued = _nextAttackQueued && currentComboCount < maxComboCount;
		Debug.Log($"Exit Queue Check - Queued: {isQueued}, Combo: {currentComboCount}, Type: {currentAttackType}, Time: {Time.time}");

		if (OnQueueCheckedBeforeExit != null)
		{
			OnQueueCheckedBeforeExit(isQueued);
		}

		if (!isQueued && _animator)
		{
			// Hard reset
			currentComboCount = 0;
			currentAttackType = -1;
			lastAttackTypePerformed = 0;
			lastAttackIndexPerformed = -1;
			_nextAttackQueued = false;
			_nextAttackType = 0;
			_inputAcceptedThisAttack = false;
			lastLightAttackInputTime = -1f;
			lastHeavyAttackInputTime = -1f;
			_animator.SetBool("IsAttacking", false);
			_animator.SetInteger("ComboCount", 0);
			_animator.SetInteger("AttackType", -1);
			_animator.ResetTrigger("Attack");
			Debug.Log("No queue—HARD RESET before exit like a *Metroid* save point! 🚪");
		}
	}
    
    
	public void ComboWindowOpen()
	{
		inComboThreshold = true;
		if (OnComboThresholdEntered != null)
			OnComboThresholdEntered(currentAttackType);
	}

	// Called by animation event
	public void ComboWindowClose()
	{
		inComboThreshold = false;
		if (OnComboThresholdExited != null)
			OnComboThresholdExited();
	}

    
    
	public void AttackEnded()
	{
		EndAttack();
	}
    
    
    
	public void EndAttack()
	{
		if (!isAttacking) return;

		isAttacking = false;
		inComboThreshold = false;
		SetCanInterruptWithRoll(true); // Reset when attack ends—like a *Sonic* speed boost!
		
		if (_animator)
		{
			AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
			Debug.Log($"EndAttack Called - Type: {currentAttackType}, State: {currentState.fullPathHash}, Time: {Time.time}");

			if (_nextAttackQueued && currentComboCount < maxComboCount)
			{
				Debug.Log("Attack Ended—Queued Attack Processing like a *Street Fighter* combo!");
				StartCoroutine(ProcessQueuedAttack());
			}
			else
			{
				_animator.ResetTrigger("Attack");
				currentAttackType = -1;
				Debug.Log("No queue—attack ended, reset deferred to locomotion like a *Sonic* checkpoint! 🏃‍♂️");
			}
		}
	}
	
	
	 
	
    
	private IEnumerator ProcessQueuedAttack()
	{
		float adjustedDuration = GetCurrentAttackDuration();
		float timeSinceStart = Time.time - attackStartTime;
		float remainingTime = Mathf.Max(0, adjustedDuration - timeSinceStart + attackTransitionDelay);
		yield return new WaitForSeconds(remainingTime);

		if (isAttacking)
		{
			Debug.LogWarning("Still attacking after delay—waiting one more frame like a *Zelda* puzzle solve!");
			yield return null;
			if (isAttacking)
			{
				Debug.Log("Queued attack aborted—still swinging, no overlap allowed!");
				_nextAttackQueued = false;
				yield break;
			}
		}

		lastSuccessfulAttackTime = Time.time;

		if (_nextAttackType == 1) // Light
		{
			PerformNextLightAttack();
		}
		else // Heavy
		{
			PerformNextHeavyAttack();
		}

		_nextAttackQueued = false;
		Debug.Log($"Queued attack executed: {(_nextAttackType == 1 ? "Light" : "Heavy")}, Time: {Time.time}");
	}
    
	public void ResetCombo()
	{
		if (currentComboCount == 0) return;

		currentComboCount = 0;
		lastAttackTypePerformed = 0;
		lastAttackIndexPerformed = -1;
		_nextAttackQueued = false;
		comboThresholdCloseTime = -1f; // Reset grace period—like reloading a *Metroid* charge shot!

		if (_animator)
		{
			_animator.SetInteger("ComboCount", 0);
			_animator.SetInteger("AttackType", -1);
			_animator.SetBool("IsAttacking", false); // Double down on cleanup
		}

		Debug.Log("Combo RESET: Count zeroed, grace period cleared—fresh start, baby! 🏁");
	}
    
    
    
    
	private void SetupComboTransitions()
	{
		// From Light Attack 1 (index 0)
		lightAttack1NextValid = new int[] { 1, 2 };  // Can go to Light2 or Heavy1
    
		// From Light Attack 2 (index 1)
		lightAttack2NextValid = new int[] { 1, 2 };  // Can go to Light3 or Heavy2
    
		// From Light Attack 3 (index 2)
		lightAttack3NextValid = new int[] { };  // End of combo
    
		// From Heavy Attack 1 (index 0)
		heavyAttack1NextValid = new int[] { 1, 2 };  // Can go to Light2 or Heavy2
    
		// From Heavy Attack 2 (index 1)
		heavyAttack2NextValid = new int[] { };  // End of combo
	}
    
    
    
    
	private void FaceAttackDirection()
	{
		Vector2 input = _input.move.normalized;
		if (input.magnitude > 0.1f)
		{
			GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			if (mainCamera)
			{
				Vector3 worldDirection = mainCamera.transform.TransformDirection(new Vector3(input.x, 0, input.y));
				worldDirection.y = 0;
				worldDirection.Normalize();
				transform.rotation = Quaternion.LookRotation(worldDirection);
			}
		}
	}
    
	// Called from animation events to apply damage
	public void OnDamageFrame(int attackType)
	{
		float damage = 0;
        
		switch (attackType)
		{
		case 0: damage = lightAttack1Damage; break;
		case 1: damage = lightAttack2Damage; break;
		case 2: damage = lightAttack3Damage; break;
		case 3: damage = heavyAttack1Damage; break;
		case 4: damage = heavyAttack2Damage; break;
		}
        
		// Apply damage to targets in range
		ApplyDamage(damage);
	}
    
	// This can be called from animation events to force the next attack in a combo
	public void OnAttackEnded()
	{
		Debug.Log("Animation event: Attack ENDED—triggering cleanup!");
		EndAttack();
	}
	public void OnPlayerDeath()
	{
		CombatSystem combat = FindObjectOfType<CombatSystem>();
		combat.ResetCombo();
		// Teleport to level one logic...
	}
	
	private void ApplyDamage(float damage)
	{
		// Set up attack range based on attack type (heavy attacks have longer range)
		float attackRange = (currentAttackType >= 3) ? 2.0f : 1.5f;
		Vector3 attackOrigin = transform.position + transform.forward * 0.5f;
        
		// Find all colliders in the attack range
		Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, attackRange);
        
		foreach (var hitCollider in hitColliders)
		{
			if (hitCollider.CompareTag("Enemy"))
			{
				// You would implement your enemy damage handling here
				// For example:
				// var enemyHealth = hitCollider.GetComponent<EnemyHealth>();
				// if (enemyHealth) { enemyHealth.TakeDamage(damage); }
                
				Debug.Log($"Hit enemy for {damage} damage");
			}
		}
	}

}
