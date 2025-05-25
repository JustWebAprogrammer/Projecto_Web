using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		
		[Header ("Weapon Switching")]
		public bool weaponSwitch1;
		public bool weaponSwitch2;
		public bool weaponSwitch3;
		
		[Header ("Combat")]
		public bool block;
		public bool lightAttack;
		public bool heavyAttack;
		public bool isAttack;
		public bool roll; // NEW HOTNESS: The roll button joins the party! 🌀
		public Vector2 attackDirection;	
	
		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
		
		[Header("Bow Inputs")]
		public bool aim; // New aim input
		public bool fire; // New fire input
		public float mouseX; // For raw mouse X delta
		public ThirdPersonController Controll;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}
		
		public void OnMouseX(InputValue value)
		{
			mouseX = value.Get<float>();
			// Optional: Debug.Log($"Mouse X: {mouseX}");
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
		
		
		public void OnBlock(InputValue value)
		{
			BlockInput(value.isPressed);
		}
		
		
		public void OnRoll(InputValue value)
		{
			RollInput(value.isPressed);
		}
		
		
		public void OnWeaponSwitch1(InputValue value)
		{
			WeaponSwitch1Input(value.isPressed);
		}
		
		public void OnWeaponSwitch2(InputValue value)
		{
			WeaponSwitch2Input(value.isPressed);
		}
		
		public void OnWeaponSwitch3(InputValue value)
		{
			WeaponSwitch3Input(value.isPressed);
		}
		
		
		public void OnLightAttack(InputValue value)
		{
			LightAttackInput(value.isPressed);
		}
		
		
		public void OnHeavyAttack(InputValue value)
		{
			HeavyAttackInput(value.isPressed);
		}
		
		
		public void OnAim(InputValue value)
		{
			bool newAimState = value.isPressed;
			aim = newAimState; // Always update aim to match input

			// Safety check: disable aiming if crossbow isn't equipped
			if (Controll.currentWeaponType != 2 && aim)
			{
				aim = false;
				Debug.Log($"Aim Reset: Crossbow not equipped (WeaponType: {Controll.currentWeaponType})");
			}

			// Debug log to keep an eye on things (remove later if you’re feeling less lazy!)
			Debug.Log($"Aim State: {aim}, WeaponType: {Controll.currentWeaponType}, Time: {Time.time}");
		}

		public void OnFire(InputValue value)
		{
			FireInput(value.isPressed);
		}
		
		
		
#endif




		

		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
		
		public void BlockInput(bool newBlockState)
		{
			block = newBlockState;
		}
		
		
		public void RollInput(bool newRollState)
		{
			if (newRollState && !roll && CombatSystem.CanInterruptWithRoll && Controll.currentWeaponType != 2) // Add weapon check
			{
				roll = true;
				Debug.Log("Roll Input: ON—Dodge like *Bayonetta* in style!");
			}
			else if (newRollState && !roll && !CombatSystem.CanInterruptWithRoll)
			{
				roll = false;
				Debug.Log("Roll Input: BLOCKED—Locked tighter than a *Zelda* dungeon door!");
			}
			else if (!newRollState)
			{
				roll = false;
				Debug.Log("Roll Input: OFF—Back to the fight!");
			}
		}
		
		public void LightAttackInput(bool newLightAttackState)
		{
			if (newLightAttackState && !lightAttack && Controll.currentWeaponType == 1)
			{
				lightAttack = true;
				Debug.Log("Light Attack Input: ON");
			}
			else if (!newLightAttackState && lightAttack && Controll.currentWeaponType == 1)
			{
				lightAttack = false;
				Debug.Log("Light Attack Input: OFF");
			}
		}

		public void HeavyAttackInput(bool newHeavyAttackState)
		{
			if (newHeavyAttackState && !heavyAttack && Controll.currentWeaponType == 1)
			{
				heavyAttack = true;
				Debug.Log("Heavy Attack Input: ON");
			}
			else if (!newHeavyAttackState && heavyAttack && Controll.currentWeaponType == 1)
			{
				heavyAttack = false;
				Debug.Log("Heavy Attack Input: OFF");
			}
		}
		 
		 
		public void WeaponSwitch1Input(bool newWeaponSwitchState)
		{
			if (newWeaponSwitchState && !weaponSwitch1)
			{
				weaponSwitch1 = true;
				Debug.Log("Weapon Switch 1 Input: ON");
			}
			else if (!newWeaponSwitchState && weaponSwitch1)
			{
				weaponSwitch1 = false;
				Debug.Log("Weapon Switch 1 Input: OFF");
			}
		}

		public void WeaponSwitch2Input(bool newWeaponSwitchState)
		{
			if (newWeaponSwitchState && !weaponSwitch2)
			{
				weaponSwitch2 = true;
				Debug.Log("Weapon Switch 2 Input: ON");
			}
			else if (!newWeaponSwitchState && weaponSwitch2)
			{
				weaponSwitch2 = false;
				Debug.Log("Weapon Switch 2 Input: OFF");
			}
		}

		public void WeaponSwitch3Input(bool newWeaponSwitchState)
		{
			if (newWeaponSwitchState && !weaponSwitch3)
			{
				weaponSwitch3 = true;
				Debug.Log("Weapon Switch 3 Input: ON");
			}
			else if (!newWeaponSwitchState && weaponSwitch3)
			{
				weaponSwitch3 = false;
				Debug.Log("Weapon Switch 3 Input: OFF");
			}
		} 

		public void AimInput(bool newAimState)
		{
			if (Controll.currentWeaponType == 2){
			aim = newAimState;
			if (aim) Debug.Log("Aim Input: ON!");
			else Debug.Log("Aim Input: OFF!");
			}
		}

		public void FireInput(bool newFireState)
		{
			if (newFireState && !fire&& Controll.currentWeaponType == 2)
			{
				fire = true;
				Debug.Log("Fire Input: ON");
			}
			else if (!newFireState && fire && Controll.currentWeaponType == 2)
			{
				fire = false;
				Debug.Log("Fire Input: OFF");
			}
		}


		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}