using UnityEngine;

namespace AC
{

	public class EventInventoryInteract : EventBase
	{

		[SerializeField] private int itemID = -1;


		public override string[] EditorNames { get { return new string[] { "Inventory/Interact" }; } }
		protected override string EventName { get { return "OnInventoryInteract"; } }
		protected override string ConditionHelp { get { return "Whenever " + ((itemID >= 0) ? GetItemName () : "an Inventory item") + " is interacted with."; } }


		public EventInventoryInteract (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, int _itemID)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			itemID = _itemID;
		}


		public EventInventoryInteract () {}


		public override void Register ()
		{
			EventManager.OnInventoryInteract_Alt += OnInventoryInteract_Alt;
		}


		public override void Unregister ()
		{
			EventManager.OnInventoryInteract_Alt -= OnInventoryInteract_Alt;
		}


		private void OnInventoryInteract_Alt (InvInstance invInstance, int iconID)
		{
			if (itemID < 0 || itemID == invInstance.ItemID)
			{
				Run (new object[] { invInstance.ItemID, iconID });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.InventoryItem, "Inventory item"),
				new ParameterReference (ParameterType.Integer, "Icon ID"),
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