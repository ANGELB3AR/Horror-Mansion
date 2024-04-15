using UnityEngine;

namespace AC
{

	public class EventInventorySpawn : EventBase
	{

		[SerializeField] private int itemID = -1;


		public override string[] EditorNames { get { return new string[] { "Inventory/Spawn" }; } }
		protected override string EventName { get { return "OnInventorySpawn"; } }
		protected override string ConditionHelp { get { return "Whenever " + ((itemID >= 0) ? GetItemName () : "an Inventory item") + " is spawned into the scene."; } }


		public EventInventorySpawn (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, int _itemID)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			itemID = _itemID;
		}


		public EventInventorySpawn () {}


		public override void Register ()
		{
			EventManager.OnInventorySpawn += OnInventorySpawn;
		}


		public override void Unregister ()
		{
			EventManager.OnInventorySpawn -= OnInventorySpawn;
		}


		private void OnInventorySpawn (InvInstance invInstance, SceneItem sceneItem)
		{
			if (itemID < 0 || itemID == invInstance.ItemID)
			{
				Run (new object[] { invInstance.ItemID, sceneItem.gameObject });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.InventoryItem, "Inventory item"),
				new ParameterReference (ParameterType.GameObject, "Spawned object"),
			};
		}


		private string GetItemName ()
		{
			if (KickStarter.inventoryManager)
			{
				InvItem invItem = KickStarter.inventoryManager.GetItem (itemID);
				if (invItem != null) return "item '" + invItem.label + "'";
			}
			return "item " + itemID;
		}


#if UNITY_EDITOR

		protected override bool HasConditions (bool isAssetFile) { return true; }


		protected override void ShowConditionGUI (bool isAssetFile)
		{
			if (KickStarter.inventoryManager)
			{
				itemID = ActionRunActionList.ShowInvItemSelectorGUI ("Item:", KickStarter.inventoryManager.items, itemID);
			}
			else
			{
				itemID = CustomGUILayout.IntField ("Item ID:", itemID);
			}
		}

#endif

	}

}