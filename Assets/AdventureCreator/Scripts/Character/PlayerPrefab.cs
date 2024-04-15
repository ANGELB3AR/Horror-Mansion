/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"PlayerPrefab.cs"
 * 
 *	A data container for a Player that is spawned automatically at runtime, and whose data is tracked automatically.
 * 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if AddressableIsPresent
using UnityEngine.AddressableAssets;
#endif

namespace AC
{

	/** A data container for a Player that is spawned automatically at runtime, and whose data is tracked automatically. */
	[System.Serializable]
	public class PlayerPrefab
	{

		#region Variables

		/** The Player prefab */
		public Player playerOb;

		#if AddressableIsPresent
		/** The Player prefab's asset reference, if using Addressables */
		public AssetReference playerAssetReference = new AssetReference ();
		/** A default set of PlayerData */
		public PlayerData defaultPlayerData;
		#endif

		/** A unique identifier */
		public int ID;
		/** If True, this Player is the game's default */
		public bool isDefault;
		/** The scene index to start in, if the Player is not the default, and chooseSceneBy = ChooseSceneBy.Number */
		public int startingSceneIndex = 0;
		/** How to reference the Player's starting scene, if not the default (Name, Number) */
		public ChooseSceneBy chooseSceneBy = ChooseSceneBy.Number;
		/** The name of the scene to start in, if the Player is not the default, and chooseSceneBy = ChooseSceneBy.Name */
		public string startingSceneName = "";
		/** If True, then the Player will appear at their initial scene's Default PlayerStart - as opposed to one specified here */
		public bool useSceneDefaultPlayerStart = true;
		/** The ConstantID value of the PlayerStart to appear at, if not the default Player */
		public int startingPlayerStartID;
		
		#endregion


		#region Constructors

		public PlayerPrefab (Player _playerOb)
		{
			playerOb = _playerOb;
			ID = 0;
		}


		/**
		 * The default Constructor.
		 * An array of ID numbers is required, to ensure its own ID is unique.
		 */
		public PlayerPrefab (int[] idArray)
		{
			ID = 0;
			playerOb = null;

			if (idArray.Length > 0)
			{
				isDefault = false;

				foreach (int _id in idArray)
				{
					if (ID == _id)
						ID++;
				}
			}
			else
			{
				isDefault = true;
			}

			startingSceneIndex = 0;
			startingSceneName = string.Empty;
			chooseSceneBy = ChooseSceneBy.Number;
			startingPlayerStartID = 0;
		}

		#endregion


		#region PublicFunctions

		/**
		 * <summary>Gets the runtime scene instance of the Player</summary>
		 * <param name = "spawnIfNotPresent">If True, the Player will be spawned if no scene instance was found.</param>
		 * <returns>The scene instance of the Player</returns>
		 */
		public Player GetSceneInstance ()
		{
			Player[] scenePlayers = UnityVersionHandler.FindObjectsOfType<Player> ();
			foreach (Player scenePlayer in scenePlayers)
			{
				if (scenePlayer.ID == ID)
				{
					return scenePlayer;
				}
			}

			return null;
		}


		public PlayerData GetPlayerData ()
		{
			Player player = GetSceneInstance ();
			PlayerData playerData = new PlayerData ();

			if (player)
			{
				return player.SaveData (playerData);
			}
			return GetDefaultPlayerData ();
		}


		public void SetInitialPosition (PlayerData playerData)
		{
			TeleportPlayerStartMethod teleportPlayerStartMethod = (useSceneDefaultPlayerStart) ? TeleportPlayerStartMethod.SceneDefault : TeleportPlayerStartMethod.EnteredHere;

			switch (KickStarter.settingsManager.referenceScenesInSave)
			{
				case ChooseSceneBy.Name:
					playerData.UpdatePosition (StartingSceneName, teleportPlayerStartMethod, startingPlayerStartID);
					break;

				case ChooseSceneBy.Number:
				default:
					playerData.UpdatePosition (StartingSceneIndex, teleportPlayerStartMethod, startingPlayerStartID);
					break;
			}
		}


		/** Removes any runtime instance of the Player from the scene */
		public void RemoveFromScene ()
		{
			if (IsValid ())
			{
				Player sceneInstance = GetSceneInstance ();
				if (sceneInstance)
				{
					sceneInstance.RemoveFromScene ();
				}
			}
		}

		#endregion


		#region PrivateFunctions

#if UNITY_EDITOR && AddressableIsPresent

