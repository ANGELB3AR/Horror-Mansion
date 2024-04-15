using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
#endif

namespace AC
{

	[RequireComponent (typeof (SceneItem))]
	public class RememberSceneItem : Remember
	{

		#region Variables

		public int defaultLinkedItemID;
		private bool isLoading;

		#endregion


		#region CustomEvents

		protected override void OnInitialiseScene ()
		{
			if (isActiveAndEnabled)
			{
				SceneItem sceneItem = GetComponent<SceneItem> ();
				sceneItem.AssignLinkedInvInstance (new InvInstance (defaultLinkedItemID));
			}
		}

		#endregion


		#region PublicFunctions

		public override string SaveData ()
		{
			SceneItem sceneItem = GetComponent<SceneItem> ();

			SceneItemData data = new SceneItemData ();
			data.objectID = constantID;
			data.savePrevented = savePrevented;

			data.invInstanceData = InvInstance.GetSaveData (sceneItem.LinkedInvInstance);
			data.sourceType = 0;
			data.sourceID = 0;

			if (InvInstance.IsValid (sceneItem.LinkedInvInstance))
			{
				InvCollection sourceCollection = sceneItem.LinkedInvInstance.GetSource ();

				if (sourceCollection != null)
				{
					data.sourceCollectionIndex = sourceCollection.GetInstanceIndex (sceneItem.LinkedInvInstance);

					Container sourceContainer = sceneItem.LinkedInvInstance.GetSourceContainer ();
					if (sourceContainer)
					{
						ConstantID sourceContainerID = sourceContainer.GetComponent<ConstantID> ();
						if (sourceContainerID)
						{
							data.sourceType = (int) ActionInventoryPrefab.ItemSource.Container;
							data.sourceID = sourceContainerID.constantID;
						}
						else
						{
							ACDebug.LogWarning ("Cannot save source data of Scene Item " + name + " as the Container " + sourceContainer + " has no Constant ID", sourceContainer);
						}
					}
					else if (sourceCollection == KickStarter.runtimeInventory.PlayerInvCollection)
					{
						data.sourceType = (int) ActionInventoryPrefab.ItemSource.PlayerInventory;
					}
				}
			}

			return Serializer.SaveScriptData<SceneItemData> (data);
		}


		public override IEnumerator LoadDataCo (string stringData)
		{
			SceneItemData data = Serializer.LoadScriptData<SceneItemData> (stringData);
			if (data == null) yield break;
			SavePrevented = data.savePrevented; if (savePrevented) yield break;

			SceneItem sceneItem = GetComponent<SceneItem> ();

			InvInstance invInstance = null;
			ActionInventoryPrefab.ItemSource itemSource = (ActionInventoryPrefab.ItemSource) data.sourceType;

			switch (itemSource)
			{
				case ActionInventoryPrefab.ItemSource.Container:
					Container container = GetComponent<Container> (data.sourceID);
					if (container)
					{
						invInstance = container.InvCollection.GetInstanceAtIndex (data.sourceCollectionIndex);
					}
					break;

				case ActionInventoryPrefab.ItemSource.PlayerInventory:
					invInstance = KickStarter.runtimeInventory.PlayerInvCollection.GetInstanceAtIndex (data.sourceCollectionIndex);
					break;
			}

			if (!InvInstance.IsValid (invInstance))
			{
				invInstance = InvInstance.LoadData (data.invInstanceData);
			}

			isLoading = true;
			if (InvInstance.IsValid (invInstance))
			{
				var assignLinkedInvInstanceCoroutine = sceneItem.AssignLinkedInvInstanceCo (invInstance, true, OnCompleteLoad);
				while (assignLinkedInvInstanceCoroutine.MoveNext ())
				{
					yield return assignLinkedInvInstanceCoroutine.Current;
				}

				while (isLoading)
				{
					yield return null;
				}
			}
		}


		public SceneItemSpawnData SaveSpawnData ()
		{
			SceneItem sceneItem = GetComponent<SceneItem> ();
			SceneItemSpawnData data = new SceneItemSpawnData (sceneItem, constantID);
			return data;
		}


		public void LoadSpawnData (SceneItemSpawnData data)
		{
			if (data.objectID != 0)
			{
				ConstantID[] idScripts = GetComponents<ConstantID> ();
				foreach (ConstantID idScript in idScripts)
				{
					idScript.constantID = data.objectID;
				}
			}
		}


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			CustomGUILayout.Header ("Scene item");
			CustomGUILayout.BeginVertical ();

			if (Application.isPlaying && gameObject.activeInHierarchy)
			{
				InvInstance linkedInvInstance = GetComponent<SceneItem> ().LinkedInvInstance;

				if (InvInstance.IsValid (linkedInvInstance))
				{
					EditorGUILayout.LabelField ("Linked item: " + linkedInvInstance.InvItem.label);

					Container container = linkedInvInstance.GetSourceContainer ();
					if (container)
					{
						EditorGUILayout.LabelField ("Item source: " + container.name);
					}
					else if (linkedInvInstance.GetSource () == KickStarter.runtimeInventory.PlayerInvCollection)
					{
						EditorGUILayout.LabelField ("Item source: Player inventory");
					}
				}
			}
			else if (KickStarter.inventoryManager && KickStarter.inventoryManager.items.Count > 0)
			{
				// Create a string List of the field's names (for the PopUp box)
				List<string> labelList = new List<string> ();

				int i = 0;
				int invNumber = -1;

				foreach (InvItem _item in KickStarter.inventoryManager.items)
				{
					labelList.Add (_item.label);

					// If an item has been removed, make sure selected variable is still valid
					if (_item.id == defaultLinkedItemID)
					{
						invNumber = i;
					}

					i++;
				}

				if (invNumber >= 0)
				{
					invNumber = EditorGUILayout.Popup ("Default Inventory item:", invNumber, labelList.ToArray ());
					defaultLinkedItemID = KickStarter.inventoryManager.items[invNumber].id;
				}
				else
				{
					defaultLinkedItemID = EditorGUILayout.IntField ("Linked item ID:", defaultLinkedItemID);
				}
			}
			else
			{
				defaultLinkedItemID = EditorGUILayout.IntField ("Linked item ID:", defaultLinkedItemID);
			}

			CustomGUILayout.EndVertical ();
		}

		#endif

		#endregion


		#region PrivateFunctions

		private void OnCompleteLoad ()
		{
			isLoading = false;
		}

		#endregion


		#region GetSet

		public override int LoadOrder { get { return 10; } }

		#endregion

	}


	[Serializable]
	public class SceneItemSpawnData
	{

		public int itemID;
		public int objectID;

		public SceneItemSpawnData ()
		{
			itemID = -1;
			objectID = 0;
		}


		public SceneItemSpawnData (SceneItem sceneItem, int _objectID)
		{
			if (sceneItem == null || !InvInstance.IsValid (sceneItem.LinkedInvInstance))
			{
				itemID = -1;
				objectID = 0;
				return;
			}

			sceneItem.SaveRememberDataToLinkedInstance ();
			itemID = sceneItem.LinkedInvInstance.ItemID;
			objectID = _objectID;
		}

	}


	[Serializable]
	public class SceneItemData : RememberData
	{

		public string invInstanceData;
		
		public int sourceCollectionIndex;
		public int sourceType;
		public int sourceID;
		

		public SceneItemData ()	{}

	}

}