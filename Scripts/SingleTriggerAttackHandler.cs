using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using System;
public class SingleTriggerAttackHandler : StateMachineBehaviour
{
	public delegate void AttackTriggeredHandler(int attackType);
	public static event AttackTriggeredHandler OnAttackTriggered;

	[SerializeField, Tooltip("Can this attack be interrupted by a roll? Set in Animator per state.")]
	public bool canRollInterrupt = true;

	[SerializeField, Tooltip("Matches CombatSystem’s attack type (0-4)")]
	public int attackType = 0;

	private bool hasTriggered = false;
	private float lastInputTime = -1f;
	private const float INPUT_COOLDOWN = 0.1f;
	private const float ROLL_INTERRUPT_THRESHOLD = 0.20f; // 35% cutoff

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		hasTriggered = false;
		CombatSystem.SetCanInterruptWithRoll(canRollInterrupt);
		Debug.Log($"Attack {attackType} START—Roll interruptible: {canRollInterrupt}, like *Link* swinging {GetAttackName(attackType)}!");

		var (combatSystem, inputs) = GetComponents(animator);
		if (combatSystem == null || inputs == null) return;

		if (combatSystem._isLastComboCountReached) return;

		TryTriggerEarlyAttack(animator, combatSystem, inputs);
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		float normalizedTime = stateInfo.normalizedTime % 1;
		CombatSystem.SetCanInterruptWithRoll(canRollInterrupt && normalizedTime < ROLL_INTERRUPT_THRESHOLD);

		if (normalizedTime >= ROLL_INTERRUPT_THRESHOLD && canRollInterrupt)
			Debug.Log($"Roll LOCKED at {normalizedTime:F2}—too late, like missing a *Zelda* parry!");

		if (normalizedTime > 0.75f) return; // Input window cutoff

		var (combatSystem, inputs) = GetComponents(animator);
		if (combatSystem == null || inputs == null || combatSystem._isLastComboCountReached) return;

		if (Time.time - lastInputTime < INPUT_COOLDOWN) return;
		if (inputs.lightAttack || inputs.heavyAttack)
		{
			lastInputTime = Time.time;
			TriggerAttack(animator, combatSystem, inputs.lightAttack ? 1 : 2);
			inputs.lightAttack = inputs.heavyAttack = false;
		}
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		var (combatSystem, inputs) = GetComponents(animator);
		if (combatSystem != null && !animator.GetNextAnimatorStateInfo(layerIndex).IsTag("Attack"))
		{
			combatSystem.EndAttack();
			CombatSystem.SetCanInterruptWithRoll(true);
			combatSystem._isLastComboCountReached = false;
		}
		if (inputs != null) inputs.roll = false;

		hasTriggered = false;
		Debug.Log("Attack EXIT—Reset like a *Sonic* speed pad!");
	}

	private void TryTriggerEarlyAttack(Animator animator, CombatSystem combatSystem, StarterAssetsInputs inputs)
	{
		if (hasTriggered || (!inputs.lightAttack && !inputs.heavyAttack)) return;

		int triggeredType = inputs.lightAttack ? 1 : 2;
		TriggerAttack(animator, combatSystem, triggeredType);
		hasTriggered = true;
		inputs.lightAttack = inputs.heavyAttack = false;
		Debug.Log($"Early {(triggeredType == 1 ? "Light" : "Heavy")}—Faster than Sonic on a chili dog!");
	}

	private void TriggerAttack(Animator animator, CombatSystem combatSystem, int attackType)
	{
		OnAttackTriggered?.Invoke(attackType);
		if (attackType == 1) combatSystem.PerformNextLightAttack();
		else combatSystem.PerformNextHeavyAttack();

		animator.SetBool("IsAttacking", true);
		animator.SetInteger("AttackType", attackType == 1 ? combatSystem.currentLightAttackIndex : combatSystem.currentHeavyAttackIndex + 3);
		animator.SetTrigger("Attack");

		Debug.Log($"Attack {attackType} FIRED—Synced like a *Street Fighter* combo!");
	}

	private (CombatSystem, StarterAssetsInputs) GetComponents(Animator animator)
	{
		var combatSystem = animator.GetComponent<CombatSystem>();
		var inputs = animator.GetComponent<StarterAssetsInputs>();
		if (combatSystem == null) Debug.LogError("CombatSystem missing—panic like a Cucco swarm!");
		if (inputs == null) Debug.LogError("No inputs? Stuck in a cutscene!");
		return (combatSystem, inputs);
	}

	private string GetAttackName(int type)
	{
		return type switch
		{
			0 => "LightAttack1",
            1 => "LightAttack2",
            2 => "LightAttack3",
            3 => "HeavyAttack1",
            4 => "HeavyAttack2",
			_ => "Unknown Attack"
		};
	}
	
	}