		private void CacheAddressablePlayerData ()
		{
			if (KickStarter.settingsManager.savePlayerReferencesWithAddressables)
			{
				if (IsValid (false))
				{
					PlayerData playerData = new PlayerData ();
					defaultPlayerData = EditorPrefab.SaveData (playerData);
					return;
				}
				ACDebug.LogWarning ("Cannot cache PlayerData for Addressable Player " + ID);
			}
			defaultPlayerData = new PlayerData ();
		}

#endif


		private PlayerData GetDefaultPlayerData ()
		{
			#if AddressableIsPresent
			if (KickStarter.settingsManager.savePlayerReferencesWithAddressables)
			{
				#if UNITY_EDITOR
				CacheAddressablePlayerData ();
				#endif
				return defaultPlayerData;
			}
			#endif

			if (playerOb)
			{
				PlayerData playerData = new PlayerData ();
				playerData = playerOb.SaveData (playerData);
				return playerData;
			}
			return new PlayerData ();
		}

		#endregion


		#region GetSet

		private int StartingSceneIndex
		{
			get
			{
				if (KickStarter.settingsManager && isDefault)
				{
					return -1;
				}

				if (chooseSceneBy == ChooseSceneBy.Name) return KickStarter.sceneChanger.NameToIndex (startingSceneName);
				return startingSceneIndex;
			}
			set
			{
				startingSceneIndex = value;
				chooseSceneBy = ChooseSceneBy.Number;
			}
		}


		private string StartingSceneName
		{
			get
			{
				if (KickStarter.settingsManager && isDefault)
				{
					return string.Empty;
				}

				if (chooseSceneBy == ChooseSceneBy.Name) return startingSceneName;
				return KickStarter.sceneChanger.IndexToName (startingSceneIndex);
			}
			set
			{
				startingSceneName = value;
				chooseSceneBy = ChooseSceneBy.Name;
			}
		}
		

		public bool IsValid (bool warnIfNot = true)
		{
			#if AddressableIsPresent
			if (KickStarter.settingsManager.savePlayerReferencesWithAddressables && playerAssetReference != null)
			{
				if (!playerAssetReference.RuntimeKeyIsValid () && warnIfNot)
				{
					ACDebug.LogWarning ("Invalid runtime key for Player prefab " + ID);
				}

				#if UNITY_EDITOR
				return playerAssetReference.editorAsset;
				
				#else
				return playerAssetReference.RuntimeKeyIsValid ();
				#endif
			}
			#endif
			return playerOb;
		}

		#endregion


		#if UNITY_EDITOR

		public Player EditModeLoadAddressable ()
		{
			if (!IsValid ())
			{
				return null;
			}

#if AddressableIsPresent
			var path = AssetDatabase.GUIDToAssetPath (playerAssetReference.RuntimeKey.ToString ());
			var type = AssetDatabase.GetMainAssetTypeAtPath (path);
			if (type != typeof(GameObject))
			{
				return null;
			}

			var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (go)
			{
				return go.GetComponent<Player> ();
			}
#endif
			return null;
		}
 
		
		public Player EditorPrefab
		{
			get
			{
#if AddressableIsPresent
				if (KickStarter.settingsManager && KickStarter.settingsManager.savePlayerReferencesWithAddressables)
				{
					if (!IsValid (false))
					{
						return null;
					}

					if (playerAssetReference != null && playerAssetReference.editorAsset != null)
					{
						GameObject go = (GameObject) playerAssetReference.editorAsset;
						if (go)
						{
							return go.GetComponent<Player> ();
						}
					}
					return null;
				}
				else
#endif
				{
					return playerOb;
				}
			}
		}


		private static void PlayerCallback (object obj)
		{
			switch (obj.ToString ())
			{
				case "FindReferences":
					PlayerPrefab.FindPlayerReferences (-1, KickStarter.settingsManager.PlayerPrefab.EditorPrefab.GetName ());
					break;

				default:
					break;
			}
		}


