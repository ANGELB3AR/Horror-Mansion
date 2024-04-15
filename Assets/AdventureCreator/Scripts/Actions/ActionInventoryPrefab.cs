/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionInstantiate.cs"
 * 
 *	This Action spawns prefabs and deletes
 *  objects from the scene
 * 
 */

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionInventoryPrefab : Action, IItemReferencerAction
	{

		public int invID = 0;
		public int invParameterID = -1;

		public int spawnedObjectParameterID = -1;
		protected ActionParameter runtimeSpawnedObjectParameter;

		public int newItemParameterID = -1;
		protected ActionParameter runtimeNewItemParameter;

		public SceneItem sceneItem;
		public int sceneItemConstantID;
		public int sceneItemParameterID = -1;
		private SceneItem runtimeSceneItem;

		public Method method = Method.ItemToScene;
		public enum Method { ItemToScene, SceneToItem };

		public enum ItemSpawnMethod { SpecificItem, LastSelectedItem };
		public ItemSpawnMethod itemSpawnMethod = ItemSpawnMethod.SpecificItem;

		public enum ItemSource { NoSource, PlayerInventory, Container };
		public ItemSource itemSource = ItemSource.NoSource;

		public bool removeOriginal;
		public bool applyRememberData = true;

		public int playerID;
		public int playerIDParameterID = -1;

		public Container container;
		public int containerConstantID = 0;
		public int containerParameterID = -1;
		private Container runtimeContainer;


		public override ActionCategory Category { get { return ActionCategory.Inventory; } }
		public override string Title { get { return "Scene Item"; } }
		public override string Description { get { return "Spawns the prefab associated with a given Inventory item into the scene, or takes an item in the scene and adds it to the Inventory"; } }


		public override void AssignValues (List<ActionParameter> parameters)
		{
			if (method == Method.ItemToScene)
			{
				invID = AssignInvItemID (parameters, invParameterID, invID);
				runtimeSpawnedObjectParameter = GetParameterWithID (parameters, spawnedObjectParameterID);

				if (itemSpawnMethod == ItemSpawnMethod.SpecificItem)
				{
					if (itemSource == ItemSource.PlayerInventory)
					{
						playerID = AssignInteger (parameters, playerIDParameterID, playerID);
					}
					else if (itemSource == ItemSource.Container)
					{
						runtimeContainer = AssignFile (parameters, containerParameterID, containerConstantID, container);
					}
				}
			}
			else if (method == Method.SceneToItem)
			{
				runtimeSceneItem = AssignFile<SceneItem> (parameters, sceneItemParameterID, sceneItemConstantID, sceneItem);
				runtimeNewItemParameter = GetParameterWithID (parameters, newItemParameterID);

				if (itemSource == ItemSource.PlayerInventory)
				{
					playerID = AssignInteger (parameters, playerIDParameterID, playerID);
				}
				else if (itemSource == ItemSource.Container)
				{
					runtimeContainer = AssignFile (parameters, containerParameterID, containerConstantID, container);
				}
			}
		}


		public override float Run ()
		{
			switch (method)
			{
				case Method.ItemToScene:
					return RunItemToScene ();

				case Method.SceneToItem:
					return RunSceneToItem ();

				default:
					return 0f;
			}
		}


		private float RunItemToScene ()
		{
			InvInstance invInstance = null;

			switch (itemSpawnMethod)
			{
				case ItemSpawnMethod.SpecificItem:

					invInstance = GetItemToSpawn ();
					break;

				case ItemSpawnMethod.LastSelectedItem:
					invInstance = KickStarter.runtimeInventory.LastSelectedInstance;
					break;
			}

			if (!InvInstance.IsValid (invInstance))
			{
				return 0f;
			}

			int originalCount = invInstance.Count;
			SceneItem sceneItem = invInstance.SpawnInScene (removeOriginal, applyRememberData);

			if (sceneItem && runtimeSpawnedObjectParameter != null && runtimeSpawnedObjectParameter.parameterType == ParameterType.GameObject)
			{
				runtimeSpawnedObjectParameter.SetValue (sceneItem.gameObject);
			}

			KickStarter.actionListManager.RegisterCutsceneSpawnedObject (sceneItem.gameObject);

			if (invInstance.Count == 0 || (removeOriginal && !invInstance.InvItem.canCarryMultiple) || (removeOriginal && invInstance.InvItem.canCarryMultiple && originalCount == 1))
			{
				InvCollection source = invInstance.GetSource ();
				if (source != null)
				{
					source.Delete (invInstance);
				}
			}

			return 0f;
		}


		private InvInstance GetItemToSpawn ()
		{
			switch (itemSource)
			{
				case ItemSource.NoSource:
					return new InvInstance (invID);

				case ItemSource.PlayerInventory:
					{
						InvCollection invCollection;
						if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow || playerID < 0 || (KickStarter.player && playerID == KickStarter.player.ID))
						{
							invCollection = KickStarter.runtimeInventory.PlayerInvCollection;
						}
						else
						{
							invCollection = KickStarter.saveSystem.GetItemsFromPlayer (playerID);
						}
						
						InvInstance invInstance = invCollection.GetFirstInstance (invID);
						if (!InvInstance.IsValid (invInstance))
						{
							if (playerID >= 0)
							{
								LogWarning ("Item " + invID + " is not held by Player " + playerID);
								return null;
							}
							else
							{
								LogWarning ("Item " + invID + " is not held by the Player");
								return null;
							}
						}
						return invInstance;
					}

				case ItemSource.Container:
					if (runtimeContainer)
					{
						InvInstance invInstance = runtimeContainer.InvCollection.GetFirstInstance (invID);
						if (!InvInstance.IsValid (invInstance))
						{
							LogWarning ("Item " + invID + " is not held by the Container " + runtimeContainer, runtimeContainer);
						}
						return invInstance;
					}
					else
					{
						LogWarning ("No Container found!");
						return null;
					}

				default:
					return null;
			}
		}


		private float RunSceneToItem ()
		{
			if (runtimeSceneItem == null)
			{
				LogWarning ("No Scene Item found!");
				return 0f;
			}

			if (!InvInstance.IsValid (runtimeSceneItem.LinkedInvInstance))
			{
				LogWarning ("Scene Item " + runtimeSceneItem.name + " has no valid InvInstance");
				return 0f;
			}

			runtimeSceneItem.SaveRememberDataToLinkedInstance ();
			InvInstance instanceToAdd = removeOriginal ? runtimeSceneItem.LinkedInvInstance : new InvInstance (runtimeSceneItem.LinkedInvInstance);
			InvInstance addedInstance = null;

			switch (itemSource)
			{
				case ItemSource.NoSource:
					addedInstance = instanceToAdd;
					break;

				case ItemSource.Container:
					if (runtimeContainer)
					{
						addedInstance = runtimeContainer.InvCollection.Add (instanceToAdd);
					}
					else
					{
						LogWarning ("No Container found!");
					}
					break;

				case ItemSource.PlayerInventory:
					InvCollection invCollection;
					if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow || playerID < 0 || (KickStarter.player && playerID == KickStarter.player.ID))
					{
						invCollection = KickStarter.runtimeInventory.PlayerInvCollection;
					}
					else
					{
						invCollection = KickStarter.saveSystem.GetItemsFromPlayer (playerID);
					}
					addedInstance = invCollection.Add (instanceToAdd);
					break;

				default:
					break;
			}

			if (InvInstance.IsValid (addedInstance) && runtimeNewItemParameter != null && runtimeNewItemParameter.parameterType == ParameterType.InventoryItem)
			{
				runtimeNewItemParameter.SetValue (addedInstance.InvItem.id);
			}

			if (removeOriginal)
			{
				KickStarter.sceneChanger.ScheduleForDeletion (runtimeSceneItem.gameObject);
			}
			return 0f;
		}


