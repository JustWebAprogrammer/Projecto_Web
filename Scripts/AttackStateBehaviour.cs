using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
public class AttackStateBehaviour : StateMachineBehaviour
{
    
	[SerializeField] private int attackType; // 0-2 = Light 1-3, 3-4 = Heavy 1-2
	[SerializeField] private int[] validNextAttacks; // e.g., {1, 2} for Light/Heavy followup
	private bool inputRegistered = false;
	private CombatSystem combatSystem;
	private StarterAssetsInputs inputs;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		combatSystem = animator.GetComponent<CombatSystem>();
		inputs = animator.GetComponent<StarterAssetsInputs>();
		inputRegistered = false; // Reset for this state
		combatSystem.isAttacking = true;
		combatSystem.currentAttackType = attackType;
		combatSystem.currentComboCount++;
		animator.SetBool("IsAttacking", true);
		animator.SetInteger("AttackType", attackType);
		animator.SetInteger("ComboCount", combatSystem.currentComboCount);
		Debug.Log($"Entered Attack State {attackType}—Combo: {combatSystem.currentComboCount}, ready like a *God of War* swing!");
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (inputRegistered || combatSystem.currentComboCount >= combatSystem.maxComboCount) return;

		bool lightRequested = inputs.lightAttack;
		bool heavyRequested = inputs.heavyAttack;

		if (lightRequested && System.Array.IndexOf(validNextAttacks, 1) != -1)
		{
			combatSystem._nextAttackQueued = true;
			combatSystem._nextAttackType = 1;
			inputRegistered = true;
			inputs.lightAttack = false; // Clear input
			Debug.Log($"Queued Light Attack from {attackType}—one input only, like a *Sekiro* parry!");
		}
		else if (heavyRequested && System.Array.IndexOf(validNextAttacks, 2) != -1)
		{
			combatSystem._nextAttackQueued = true;
			combatSystem._nextAttackType = 2;
			inputRegistered = true;
			inputs.heavyAttack = false; // Clear input
			Debug.Log($"Queued Heavy Attack from {attackType}—locked in like a *Monster Hunter* charge!");
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (combatSystem._nextAttackQueued)
		{
			if (combatSystem._nextAttackType == 1)
				combatSystem.PerformNextLightAttack();
			else
				combatSystem.PerformNextHeavyAttack();
			combatSystem._nextAttackQueued = false;
		}
		else
		{
			combatSystem.ResetCombo(); // No queue, reset
			animator.SetBool("IsAttacking", false);
			animator.SetInteger("AttackType", -1);
			Debug.Log($"Exiting {attackType}—No queue, reset like a *Hollow Knight* shade!");
		}
	}
    
}
