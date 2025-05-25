using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StarterAssets;
public class ComboStateManager : StateMachineBehaviour
{
	// Define the event delegate
	public delegate void IdleTransitionHandler();
	public static event IdleTransitionHandler OnReturnToIdle;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		CombatSystem combatSystem = animator.GetComponent<CombatSystem>();
		ThirdPersonController controller = animator.GetComponent<ThirdPersonController>();
		if (combatSystem == null || controller == null) return;

		// Enable root motion for sword attacks
		if (controller.currentWeaponType == 1 && combatSystem.isAttacking)
		{
			animator.applyRootMotion = true;
			Debug.Log("Combo State Enter—Root Motion ON like a *God of War* axe slam! ⚔️");
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		CombatSystem combatSystem = animator.GetComponent<CombatSystem>();
		ThirdPersonController controller = animator.GetComponent<ThirdPersonController>();
		if (combatSystem == null || controller == null) return;

		// Check the next state
		AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(layerIndex);
		bool goingToAttackState = nextState.IsTag("LightAttack") || nextState.IsTag("HeavyAttack");

		if (!goingToAttackState)
		{
			// Only reset if no attack is queued and not mid-attack
			bool isStillAttacking = combatSystem.isAttacking || combatSystem._nextAttackQueued;

			if (!isStillAttacking)
			{
				combatSystem.ResetCombo();
				if (OnReturnToIdle != null)
					OnReturnToIdle.Invoke(); // Fire the event like a *Final Fantasy* victory fanfare!

				if (!controller.isRolling && controller.currentWeaponType != 1)
				{
					controller.DisableRootMotionSmoothly(); // Smoothly disable root motion
					Debug.Log("Root Motion OFF—back to free movement like a *Super Mario* run!");
				}
				else
				{
					Debug.Log("Keeping Root Motion ON—roll or Weapon 1 still rocking! ⚔️🌀");
				}
			}
			else if (combatSystem._nextAttackQueued)
			{
				Debug.Log("Queue detected—combo keeps rolling like a *Devil May Cry* style chain!");
			}
		}
		else
		{
			Debug.Log("Next state is an attack—combo’s alive like a *Street Fighter* ultra!");
		}
	}
}