#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			method = (Method) EditorGUILayout.EnumPopup ("Method:", method);

			switch (method)
			{
				case Method.ItemToScene:
					ShowGUI_ItemToScene (parameters);
					return;

				case Method.SceneToItem:
					ShowGUI_SceneToItem (parameters);
					return;

				default:
					return;
			}
		}


		private void ShowGUI_ItemToScene (List<ActionParameter> parameters)
		{
			InventoryManager inventoryManager = KickStarter.inventoryManager;

			itemSpawnMethod = (ItemSpawnMethod) EditorGUILayout.EnumPopup ("Item type:", itemSpawnMethod);

			if (itemSpawnMethod == ItemSpawnMethod.SpecificItem)
			{
				if (inventoryManager)
				{
					ItemField ("Item to spawn:", ref invID, parameters, ref invParameterID, "Item to spawn ID:");

					itemSource = (ItemSource) EditorGUILayout.EnumPopup ("Item source:", itemSource);
					switch (itemSource)
					{
						case ItemSource.PlayerInventory:
							PlayerField (ref playerID, parameters, ref playerIDParameterID);
							break;

						case ItemSource.Container:
							ComponentField ("Container:", ref container, ref containerConstantID, parameters, ref containerParameterID);
							break;
					}
				}
				else
				{
					EditorGUILayout.HelpBox ("An Inventory Manager must be assigned for this Action to display", MessageType.Warning);
				}
			}

			if (itemSpawnMethod == ItemSpawnMethod.LastSelectedItem || itemSource != ItemSource.NoSource)
			{
				removeOriginal = EditorGUILayout.Toggle ("Remove original?", removeOriginal);
				applyRememberData = EditorGUILayout.Toggle ("Apply Remember data?", applyRememberData);
			}

			spawnedObjectParameterID = ChooseParameterGUI ("Send to parameter:", parameters, spawnedObjectParameterID, ParameterType.GameObject);
		}


		private void ShowGUI_SceneToItem (List<ActionParameter> parameters)
		{
			ComponentField ("Scene item:", ref sceneItem, ref sceneItemConstantID, parameters, ref sceneItemParameterID);

			itemSource = (ItemSource) EditorGUILayout.EnumPopup ("New item location:", itemSource);
			switch (itemSource)
			{
				case ItemSource.PlayerInventory:
					PlayerField (ref playerID, parameters, ref playerIDParameterID);
					break;

				case ItemSource.Container:
					ComponentField ("Container:", ref container, ref containerConstantID, parameters, ref containerParameterID);
					break;
			}

			removeOriginal = EditorGUILayout.Toggle ("Remove original?", removeOriginal);
			newItemParameterID = ChooseParameterGUI ("Send to parameter:", parameters, newItemParameterID, ParameterType.InventoryItem);
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (method == Method.ItemToScene)
			{
				if (itemSpawnMethod == ItemSpawnMethod.SpecificItem && invParameterID < 0 && saveScriptsToo)
				{
					InvItem invItem = KickStarter.inventoryManager.GetItem (invID);
					if (invItem != null && invItem.linkedPrefab)
					{
						if (invItem.linkedPrefab.GetComponent<SceneItem> () == null) invItem.linkedPrefab.AddComponent<SceneItem> ();
						if (invItem.linkedPrefab.GetComponent<RememberSceneItem> () == null)
						{
							RememberSceneItem rememberSceneItem = invItem.linkedPrefab.AddComponent<RememberSceneItem> ();
							rememberSceneItem.defaultLinkedItemID = invID;
						}
					}
				}
			}
			else if (method == Method.SceneToItem)
			{
				sceneItemConstantID = AssignConstantID (sceneItem, sceneItemConstantID, sceneItemParameterID);

			}

			if (itemSource == ItemSource.Container)
			{
				if (saveScriptsToo)
				{
					AddSaveScript<RememberContainer> (container);
				}
				containerConstantID = AssignConstantID<Container> (container, containerConstantID, containerParameterID);
			}
		}


		public int GetNumItemReferences (int _itemID, List<ActionParameter> parameters)
		{
			int numFound = 0;

			if (method == Method.ItemToScene)
			{
				if (itemSpawnMethod == ItemSpawnMethod.SpecificItem && invParameterID < 0 && invID == _itemID)
				{
					numFound++;
				}
			}

			return numFound;
		}


		public int UpdateItemReferences (int oldItemID, int newItemID, List<ActionParameter> parameters)
		{
			int numFound = 0;

			if (method == Method.ItemToScene)
			{
				if (itemSpawnMethod == ItemSpawnMethod.SpecificItem && invParameterID < 0 && invID == oldItemID)
				{
					invID = newItemID;
					numFound++;
				}
			}

			return numFound;
		}


		public override bool ReferencesPlayer (int _playerID = -1)
		{
			if (method == Method.SceneToItem || (itemSpawnMethod == ItemSpawnMethod.SpecificItem && itemSource == ItemSource.PlayerInventory))
			{
				if (_playerID < 0 || playerIDParameterID >= 0) return false;
				return (playerID == _playerID);
			}
			return false;
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Inventory: Spawn linked prefab' Action, where an item is spawned in the scene.</summary>
		 * <param name = "invID">The ID of the inventory item to spawn</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionInventoryPrefab CreateNew_ItemToScene (int invID)
		{
			ActionInventoryPrefab newAction = CreateNew<ActionInventoryPrefab> ();
			newAction.method = Method.ItemToScene;
			newAction.invID = invID;

			return newAction;
		}

	}

}