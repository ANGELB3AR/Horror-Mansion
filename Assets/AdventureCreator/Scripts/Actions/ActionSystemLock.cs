/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionSystemLock.cs"
 * 
 *	This action handles the enabling / disabling
 *	of individual AC systems, allowing for
 *	minigames or other non-adventure elements
 *	to be run.
 * 
 */

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionSystemLock : Action
	{

		public bool changeMovementMethod = false;
		public MovementMethod newMovementMethod;

		public LockType cursorLock = LockType.NoChange;
		public LockType inputLock = LockType.NoChange;
		public LockType interactionLock = LockType.NoChange;
		public LockType draggableLock = LockType.NoChange;
		public LockType menuLock = LockType.NoChange;
		public LockType movementLock = LockType.NoChange;
		public LockType cameraLock = LockType.NoChange;
		public LockType triggerLock = LockType.NoChange;
		public LockType playerLock = LockType.NoChange;
		public LockType saveLock = LockType.NoChange;
		public LockType keyboardGameplayMenusLock = LockType.NoChange;


		public override ActionCategory Category { get { return ActionCategory.Engine; }}
		public override string Title { get { return "Manage systems"; }}
		public override string Description { get { return "Enables and disables individual systems within Adventure Creator, such as Interactions. Can also be used to change the 'Movement method', as set in the Settings Manager, but note that this change will not be recorded in save games."; }}
		
		
		public override float Run ()
		{
			if (changeMovementMethod)
			{
				KickStarter.playerInput.InitialiseCursorLock (newMovementMethod);
				KickStarter.settingsManager.movementMethod = newMovementMethod;
			}

			switch (cursorLock)
			{
				case LockType.Enabled:
					KickStarter.stateHandler.SetCursorSystem (true);
					break;

				case LockType.Disabled:
					KickStarter.stateHandler.SetCursorSystem (false);
					break;

				case LockType.Toggle:
					KickStarter.stateHandler.SetCursorSystem (!KickStarter.stateHandler.CursorSystemIsEnabled);
					break;

				default:
					break;
			}

			switch (inputLock)
			{
				case LockType.Enabled:
					KickStarter.stateHandler.SetInputSystem (true);
					break;

				case LockType.Disabled:
					KickStarter.stateHandler.SetInputSystem (false);
					break;

				case LockType.Toggle:
					KickStarter.stateHandler.SetInputSystem (!KickStarter.stateHandler.InputSystemIsEnabled);
					break;

				default:
					break;
			}

			switch (interactionLock)
			{
				case LockType.Enabled:
					KickStarter.stateHandler.SetInteractionSystem (true);
					break;
				
				case LockType.Disabled:
					KickStarter.stateHandler.SetInteractionSystem (false);
					break;

				case LockType.Toggle:
					KickStarter.stateHandler.SetInteractionSystem (!KickStarter.stateHandler.InteractionSystemIsEnabled);
					break;
				
				default:
					break;
			}

			switch (draggableLock)
			{
				case LockType.Enabled:
					KickStarter.stateHandler.SetDraggableSystem (true);
					break;

				case LockType.Disabled:
					KickStarter.stateHandler.SetDraggableSystem (false);
					break;

				case LockType.Toggle:
					KickStarter.stateHandler.SetDraggableSystem (!KickStarter.stateHandler.DraggableSystemIsEnabled);
					break;

				default:
					break;
			}

			switch (menuLock)
			{
				case LockType.Enabled:
					KickStarter.stateHandler.SetMenuSystem (true);
					break;

				case LockType.Disabled:
					KickStarter.stateHandler.SetMenuSystem (false);
					break;

				case LockType.Toggle:
					KickStarter.stateHandler.SetMenuSystem (!KickStarter.stateHandler.MenuSystemIsEnabled);
					break;

				default:
					break;
			}

			switch (movementLock)
			{
				case LockType.Enabled:
					KickStarter.stateHandler.SetMovementSystem (true);
					break;

				case LockType.Disabled:
					KickStarter.stateHandler.SetMovementSystem (false);
					break;

				case LockType.Toggle:
					KickStarter.stateHandler.SetMovementSystem (!KickStarter.stateHandler.MovementSystemIsEnabled);
					break;

				default:
					break;
			}

			switch (cameraLock)
			{
				case LockType.Enabled:
					KickStarter.stateHandler.SetCameraSystem (true);
					break;

				case LockType.Disabled:
					KickStarter.stateHandler.SetCameraSystem (false);
					break;

				case LockType.Toggle:
					KickStarter.stateHandler.SetCameraSystem (!KickStarter.stateHandler.CameraSystemIsEnabled);
					break;

				default:
					break;
			}

			switch (triggerLock)
			{
				case LockType.Enabled:
					KickStarter.stateHandler.SetTriggerSystem (true);
					break;

				case LockType.Disabled:
					KickStarter.stateHandler.SetTriggerSystem (false);
					break;

				case LockType.Toggle:
					KickStarter.stateHandler.SetTriggerSystem (!KickStarter.stateHandler.TriggerSystemIsEnabled);
					break;

				default:
					break;
			}

			switch (playerLock)
			{
				case LockType.Enabled:
					KickStarter.stateHandler.SetPlayerSystem (true);
					break;

				case LockType.Disabled:
					KickStarter.stateHandler.SetPlayerSystem (false);
					break;

				case LockType.Toggle:
					KickStarter.stateHandler.SetPlayerSystem (KickStarter.stateHandler.PlayerSystemIsEnabled);
					break;

				default:
					break;
			}

			switch (saveLock)
			{
				case LockType.Enabled:
					KickStarter.playerMenus.PreventSaving = false;
					break;

				case LockType.Disabled:
					KickStarter.playerMenus.PreventSaving = true;
					break;

				case LockType.Toggle:
					KickStarter.playerMenus.PreventSaving = !KickStarter.playerMenus.PreventSaving;
					break;

				default:
					break;
			}

			if (KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen)
			{
				switch (keyboardGameplayMenusLock)
				{
					case LockType.Enabled:
						KickStarter.playerInput.CanKeyboardControlMenusDuringGameplay = true;
						break;

					case LockType.Disabled:
						KickStarter.playerInput.CanKeyboardControlMenusDuringGameplay = false;
						break;

					case LockType.Toggle:
						KickStarter.playerInput.CanKeyboardControlMenusDuringGameplay = !KickStarter.playerInput.CanKeyboardControlMenusDuringGameplay;
						break;

					default:
						break;
				}
			}

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			changeMovementMethod = EditorGUILayout.ToggleLeft ("Change movement method?", changeMovementMethod);
			if (changeMovementMethod)
			{
				newMovementMethod = (MovementMethod) EditorGUILayout.EnumPopup ("Movement method:", newMovementMethod);
			}

			EditorGUILayout.Space ();

			cursorLock = (LockType) EditorGUILayout.EnumPopup ("Cursor:", cursorLock);
			inputLock = (LockType) EditorGUILayout.EnumPopup ("Input:", inputLock);
			interactionLock = (LockType) EditorGUILayout.EnumPopup ("Interactions:", interactionLock);
			draggableLock = (LockType) EditorGUILayout.EnumPopup ("Draggables:", draggableLock);
			menuLock = (LockType) EditorGUILayout.EnumPopup ("Menus:", menuLock);
			movementLock = (LockType) EditorGUILayout.EnumPopup ("Movement:", movementLock);
			cameraLock = (LockType) EditorGUILayout.EnumPopup ("Camera:", cameraLock);
			triggerLock = (LockType) EditorGUILayout.EnumPopup ("Triggers:", triggerLock);
			playerLock = (LockType) EditorGUILayout.EnumPopup ("Player:", playerLock);
			saveLock = (LockType) EditorGUILayout.EnumPopup ("Saving:", saveLock);

			if (KickStarter.settingsManager && KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen)
			{
				keyboardGameplayMenusLock = (LockType) EditorGUILayout.EnumPopup ("Direct-nav in-game Menus:", keyboardGameplayMenusLock);
			}
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Engine: Manage systems' Action</summary>
		 * <param name = "cursorLock">Whether or not to disable the cursor system</param>
		 * <param name = "inputLock">Whether or not to disable the input system</param>
		 * <param name = "interactionLock">Whether or not to disable the interaction system</param>
		 * <param name = "menuLock">Whether or not to disable the menu system</param>
		 * <param name = "movementLock">Whether or not to disable the movement system</param>
		 * <param name = "cameraLock">Whether or not to disable the camera system</param>
		 * <param name = "triggerLock">Whether or not to disable the trigger system</param>
		 * <param name = "playerLock">Whether or not to disable the player system</param>
		 * <param name = "saveLock">Whether or not to disable the save system</param>
		 * <param name = "directControlInGameMenusLock">Whether or not to allow direct-navigation of in-game menus</param>
		 * * <param name = "draggableLock">Whether or not to disable the draggable system</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionSystemLock CreateNew (LockType cursorLock = LockType.NoChange, LockType inputLock = LockType.NoChange, LockType interactionLock = LockType.NoChange, LockType menuLock = LockType.NoChange, LockType movementLock = LockType.NoChange, LockType cameraLock = LockType.NoChange, LockType triggerLock = LockType.NoChange, LockType playerLock = LockType.NoChange, LockType saveLock = LockType.NoChange, LockType directControlInGameMenusLock = LockType.NoChange, LockType draggableLock = LockType.NoChange)
		{
			ActionSystemLock newAction = CreateNew<ActionSystemLock> ();
			newAction.changeMovementMethod = false;
			newAction.cursorLock = cursorLock;
			newAction.inputLock = inputLock;
			newAction.interactionLock = interactionLock;
			newAction.draggableLock = draggableLock;
			newAction.menuLock = menuLock;
			newAction.movementLock = movementLock;
			newAction.cameraLock = cameraLock;
			newAction.triggerLock = triggerLock;
			newAction.playerLock = playerLock;
			newAction.saveLock = saveLock;
			newAction.keyboardGameplayMenusLock = directControlInGameMenusLock;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Engine: Manage systems' Action</summary>
		 * <param name = "newMovementMethod">The game's new movement method</param>
		 * <param name = "cursorLock">Whether or not to disable the cursor system</param>
		 * <param name = "inputLock">Whether or not to disable the input system</param>
		 * <param name = "interactionLock">Whether or not to disable the interaction system</param>
		 * <param name = "menuLock">Whether or not to disable the menu system</param>
		 * <param name = "movementLock">Whether or not to disable the movement system</param>
		 * <param name = "cameraLock">Whether or not to disable the camera system</param>
		 * <param name = "triggerLock">Whether or not to disable the trigger system</param>
		 * <param name = "playerLock">Whether or not to disable the player system</param>
		 * <param name = "saveLock">Whether or not to disable the save system</param>
		 * <param name = "directControlInGameMenusLock">Whether or not to allow direct-navigation of in-game menus</param>
		 * <param name = "draggableLock">Whether or not to disable the draggable system</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionSystemLock CreateNew (MovementMethod newMovementMethod, LockType cursorLock = LockType.NoChange, LockType inputLock = LockType.NoChange, LockType interactionLock = LockType.NoChange, LockType menuLock = LockType.NoChange, LockType movementLock = LockType.NoChange, LockType cameraLock = LockType.NoChange, LockType triggerLock = LockType.NoChange, LockType playerLock = LockType.NoChange, LockType saveLock = LockType.NoChange, LockType directControlInGameMenusLock = LockType.NoChange, LockType draggableLock = LockType.NoChange)
		{
			ActionSystemLock newAction = CreateNew<ActionSystemLock> ();
			newAction.changeMovementMethod = true;
			newAction.newMovementMethod = newMovementMethod;
			newAction.cursorLock = cursorLock;
			newAction.inputLock = inputLock;
			newAction.interactionLock = interactionLock;
			newAction.draggableLock = draggableLock;
			newAction.menuLock = menuLock;
			newAction.movementLock = movementLock;
			newAction.cameraLock = cameraLock;
			newAction.triggerLock = triggerLock;
			newAction.playerLock = playerLock;
			newAction.saveLock = saveLock;
			newAction.keyboardGameplayMenusLock = directControlInGameMenusLock;
			return newAction;
		}

	}

}