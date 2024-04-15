/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionPlayerTeleportInactive.cs"
 * 
 *	Moves the recorded position of an inactive Player to the current scene.
 * 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace AC
{

	[System.Serializable]
	public class ActionPlayerTeleportInactive : Action
	{
		
		public int playerID;
		public int playerIDParameterID = -1;

		public PlayerStart newTransform;
		public int newTransformConstantID = 0;
		public int newTransformParameterID = -1;
		protected PlayerStart runtimePlayerStart;

		public bool moveToCurrentScene = true;
		private bool moveComplete;

		public TeleportPlayerStartMethod teleportPlayerStartMethod = TeleportPlayerStartMethod.SceneDefault;

		public ChooseSceneBy chooseSceneBy = ChooseSceneBy.Number;
		public string newSceneName;
		public int newSceneIndex;


		public override ActionCategory Category { get { return ActionCategory.Player; } }
		public override string Title { get { return "Teleport inactive"; } }
		public override string Description { get { return "Moves the recorded position of an inactive Player to the current scene."; } }
		public override bool RunNormallyWhenSkip { get { return true; } }


		public override void AssignValues (List<ActionParameter> parameters)
		{
			playerID = AssignInteger (parameters, playerIDParameterID, playerID);
			runtimePlayerStart = AssignFile (parameters, newTransformParameterID, newTransformConstantID, newTransform);
		}
		
		
		public override float Run ()
		{
			if (isRunning)
			{
				if (moveComplete)
				{
					isRunning = false;
					return 0f;
				}
				return defaultPauseTime;
			}

			moveComplete = false;
			if (moveToCurrentScene)
			{
				KickStarter.saveSystem.MoveInactivePlayerToCurrentScene (playerID, teleportPlayerStartMethod, runtimePlayerStart, OnCompleteMove);
			}
			else
			{
				switch (KickStarter.settingsManager.referenceScenesInSave)
				{
					case ChooseSceneBy.Name:
						string runtimeSceneName = (chooseSceneBy == ChooseSceneBy.Name) ? newSceneName : KickStarter.sceneChanger.IndexToName (newSceneIndex);
						KickStarter.saveSystem.MoveInactivePlayer (playerID, runtimeSceneName, teleportPlayerStartMethod, newTransformConstantID, OnCompleteMove);
						break;

					case ChooseSceneBy.Number:
					default:
						int runtimeSceneIndex = (chooseSceneBy == ChooseSceneBy.Name) ? KickStarter.sceneChanger.NameToIndex (newSceneName) : newSceneIndex;
						KickStarter.saveSystem.MoveInactivePlayer (playerID, runtimeSceneIndex, teleportPlayerStartMethod, newTransformConstantID, OnCompleteMove);
						break;
				}
			}

			isRunning = !moveComplete;
			return isRunning ? defaultPauseTime : 0f;
		}


		private void OnCompleteMove ()
		{
			moveComplete = true;
		}

		
		#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			if (KickStarter.settingsManager != null)
			{
				if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow)
				{
					EditorGUILayout.HelpBox ("This Action requires Player Switching to be allowed, as set in the Settings Manager.", MessageType.Info);
					return;
				}
				
				if (KickStarter.settingsManager.players.Count == 0)
				{
					EditorGUILayout.HelpBox ("No players are defined in the Settings Manager.", MessageType.Warning);
					return;
				}

				PlayerField ("New Player:", "New Player ID:", ref playerID, parameters, ref playerIDParameterID, false);

				moveToCurrentScene = EditorGUILayout.Toggle ("Move to current scene?", moveToCurrentScene);
				if (moveToCurrentScene)
				{
					teleportPlayerStartMethod = (TeleportPlayerStartMethod)EditorGUILayout.EnumPopup ("PlayerStart:", teleportPlayerStartMethod);

					if (teleportPlayerStartMethod == TeleportPlayerStartMethod.EnteredHere)
					{
						ComponentField ("New PlayerStart:", ref newTransform, ref newTransformConstantID, parameters, ref newTransformParameterID);
					}
				}
				else
				{
					chooseSceneBy = (ChooseSceneBy)EditorGUILayout.EnumPopup ("Choose scene by:", chooseSceneBy);
					switch (chooseSceneBy)
					{
						case ChooseSceneBy.Number:
							newSceneIndex = EditorGUILayout.IntField ("New scene index:", newSceneIndex);
							break;

						case ChooseSceneBy.Name:
							newSceneName = EditorGUILayout.TextField ("New scene name:", newSceneName);
							break;

						default:
							break;
					}

					teleportPlayerStartMethod = (TeleportPlayerStartMethod)EditorGUILayout.EnumPopup ("PlayerStart:", teleportPlayerStartMethod);

					if (teleportPlayerStartMethod == TeleportPlayerStartMethod.EnteredHere)
					{
						newTransformParameterID = -1;
						ComponentField ("New PlayerStart:", ref newTransform, ref newTransformConstantID);
					}
				}

				
			}
			else
			{
				EditorGUILayout.HelpBox ("No Settings Manager assigned!", MessageType.Warning);
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			newTransformConstantID = AssignConstantID<PlayerStart> (newTransform, newTransformConstantID, newTransformParameterID);
		}
		

		public override string SetLabel ()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow)
			{
				PlayerPrefab newPlayerPrefab = KickStarter.settingsManager.GetPlayerPrefab (playerID);
				if (newPlayerPrefab != null)
				{
					if (newPlayerPrefab.EditorPrefab != null)
					{
						return newPlayerPrefab.EditorPrefab.name;
					}
					else
					{
						return "Undefined prefab";
					}
				}
			}
			
			return string.Empty;
		}


		public override bool ReferencesObjectOrID (GameObject gameObject, int id)
		{
			if (newTransformParameterID < 0)
			{
				if (newTransform && newTransform.gameObject == gameObject) return true;
				if (newTransformConstantID == id && id != 0) return true;
			}
			return base.ReferencesObjectOrID (gameObject, id);
		}


		public override bool ReferencesPlayer (int _playerID = -1)
		{
			if (_playerID < 0 || playerIDParameterID >= 0) return false;
			return (playerID == _playerID);
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Player: Teleport inactive' Action</summary>
		 * <param name = "playerID">The ID number of the Player to teleport</param>
		 * <param name = "newPlayerStart">The new PlayerStart for the Player to take</param>
		 * <param name = "newCamera">If set, the camera that will be active when the Player is next switched to</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionPlayerTeleportInactive CreateNew (int playerID, PlayerStart newPlayerStart, _Camera newCamera = null)
		{
			ActionPlayerTeleportInactive newAction = CreateNew<ActionPlayerTeleportInactive> ();
			newAction.playerID = playerID;
			newAction.teleportPlayerStartMethod = TeleportPlayerStartMethod.EnteredHere;
			newAction.newTransform = newPlayerStart;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Player: Teleport inactive' Action</summary>
		 * <param name = "playerID">The ID number of the Player to teleport</param>
		 * <param name = "teleportPlayerStartMethod">The method by which to assign which PlayerStart the Player appears at</param>
		 * <param name = "newPlayerStart">The new PlayerStart for the Player to take, if teleportPlayerStartMethod = TeleportPlayerStartMethod.EnteredHere</param>
		 * <param name = "newCamera">If set, the camera that will be active when the Player is next switched to</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionPlayerTeleportInactive CreateNew (int playerID, TeleportPlayerStartMethod teleportPlayerStartMethod, PlayerStart newPlayerStart = null, _Camera newCamera = null)
		{
			ActionPlayerTeleportInactive newAction = CreateNew<ActionPlayerTeleportInactive> ();
			newAction.playerID = playerID;
			newAction.teleportPlayerStartMethod = teleportPlayerStartMethod;
			newAction.newTransform = newPlayerStart;
			return newAction;
		}

	}

}