/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"SceneItem.cs"
 * 
 *	A scene-object representation of an Inventory item
 * 
 */

using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/** A scene-object representation of an Inventory item */
	[AddComponentMenu ("Adventure Creator/Inventory/Scene Item")]
	[HelpURL ("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_scene_item.html")]
	public class SceneItem : MonoBehaviour
	{

		#region Variables

		private InvInstance linkedInvInstance;

		#endregion


		#region UnityStandards

		private void OnDisable ()
		{
			if (Application.isPlaying && KickStarter.saveSystem && !KickStarter.sceneChanger.IsLoading () && InvInstance.IsValid (LinkedInvInstance))
			{
				SaveRememberDataToLinkedInstance ();
			}
		}

		#endregion


		#region PublicFunctions

		/** Initialises the component.  This needs to be called if the object it is attached to is generated/spawned at runtime manually through code. */
		public void OnSpawn ()
		{
			ConstantID constantID = GetComponent<ConstantID> ();
			if (constantID && !constantID.retainInPrefab && constantID.constantID == 0)
			{
				int newID = GetInstanceID ();
				ConstantID[] idScripts = GetComponents<ConstantID> ();
				foreach (ConstantID idScript in idScripts)
				{
					idScript.constantID = newID;
				}

				ACDebug.Log ("Spawned new instance of " + gameObject.name + ", given new ID: " + newID, this);
			}
		}


		public void AssignLinkedInvInstance (InvInstance invInstance, bool applyRememberData = true, System.Action callback = null)
		{
			if (applyRememberData)
			{
				StartCoroutine (AssignLinkedInvInstanceCo (invInstance, applyRememberData, callback));
				return;
			}

			if (!InvInstance.IsValid (invInstance)) return;

			linkedInvInstance = invInstance;
			if (callback != null)
			{
				callback.Invoke ();
			}
		}


		/**
		 * <summary>Sets the Inventory Item to link with the object</summary>
		 * <param name = "invInstance">The InvInstance to link</param>
		 * <param name = "applyRememberData">If True, then any backup data for the SceneItem's Remember components will be applied</param>
		 * <param name = "callback">A callback to invoke once the assignment is complete.  This may take more than one frame if there is Remember data to apply</param>
		 */
		public IEnumerator AssignLinkedInvInstanceCo (InvInstance invInstance, bool applyRememberData = true, System.Action callback = null)
		{
			if (!InvInstance.IsValid (invInstance))
			{
				yield break;
			}

			linkedInvInstance = invInstance;
			
			if (applyRememberData && !string.IsNullOrEmpty (linkedInvInstance.SceneItemRememberData))
			{
				List<LevelStorage.RememberDataPairing> dataPairingList = new List<LevelStorage.RememberDataPairing> ();
				
				AllRememberData allRememberData = JsonUtility.FromJson<AllRememberData> (linkedInvInstance.SceneItemRememberData);
				if (allRememberData == null)
				{
					yield break;
				}

				List<ScriptData> allScriptData = allRememberData.allScriptData;

				Remember[] remembers = GetRemembersToRecord ();
				if (remembers.Length > 0)
				{
					foreach (ScriptData _scriptData in allScriptData)
					{
						if (!string.IsNullOrEmpty (_scriptData.data))
						{
							foreach (Remember remember in remembers)
							{
								if (remember == null || !remember.isActiveAndEnabled || remember is RememberSceneItem) continue;

								if (remember.constantID == _scriptData.objectID)
								{
									LevelStorage.RememberDataPairing rememberDataPairing = new LevelStorage.RememberDataPairing (remember, _scriptData);
									dataPairingList.Add (rememberDataPairing);
								}
							}
						}
					}
				}

				dataPairingList.Sort (delegate (LevelStorage.RememberDataPairing a, LevelStorage.RememberDataPairing b) { return a.Remember.LoadOrder.CompareTo (b.Remember.LoadOrder); });
				foreach (LevelStorage.RememberDataPairing dataPairing in dataPairingList)
				{
					var loadDataCoroutine = dataPairing.Remember.LoadDataCo (dataPairing.ScriptData.data);
					while (loadDataCoroutine.MoveNext ())
					{
						yield return loadDataCoroutine.Current;
					}
				}

				if (KickStarter.settingsManager.autoCallUnloadUnusedAssets)
				{
					AssetLoader.UnloadAssets ();
				}
			}

			if (callback != null)
			{
				callback.Invoke ();
			}
		}


		/** Updates the linked InvInstance with the Remember data of this object and its children */
		public void SaveRememberDataToLinkedInstance ()
		{
			if (!InvInstance.IsValid (LinkedInvInstance))
			{
				ACDebug.LogWarning ("Scene Item " + name + " is not linked to a valid InvInstance", this);
				return;
			}

			List<ScriptData> allScriptData = new List<ScriptData>();
			Remember[] remembers = GetRemembersToRecord ();

			foreach (Remember remember in remembers)
			{
				if (remember.constantID == 0 || remember is RememberSceneItem) continue;
				allScriptData.Add (new ScriptData (remember.constantID, remember.SaveData ()));
			}

			AllRememberData allRememberData = new AllRememberData (allScriptData);
			string scriptDataAsJson = (allScriptData.Count > 0) ? JsonUtility.ToJson (allRememberData) : string.Empty;
			LinkedInvInstance.SceneItemRememberData = scriptDataAsJson;
		}


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			Remember[] remembers = GetRemembersToRecord ();
			if (remembers.Length > 0)
			{
				string rememberMessage = "The following Remember component data will be stored in the linked Item instance:";
				foreach (Remember remember in remembers)
				{
					rememberMessage += "\n -" + remember.GetType ().Name;
				}
				EditorGUILayout.HelpBox (rememberMessage, MessageType.Info);
			}
		}

		#endif

		#endregion


		#region PrivateFunctions

		private Remember[] GetRemembersToRecord ()
		{
			Remember[] remembers = gameObject.GetComponents<Remember> ();
			List<Remember> filteredList = new List<Remember> ();
			foreach (Remember remember in remembers)
			{
				if (remember is RememberSceneItem) continue;
				filteredList.Add (remember);
			}
			return filteredList.ToArray ();
		}

		#endregion


		#region GetSet

		/** The InvInstance that this object represents */
		public InvInstance LinkedInvInstance { get { return linkedInvInstance; } }

		/** The InvItem that this object represents */
		public InvItem LinkedInvItem { get { return InvInstance.IsValid (LinkedInvInstance) ? LinkedInvInstance.InvItem : null; } }

		#endregion


		#region PrivateClasses

		[Serializable]
		public class AllRememberData
		{

			public List<ScriptData> allScriptData;

			public AllRememberData (List<ScriptData> _allScriptData)
			{
				allScriptData = _allScriptData;
			}

		}

		#endregion

	}

}