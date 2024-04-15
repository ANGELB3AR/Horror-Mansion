using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if AddressableIsPresent
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace AC
{

	public class PlayerSpawner : MonoBehaviour
	{

		#region Variables

		#if AddressableIsPresent
		private readonly Dictionary<PlayerPrefab, AsyncOperationHandle<GameObject>> playerAddressableDict = new Dictionary<PlayerPrefab, AsyncOperationHandle<GameObject>> ();
		#endif

		#endregion


		#region UnityStandards

		private void OnDestroy ()
		{
			#if AddressableIsPresent
			foreach (var handle in playerAddressableDict.Values)
			{
				if (handle.IsValid ())
					Addressables.Release (handle);
			}
			playerAddressableDict.Clear ();
			#endif
		}

		#endregion


		#region PublicFunctions

		public void ReleaseHandle (PlayerPrefab playerPrefab)
		{
			#if AddressableIsPresent
			if (playerAddressableDict.ContainsKey (playerPrefab))
			{
				var handle = playerAddressableDict[playerPrefab];
				Addressables.Release (handle);
				playerAddressableDict.Remove (playerPrefab);
			}
			#endif
		}


		public IEnumerator PreparePlayer ()
		{
			var spawnAllPlayersCoroutine = KickStarter.saveSystem.SpawnAllPlayers ();
			while (spawnAllPlayersCoroutine.MoveNext ())
			{
				yield return spawnAllPlayersCoroutine.Current;
			}

			Player[] localPlayers = UnityVersionHandler.FindObjectsOfType<Player> ();

			switch (KickStarter.settingsManager.playerSwitching)
			{
				case PlayerSwitching.Allow:
					{
						// Local players are ignored
						foreach (Player localPlayer in localPlayers)
						{
							if (localPlayer.ID <= -1)
							{
								ACDebug.LogWarning ("Local Player " + localPlayer.GetName () + " found in scene " + localPlayer.gameObject.scene.name + ". This is not allowed when Player Switching is enabled - in this mode, Players can only be spawned in.", localPlayer);
							}
						}

						PlayerPrefab playerPrefab = KickStarter.settingsManager.GetPlayerPrefab (KickStarter.saveSystem.CurrentPlayerID);
						if (playerPrefab != null)
						{
							var spawnPlayerCoroutine = SpawnPlayerWithData (playerPrefab, true, null);
							while (spawnPlayerCoroutine.MoveNext ())
							{
								yield return spawnPlayerCoroutine.Current;
							}
						}
					}
					break;

				case PlayerSwitching.DoNotAllow:
					{
						// Local players take priority

						int localID = 0;
						switch (KickStarter.settingsManager.referenceScenesInSave)
						{
							case ChooseSceneBy.Name:
								localID = -2 - Mathf.Abs (SceneChanger.CurrentSceneName.GetHashCode ()); // Always unique to the same, but not needing building index
								break;

							case ChooseSceneBy.Number:
							default:
								localID = -2 - SceneChanger.CurrentSceneIndex; // Always unique to the scene
								break;
						}

						// Prioritise expected ID
						foreach (Player localPlayer in localPlayers)
						{
							if (localPlayer.ID == localID)
							{
								KickStarter.player = localPlayer;
								yield break;
							}
						}

						// Priotisie default ID
						foreach (Player localPlayer in localPlayers)
						{
							if (localPlayer.ID == -1)
							{
								var setIDCoroutine = localPlayer.SetID (localID);
								while (setIDCoroutine.MoveNext ())
								{
									yield return setIDCoroutine.Current;
								}

								KickStarter.player = localPlayer;
								yield break;
							}
						}

						// Use what's left
						foreach (Player localPlayer in localPlayers)
						{
							KickStarter.player = localPlayer;
							yield break;
						}

						var spawnPlayerCoroutine = SpawnPlayerCo (KickStarter.settingsManager.PlayerPrefab, (r) => KickStarter.player = r);
						while (spawnPlayerCoroutine.MoveNext ())
						{
							yield return spawnPlayerCoroutine.Current;
						}
					}
					break;

				default:
					break;
			}

			if (KickStarter.player == null && KickStarter.settingsManager.movementMethod != MovementMethod.None)
			{
				ACDebug.LogWarning ("No Player found - this can be assigned in the Settings Manager.");
			}

			if (KickStarter.player)
			{
				KickStarter.player.EndPath ();
				KickStarter.player.Halt (false);
			}

			var spawnFollowingCoroutine = KickStarter.saveSystem.SpawnFollowingPlayers ();
			while (spawnFollowingCoroutine.MoveNext ())
			{
				yield return spawnFollowingCoroutine.Current;
			}
		}


		public void SpawnPlayer (PlayerPrefab playerPrefab, System.Action<Player> callback)
		{
			StartCoroutine (SpawnPlayerCo (playerPrefab, callback));
		}


		public IEnumerator LoadPlayerHandle (PlayerPrefab playerPrefab)
		{
			#if AddressableIsPresent
			if (KickStarter.settingsManager.savePlayerReferencesWithAddressables)
			{
				if (playerAddressableDict.ContainsKey (playerPrefab) || !playerPrefab.IsValid ())
				{
					yield break;
				}

				var handle = playerPrefab.playerAssetReference.LoadAssetAsync<GameObject> ();
				yield return handle;
				if (handle.Result != null)
				{
					playerAddressableDict.Add (playerPrefab, handle);
				}
			}
			else
			#endif
			yield break;
		}


		public Player GetPlayerAsset (PlayerPrefab playerPrefab)
		{
			#if AddressableIsPresent
			if (KickStarter.settingsManager.savePlayerReferencesWithAddressables)
			{
				if (playerAddressableDict.ContainsKey (playerPrefab))
				{
					return playerAddressableDict[playerPrefab].Result.GetComponent<Player> ();
				}
				return null;
			}
			#endif
			return playerPrefab.playerOb;
		}


		public IEnumerator SpawnPlayerCo (PlayerPrefab playerPrefab, System.Action<Player> callback)
		{
			int _ID = playerPrefab.ID;

			#if AddressableIsPresent
			if (KickStarter.settingsManager.savePlayerReferencesWithAddressables)
			{
				var loadPlayerHandleCoroutine = LoadPlayerHandle (playerPrefab);
				while (loadPlayerHandleCoroutine.MoveNext ())
				{
					yield return loadPlayerHandleCoroutine.Current;
				}

				Player assetPlayer = GetPlayerAsset (playerPrefab);
				Player spawnedPlayer = Instantiate (assetPlayer.gameObject).GetComponent<Player> ();

				spawnedPlayer.gameObject.name = assetPlayer.gameObject.name;
				var setIDCoroutine = spawnedPlayer.SetID (_ID);
				while (setIDCoroutine.MoveNext ())
				{
					yield return setIDCoroutine.Current;
				}

				if (_ID >= 0)
				{
					ACDebug.Log ("Spawned instance of Player '" + spawnedPlayer.GetName () + "'.", spawnedPlayer);
				}
				else
				{
					ACDebug.Log ("Spawned instance of Player '" + spawnedPlayer.GetName () + "' into scene " + spawnedPlayer.gameObject.scene.name + ".", spawnedPlayer);
				}

				if (KickStarter.eventManager)
				{
					KickStarter.eventManager.Call_OnPlayerSpawn (spawnedPlayer);
				}

				callback?.Invoke (spawnedPlayer);
				yield break;
			}
			else
			#endif
			if (playerPrefab.playerOb)
			{
				Player spawnedPlayer = Instantiate (playerPrefab.playerOb);

				spawnedPlayer.gameObject.name = playerPrefab.playerOb.gameObject.name;
				var setIDCoroutine = spawnedPlayer.SetID (_ID);
				while (setIDCoroutine.MoveNext ())
				{
					yield return setIDCoroutine.Current;
				}

				if (_ID >= 0)
				{
					ACDebug.Log ("Spawned instance of Player '" + spawnedPlayer.GetName () + "'.", spawnedPlayer);
				}
				else
				{
					ACDebug.Log ("Spawned instance of Player '" + spawnedPlayer.GetName () + "' into scene " + spawnedPlayer.gameObject.scene.name + ".", spawnedPlayer);
				}

				if (KickStarter.eventManager)
				{
					KickStarter.eventManager.Call_OnPlayerSpawn (spawnedPlayer);
				}

				if (callback != null) callback.Invoke (spawnedPlayer);
				yield break;
			}

			if (callback != null) callback.Invoke (null);
		}


		public void UpdatePlayerPresenceInScene (PlayerData playerData, System.Action callback)
		{
			StartCoroutine (UpdatePlayerPresenceInSceneCo (playerData, callback));
		}


		/** Updates the Player's presence in the scene. According to the data set in this class, they will be added to or removed from the scene. */
		public IEnumerator UpdatePlayerPresenceInSceneCo (PlayerData playerData, System.Action callback)
		{
			PlayerPrefab playerPrefab = KickStarter.settingsManager.GetPlayerPrefab (playerData.playerID);
			if (playerPrefab != null)
			{
				if (KickStarter.saveSystem.CurrentPlayerID == playerData.playerID)
				{
					var spawnPlayerCoroutine = SpawnPlayerWithData (playerPrefab, false, null);
					while (spawnPlayerCoroutine.MoveNext ())
					{
						yield return spawnPlayerCoroutine.Current;
					}
				}
				else if ((KickStarter.settingsManager.referenceScenesInSave == ChooseSceneBy.Name && SceneChanger.CurrentSceneName == playerData.currentSceneName) ||
						 (KickStarter.settingsManager.referenceScenesInSave == ChooseSceneBy.Number && SceneChanger.CurrentSceneIndex == playerData.currentScene))
				{
					var spawnPlayerCoroutine = SpawnPlayerWithData (playerPrefab, false, null);
					while (spawnPlayerCoroutine.MoveNext ())
					{
						yield return spawnPlayerCoroutine.Current;
					}
				}
				else
				{
					SubScene subScene = null;

					switch (KickStarter.settingsManager.referenceScenesInSave)
					{
						case ChooseSceneBy.Name:
							subScene = KickStarter.sceneChanger.GetSubScene (playerData.currentSceneName);
							break;

						case ChooseSceneBy.Number:
						default:
							subScene = KickStarter.sceneChanger.GetSubScene (playerData.currentScene);
							break;
					}

					if (subScene != null)
					{
						var spawnPlayerCoroutine = SpawnPlayerWithData (playerPrefab, subScene.gameObject.scene, null);
						while (spawnPlayerCoroutine.MoveNext ())
						{
							yield return spawnPlayerCoroutine.Current;
						}
					}
					else
					{
						playerPrefab.RemoveFromScene ();
					}
				}
			}

			if (callback != null) callback.Invoke ();
		}


		public IEnumerator SpawnPlayerIfFollowingActive (PlayerData playerData)
		{
			if (KickStarter.saveSystem.CurrentPlayerID != playerData.playerID &&
				playerData.followTargetIsPlayer &&
				playerData.followAcrossScenes)
			{
				switch (KickStarter.settingsManager.referenceScenesInSave)
				{
					case ChooseSceneBy.Name:
						if (playerData.currentSceneName == SceneChanger.CurrentSceneName)
						{
							yield break;
						}
						break;

					case ChooseSceneBy.Number:
					default:
						if (playerData.currentScene == SceneChanger.CurrentSceneIndex)
						{
							yield break;
						}
						break;
				}

				playerData.ClearPathData ();
				playerData.UpdatePosition (SceneChanger.CurrentSceneIndex, TeleportPlayerStartMethod.BasedOnPrevious, 0);

				var updatePresenceCoroutine = UpdatePlayerPresenceInSceneCo (playerData, null);
				while (updatePresenceCoroutine.MoveNext ())
				{
					yield return updatePresenceCoroutine.Current;
				}
			}
		}

		#endregion


		#region PrivateFunctions

		private IEnumerator SpawnPlayerWithData (PlayerPrefab playerPrefab, bool makeActivePlayer, System.Action<Player> callback)
		{
			if (playerPrefab.IsValid ())
			{
				Player sceneInstance = playerPrefab.GetSceneInstance ();

				if (sceneInstance == null)
				{
					var spawnPlayerCoroutine = SpawnPlayerCo (playerPrefab, (r) => sceneInstance = r);
					while (spawnPlayerCoroutine.MoveNext ())
					{
						yield return spawnPlayerCoroutine.Current;
					}
				}

				if (makeActivePlayer)
				{
					KickStarter.player = sceneInstance;
				}

				PlayerData playerData = KickStarter.saveSystem.GetPlayerData (playerPrefab.ID);
				if (playerData != null)
				{
					var loadPlayerDataCoroutine = sceneInstance.LoadData (playerData);
					while (loadPlayerDataCoroutine.MoveNext ())
					{
						yield return loadPlayerDataCoroutine.Current;
					}

					if (callback != null) callback.Invoke (sceneInstance);
					yield break;
				}
			}

			if (callback != null) callback.Invoke (null);
		}


		private IEnumerator SpawnPlayerWithData (PlayerPrefab playerPrefab, Scene scene, System.Action<Player> callback)
		{
			if (playerPrefab.IsValid ())
			{
				Player sceneInstance = playerPrefab.GetSceneInstance ();

				if (sceneInstance == null)
				{
					var spawnPlayerCoroutine = SpawnPlayerCo (playerPrefab, (r) => sceneInstance = r);
					while (spawnPlayerCoroutine.MoveNext ())
					{
						yield return spawnPlayerCoroutine.Current;
					}
				}

				UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene (sceneInstance.gameObject, scene);

				PlayerData playerData = KickStarter.saveSystem.GetPlayerData (playerPrefab.ID);
				if (playerData != null)
				{
					var loadPlayerDataCoroutine = sceneInstance.LoadData (playerData);
					while (loadPlayerDataCoroutine.MoveNext ())
					{
						yield return loadPlayerDataCoroutine.Current;
					}
				}

				if (callback != null) callback.Invoke (sceneInstance);
				yield break;
			}

			if (callback != null) callback.Invoke (null);
		}

		#endregion

	}

}