		public void ShowGUI (string apiPrefix)
		{
			EditorGUILayout.BeginHorizontal ();

			if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow)
			{
#if AddressableIsPresent
				if (KickStarter.settingsManager.savePlayerReferencesWithAddressables)
				{
					EditorGUILayout.BeginHorizontal ();
					var serializedObject = new SerializedObject (KickStarter.settingsManager);
					serializedObject.Update ();
					SerializedProperty playerPrefabProp = serializedObject.FindProperty ("playerPrefab");
					SerializedProperty playerProp = playerPrefabProp.FindPropertyRelative (nameof (playerAssetReference));
					EditorGUILayout.PropertyField (playerProp, true);
					serializedObject.ApplyModifiedProperties ();

					if (playerAssetReference.RuntimeKeyIsValid ())
					{
						if (GUILayout.Button (string.Empty, CustomStyles.IconCog))
						{
							GenericMenu menu = new GenericMenu ();
							menu.AddItem (new GUIContent ("Find references..."), false, PlayerCallback, "FindReferences");
							menu.ShowAsContext ();
						}
					}

					EditorGUILayout.EndHorizontal ();
				}
				else
#endif
				{
					playerOb = (Player) CustomGUILayout.ObjectField <Player> ("Player prefab:", playerOb, false, "AC.KickStarter.settingsManager.player", "The player prefab, to spawn in at runtime");
				}
			}
			else
			{
				string label = "Player " + ID + ":";
				if (isDefault)
				{
					label += " (DEFAULT)";
				}

#if AddressableIsPresent
				if (KickStarter.settingsManager && KickStarter.settingsManager.savePlayerReferencesWithAddressables)
				{
					for (int i = 0; i < KickStarter.settingsManager.players.Count; i++)
					{
						if (this == KickStarter.settingsManager.players[i])
						{
							var serializedObject = new SerializedObject (KickStarter.settingsManager);
							serializedObject.Update ();
							SerializedProperty playersProp = serializedObject.FindProperty (nameof (KickStarter.settingsManager.players));
							SerializedProperty playerProp = playersProp.GetArrayElementAtIndex (i);
							SerializedProperty assetProp = playerProp.FindPropertyRelative (nameof (playerAssetReference));

							EditorGUILayout.PropertyField (assetProp, true);
							serializedObject.ApplyModifiedProperties ();

						}
					}
				}
				else
#endif
				{
					playerOb = (Player) CustomGUILayout.ObjectField<Player> (label, playerOb, false, "AC.KickStarter.settingsManager.players");
				}

				if (GUILayout.Button (string.Empty, CustomStyles.IconCog))
				{
					SideMenu (this);
				}
			}

			EditorGUILayout.EndHorizontal ();
		}


		public void ShowStartDataGUI (string apiPrefix)
		{
			GUILayout.Label ("Starting point data for Player " + ID.ToString () + ": " + (EditorPrefab ? EditorPrefab.name : "(EMPTY)"), EditorStyles.boldLabel);

			chooseSceneBy = (ChooseSceneBy)CustomGUILayout.EnumPopup ("Choose scene by:", chooseSceneBy);
			switch (chooseSceneBy)
			{
				case ChooseSceneBy.Name:
					startingSceneName = CustomGUILayout.TextField ("Scene name:", startingSceneName);
					break;

				case ChooseSceneBy.Number:
					startingSceneIndex = CustomGUILayout.IntField ("Scene index:", startingSceneIndex);
					break;
			}

			useSceneDefaultPlayerStart = EditorGUILayout.Toggle ("Use default PlayerStart?", useSceneDefaultPlayerStart);
			if (!useSceneDefaultPlayerStart)
			{
				PlayerStart playerStart = ConstantID.GetComponent <PlayerStart> (startingPlayerStartID);
				playerStart = (PlayerStart)CustomGUILayout.ObjectField<PlayerStart> ("PlayerStart:", playerStart, true, apiPrefix + ".startingPlayerStartID", "The PlayerStart that this character starts from.");
				startingPlayerStartID = FieldToID<PlayerStart> (playerStart, startingPlayerStartID);

				if (startingPlayerStartID != 0)
				{
					CustomGUILayout.BeginVertical ();
					EditorGUILayout.LabelField ("Recorded ConstantID: " + startingPlayerStartID.ToString (), EditorStyles.miniLabel);
					CustomGUILayout.EndVertical ();
				}
			}
		}


		private int FieldToID<T> (T field, int _constantID) where T : Component
		{
			if (field == null)
			{
				return _constantID;
			}

			if (field.GetComponent<ConstantID> ())
			{
				if (!field.gameObject.activeInHierarchy && field.GetComponent<ConstantID> ().constantID == 0)
				{
					UnityVersionHandler.AddConstantIDToGameObject<ConstantID> (field.gameObject, true);
				}
				_constantID = field.GetComponent<ConstantID> ().constantID;
			}
			else
			{
				UnityVersionHandler.AddConstantIDToGameObject<ConstantID> (field.gameObject, true);
				AssetDatabase.SaveAssets ();
				_constantID = field.GetComponent<ConstantID> ().constantID;
			}

			return _constantID;
		}


		private static int sidePlayerPrefab = -1;
		private static void SideMenu (PlayerPrefab playerPrefab)
		{
			GenericMenu menu = new GenericMenu ();
			sidePlayerPrefab = KickStarter.settingsManager.players.IndexOf (playerPrefab);

			if (!playerPrefab.isDefault)
			{
				menu.AddItem (new GUIContent ("Set as default"), false, Callback, "SetAsDefault");
				menu.AddItem (new GUIContent ("Edit start data..."), false, Callback, "EditStartData");
				menu.AddSeparator (string.Empty);
			}

			menu.AddItem (new GUIContent ("Find references..."), false, Callback, "FindReferences");
			menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
			
			menu.ShowAsContext ();
		}


