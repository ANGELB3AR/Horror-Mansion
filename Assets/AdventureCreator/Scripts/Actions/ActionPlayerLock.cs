/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionPlayerLock.cs"
 * 
 *	This action constrains the player in various ways (movement, saving etc)
 *	In Direct control mode, the player can be assigned a path,
 *	and will only be able to move along that path during gameplay.
 * 
 */

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionPlayerLock : Action
	{
		
		public LockType doUpLock = LockType.NoChange;
		public LockType doDownLock = LockType.NoChange;
		public LockType doLeftLock = LockType.NoChange;
		public LockType doRightLock = LockType.NoChange;
		
		public PlayerMoveLock doRunLock = PlayerMoveLock.NoChange;
		public LockType doJumpLock = LockType.NoChange;
		public LockType freeAimLock = LockType.NoChange;
		public LockType cursorState = LockType.NoChange;
		public LockType doGravityLock = LockType.NoChange;
		public LockType doHotspotHeadTurnLock = LockType.NoChange;
		public Paths movePath;
		public bool lockedPathCanReverse;


		public override ActionCategory Category { get { return ActionCategory.Player; }}
		public override string Title { get { return "Constrain"; }}
		public override string Description { get { return "Locks and unlocks various aspects of Player control. When using Direct or First Person control, can also be used to specify a Path object to restrict movement to."; }}


		public override float Run ()
		{
			if (KickStarter.player == null)
			{
				LogWarning ("No Player found!");
				return 0f;
			}

			if (IsSingleLockMovement ())
			{
				doLeftLock = doUpLock;
				doRightLock = doUpLock;
				doDownLock = doUpLock;
			}

			switch (doUpLock)
			{ 
				case LockType.Disabled:
					KickStarter.player.upMovementLocked = true;
					break;

				case LockType.Enabled:
					KickStarter.player.upMovementLocked = false;
					break;

				case LockType.Toggle:
					KickStarter.player.upMovementLocked = !KickStarter.player.upMovementLocked;
					break;

				default:
					break;
			}

			switch (doDownLock)
			{ 
				case LockType.Disabled:
					KickStarter.player.downMovementLocked = true;
					break;

				case LockType.Enabled:
					KickStarter.player.downMovementLocked = false;
					break;

				case LockType.Toggle:
					KickStarter.player.downMovementLocked = !KickStarter.player.downMovementLocked;
					break;

				default:
					break;
			}

			switch (doLeftLock)
			{
				case LockType.Disabled:
					KickStarter.player.leftMovementLocked = true;
					break;

				case LockType.Enabled:
					KickStarter.player.leftMovementLocked = false;
					break;

				case LockType.Toggle:
					KickStarter.player.leftMovementLocked = !KickStarter.player.leftMovementLocked;
					break;

				default:
					break;
			}

			switch (doRightLock)
			{
				case LockType.Disabled:
					KickStarter.player.rightMovementLocked = true;
					break;

				case LockType.Enabled:
					KickStarter.player.rightMovementLocked = false;
					break;

				case LockType.Toggle:
					KickStarter.player.rightMovementLocked = !KickStarter.player.rightMovementLocked;
					break;

				default:
					break;
			}
		
			if (KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick)
			{
				switch (doJumpLock)
				{
					case LockType.Disabled:
						KickStarter.player.jumpingLocked = true;
						break;

					case LockType.Enabled:
						KickStarter.player.jumpingLocked = false;
						break;

					case LockType.Toggle:
						KickStarter.player.jumpingLocked = !KickStarter.player.jumpingLocked;
						break;

					default:
						break;
				}
			}

			if (IsInFirstPerson ())
			{
				switch (freeAimLock)
				{ 
					case LockType.Disabled:
						KickStarter.player.freeAimLocked = true;
						break;

					case LockType.Enabled:
						KickStarter.player.freeAimLocked = false;
						break;

					case LockType.Toggle:
						KickStarter.player.freeAimLocked = !KickStarter.player.freeAimLocked;
						break;

					default:
						break;
				}
			}

			switch (cursorState)
			{
				case LockType.Disabled:
					KickStarter.playerInput.SetInGameCursorState (false);
					break;

				case LockType.Enabled:
					KickStarter.playerInput.SetInGameCursorState (true);
					break;

				case LockType.Toggle:
					KickStarter.playerInput.ToggleCursor ();
					break;

				default:
					break;
			}

			switch (doRunLock)
			{
				case PlayerMoveLock.AlwaysRun:
				case PlayerMoveLock.AlwaysWalk:
				case PlayerMoveLock.Free:
					KickStarter.player.runningLocked = doRunLock;
					break;

				default:
					break;
			}
			
			if (movePath)
			{
				KickStarter.player.SetLockedPath (movePath, lockedPathCanReverse, PathSnapping.SnapToStart);
				KickStarter.player.SetMoveDirectionAsForward ();
			}
			else if (KickStarter.player.GetPath ())
			{
				if (KickStarter.player.IsPathfinding () && !ChangingMovementLock ())// && (doRunLock == PlayerMoveLock.AlwaysWalk || doRunLock == PlayerMoveLock.AlwaysRun))
				{
					switch (doRunLock)
					{
						case PlayerMoveLock.AlwaysRun:
							KickStarter.player.GetPath ().pathSpeed = PathSpeed.Run;
							KickStarter.player.isRunning = true;
							break;

						case PlayerMoveLock.AlwaysWalk:
							KickStarter.player.GetPath ().pathSpeed = PathSpeed.Walk;
							KickStarter.player.isRunning = false;
							break;

						default:
							break;
					}
				}
				else
				{
					KickStarter.player.EndPath ();
				}
			}

			switch (doGravityLock)
			{ 
				case LockType.Enabled:
					KickStarter.player.ignoreGravity = false;
					break;

				case LockType.Disabled:
					KickStarter.player.ignoreGravity = true;
					break;

				case LockType.Toggle:
					KickStarter.player.ignoreGravity = !KickStarter.player.ignoreGravity;
					break;

				default:
					break;
			}

			if (AllowHeadTurning ())
			{
				switch (doHotspotHeadTurnLock)
				{
					case LockType.Disabled:
						KickStarter.player.LockHotspotHeadTurning = true;
						break;

					case LockType.Enabled:
						KickStarter.player.LockHotspotHeadTurning = false;
						break;

					case LockType.Toggle:
						KickStarter.player.LockHotspotHeadTurning = !KickStarter.player.LockHotspotHeadTurning;
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
			if (IsSingleLockMovement ())
			{
				doUpLock = (LockType) EditorGUILayout.EnumPopup ("Movement:", doUpLock);
			}
			else
			{
				doUpLock = (LockType) EditorGUILayout.EnumPopup ("Up movement:", doUpLock);
				doDownLock = (LockType) EditorGUILayout.EnumPopup ("Down movement:", doDownLock);
				doLeftLock = (LockType) EditorGUILayout.EnumPopup ("Left movement:", doLeftLock);
				doRightLock = (LockType) EditorGUILayout.EnumPopup ("Right movement:", doRightLock);
			}

			if (KickStarter.settingsManager && KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick)
			{
				doJumpLock = (LockType) EditorGUILayout.EnumPopup ("Jumping:", doJumpLock);
			}

			if (IsInFirstPerson ())
			{
				freeAimLock = (LockType) EditorGUILayout.EnumPopup ("Free-aiming:", freeAimLock);
			}

			cursorState = (LockType) EditorGUILayout.EnumPopup ("Cursor lock:", cursorState);
			doRunLock = (PlayerMoveLock) EditorGUILayout.EnumPopup ("Walk / run:", doRunLock);
			doGravityLock = (LockType) EditorGUILayout.EnumPopup ("Affected by gravity?", doGravityLock);
			movePath = (Paths) EditorGUILayout.ObjectField ("Move path:", movePath, typeof (Paths), true);

			if (movePath)
			{
				lockedPathCanReverse = EditorGUILayout.Toggle ("Can reverse along path?", lockedPathCanReverse);
			}

			if (AllowHeadTurning ())
			{
				doHotspotHeadTurnLock = (LockType) EditorGUILayout.EnumPopup ("Hotspot head-turning?", doHotspotHeadTurnLock);
			}
		}


		public override bool ReferencesPlayer (int playerID = -1)
		{
			return true;
		}

		#endif


		protected bool AllowHeadTurning ()
		{
			if (SceneSettings.CameraPerspective != CameraPerspective.TwoD && KickStarter.settingsManager.playerFacesHotspots)
			{
				return true;
			}
			return false;
		}


		protected bool IsSingleLockMovement ()
		{
			if (KickStarter.settingsManager)
			{
				SettingsManager settingsManager = KickStarter.settingsManager;
				if (settingsManager.movementMethod == MovementMethod.PointAndClick || settingsManager.movementMethod == MovementMethod.Drag || settingsManager.movementMethod == MovementMethod.StraightToCursor)
				{
					return true;
				}
			}
			return false;
		}


		protected bool ChangingMovementLock ()
		{
			if (doUpLock != LockType.NoChange)
			{
				return true;
			}

			if (!IsSingleLockMovement ())
			{
				if (doDownLock != LockType.NoChange || doLeftLock != LockType.NoChange || doRightLock != LockType.NoChange)
				{
					return true;
				}
			}
			return false;
		}


		protected bool IsInFirstPerson ()
		{
			if (KickStarter.settingsManager && KickStarter.settingsManager.IsInFirstPerson ())
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Creates a new instance of the 'Player: Constrain' Action</summary>
		 * <param name = "movementLock">Whether or not to constrain movement in all directions</param>
		 * <param name = "jumpLock">Whether or not to constrain jumping</param>
		 * <param name = "freeAimLock">Whether or not to constrain free-aiming</param>
		 * <param name = "cursorLock">Whether or not to constrain the cursor</param>
		 * <param name = "movementSpeedLock">Whether or not to constrain movement speed</param>
		 * <param name = "hotspotHeadTurnLock">Whether or not to constrain Hotspot head-turning</param>
		 * <param name = "limitToPath">If set, a Path to constrain movement along, if using Direct movement</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionPlayerLock CreateNew (LockType movementLock, LockType jumpLock = LockType.NoChange, LockType freeAimLock = LockType.NoChange, LockType cursorLock = LockType.NoChange, PlayerMoveLock movementSpeedLock = PlayerMoveLock.NoChange, LockType gravityLock = LockType.NoChange, LockType hotspotHeadTurnLock = LockType.NoChange, Paths limitToPath = null)
		{
			ActionPlayerLock newAction = CreateNew<ActionPlayerLock> ();
			newAction.doUpLock = movementLock;
			newAction.doLeftLock = movementLock;
			newAction.doRightLock = movementLock;
			newAction.doDownLock = movementLock;
			newAction.doJumpLock = jumpLock;
			newAction.freeAimLock = freeAimLock;
			newAction.cursorState = cursorLock;
			newAction.doRunLock = movementSpeedLock;
			newAction.movePath = limitToPath;
			newAction.doHotspotHeadTurnLock = hotspotHeadTurnLock;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Player: Constrain' Action</summary>
		 * <param name = "upMovementLock">Whether or not to constrain movement in the Up direction</param>
		 * <param name = "downMovementLock">Whether or not to constrain movement in the Down direction</param>
		 * <param name = "leftMovementLock">Whether or not to constrain movement in the Left direction</param>
		 * <param name = "rightMovementLock">Whether or not to constrain movement in the Right direction</param>
		 * <param name = "jumpLock">Whether or not to constrain jumping</param>
		 * <param name = "freeAimLock">Whether or not to constrain free-aiming</param>
		 * <param name = "cursorLock">Whether or not to constrain the cursor</param>
		 * <param name = "movementSpeedLock">Whether or not to constrain movement speed</param>
		 * <param name = "hotspotHeadTurnLock">Whether or not to constrain Hotspot head-turning</param>
		 * <param name = "limitToPath">If set, a Path to constrain movement along, if using Direct movement</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionPlayerLock CreateNew (LockType upMovementLock, LockType downMovementLock, LockType leftMovementLock, LockType rightMovementLock, LockType jumpLock = LockType.NoChange, LockType freeAimLock = LockType.NoChange, LockType cursorLock = LockType.NoChange, PlayerMoveLock movementSpeedLock = PlayerMoveLock.NoChange, LockType gravityLock = LockType.NoChange, LockType hotspotHeadTurnLock = LockType.NoChange, Paths limitToPath = null)
		{
			ActionPlayerLock newAction = CreateNew<ActionPlayerLock> ();
			newAction.doUpLock = upMovementLock;
			newAction.doLeftLock = leftMovementLock;
			newAction.doRightLock = rightMovementLock;
			newAction.doDownLock = downMovementLock;
			newAction.doJumpLock = jumpLock;
			newAction.freeAimLock = freeAimLock;
			newAction.cursorState = cursorLock;
			newAction.doRunLock = movementSpeedLock;
			newAction.movePath = limitToPath;
			newAction.doHotspotHeadTurnLock = hotspotHeadTurnLock;
			return newAction;
		}

	}

}