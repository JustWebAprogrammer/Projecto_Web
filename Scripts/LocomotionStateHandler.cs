using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
public class LocomotionStateHandler : StateMachineBehaviour
{
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!stateInfo.IsTag("Locomotion")) return;

		Debug.Log("Entered Locomotion State: Time to clean house like a *Final Fantasy* inn stay! 🌟");

		CombatSystem combatSystem = animator.GetComponent<CombatSystem>();
		ThirdPersonController controller = animator.GetComponent<ThirdPersonController>();
		if (combatSystem == null || controller == null) return;

		// Forcefully reset combat state—NO EXCEPTIONS!
		combatSystem.isAttacking = false; // Slam that flag down like a Goron punch!
		combatSystem._nextAttackQueued = false;
		combatSystem._nextAttackType = 0;
		combatSystem._inputAcceptedThisAttack = false;
		combatSystem._isLastComboCountReached = false;
		combatSystem.currentAttackType = -1;
		combatSystem.currentComboCount = 0;
		combatSystem.lastAttackTypePerformed = 0;
		combatSystem.lastAttackIndexPerformed = -1;
		combatSystem.lastLightAttackInputTime = -1f;
		combatSystem.lastHeavyAttackInputTime = -1f;

		// Reset Animator parameters—clean slate, baby!
		animator.SetBool("IsAttacking", false);
		animator.SetInteger("ComboCount", 0);
		animator.SetInteger("AttackType", -1);
		animator.ResetTrigger("Attack");

		Debug.Log("Locomotion HARD RESET: Combat wiped clean like a *Zelda* fairy fountain revive! ✨");

		// Ensure root motion is off unless rolling or attacking with sword
		if (!controller.isRolling && controller.currentWeaponType != 1)
		{
			animator.applyRootMotion = false;
			Debug.Log("Root Motion OFF—moving free like a Chocobo on the plains! 🐤");
		}
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!stateInfo.IsTag("Locomotion")) return;

		Debug.Log("Exiting Locomotion State: Prepping for action like *Link* grabbing the Master Sword! ⚔️");

		CombatSystem combatSystem = animator.GetComponent<CombatSystem>();
		if (combatSystem != null)
		{
			combatSystem.comboWindow = 1.5f; // Restore default
			Debug.Log($"Combo window restored to {combatSystem.comboWindow}s—ready for a new combo spree like *Bayonetta* stepping into the fray! 💃");
		}
		else
		{
			Debug.LogWarning("No CombatSystem found on exit—did Mario lose his Fire Flower?");
		}
	}
}