		private static void Callback (object obj)
		{
			if (sidePlayerPrefab >= 0)
			{
				switch (obj.ToString ())
				{
					case "Delete":
						bool isDefault = KickStarter.settingsManager.players[sidePlayerPrefab].isDefault;
						Undo.RecordObject (KickStarter.settingsManager, "Delete player reference");
						KickStarter.settingsManager.players.RemoveAt (sidePlayerPrefab);
						if (isDefault && KickStarter.settingsManager.players.Count > 0)
						{
							KickStarter.settingsManager.players[0].isDefault = true;
						}
						break;

					case "SetAsDefault":
						for (int i=0; i<KickStarter.settingsManager.players.Count; i++)
						{
							KickStarter.settingsManager.players[i].isDefault = (i == sidePlayerPrefab);
						}
						break;

					case "EditStartData":
						PlayerStartDataEditor.CreateNew (sidePlayerPrefab);
						break;

					case "FindReferences":
						PlayerPrefab playerPrefab = KickStarter.settingsManager.players[sidePlayerPrefab];
						FindPlayerReferences (playerPrefab.ID, (playerPrefab.EditorPrefab != null) ? playerPrefab.EditorPrefab.GetName () : "(Unnamed)");
						break;

					default:
						break;
				}
			}
		}


		public static void FindPlayerReferences (int playerID, string playerName)
		{
			if (EditorUtility.DisplayDialog ("Search Player '" + playerName + "' references?", "The Editor will search ActionList assets, and scenes listed in the Build Settings, for references to this Player.  The current scene will need to be saved and listed to be included in the search process. Continue?", "OK", "Cancel"))
			{
				if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ())
				{
					// ActionList assets
					if (KickStarter.speechManager != null)
					{
						ActionListAsset[] allActionListAssets = KickStarter.speechManager.GetAllActionListAssets ();
						foreach (ActionListAsset actionListAsset in allActionListAssets)
						{
							SearchActionListAssetForPlayerReferences (playerID, playerName, actionListAsset);
						}
					}

					// Scenes
					string originalScene = UnityVersionHandler.GetCurrentSceneFilepath ();
					string[] sceneFiles = AdvGame.GetSceneFiles ();

					foreach (string sceneFile in sceneFiles)
					{
						UnityVersionHandler.OpenScene (sceneFile);

						string suffix = " in scene '" + sceneFile + "'";
						SearchSceneForPlayerReferences (playerID, playerName, suffix);
					}

					UnityVersionHandler.OpenScene (originalScene);
				}
			}
		}


		private static void SearchSceneForPlayerReferences (int playerID, string playerName, string suffix)
		{
			ActionList[] localActionLists = UnityVersionHandler.FindObjectsOfType<ActionList> ();
			foreach (ActionList actionList in localActionLists)
			{
				if (actionList.source == ActionListSource.InScene)
				{
					foreach (Action action in actionList.actions)
					{
						if (action != null)
						{
							if (action.ReferencesPlayer (playerID))
							{
								string actionLabel = (KickStarter.actionsManager != null) ? (" (" + KickStarter.actionsManager.GetActionTypeLabel (action) + ")") : "";
								Debug.Log ("'" + playerName + "' is referenced by Action #" + actionList.actions.IndexOf (action) + actionLabel + " in ActionList '" + actionList.gameObject.name + "'" + suffix, actionList);
							}
						}
					}
				}
				else if (actionList.source == ActionListSource.AssetFile)
				{
					SearchActionListAssetForPlayerReferences (playerID, playerName, actionList.assetFile);
				}
			}
		}


		private static void SearchActionListAssetForPlayerReferences (int playerID, string playerName, ActionListAsset actionListAsset)
		{
			if (actionListAsset == null) return;

			foreach (Action action in actionListAsset.actions)
			{
				if (action != null)
				{
					if (action.ReferencesPlayer (playerID))
					{
						string actionLabel = (KickStarter.actionsManager != null) ? (" (" + KickStarter.actionsManager.GetActionTypeLabel (action) + ")") : "";
						Debug.Log ("'" + playerName + "' is referenced by Action #" + actionListAsset.actions.IndexOf (action) + actionLabel + " in ActionList asset '" + actionListAsset.name + "'", actionListAsset);
					}
				}
			}
		}

#endif

		}

}