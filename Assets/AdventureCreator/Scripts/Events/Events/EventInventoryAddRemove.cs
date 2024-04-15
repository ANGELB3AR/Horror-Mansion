using UnityEngine;

namespace AC
{

	public class EventInventoryAddRemove : EventBase
	{

		[SerializeField] private AddRemove addRemove;
		public enum AddRemove { Add, Remove };
		[SerializeField] private int itemID = -1;


		public override string[] EditorNames { get { return new string[] { "Inventory/Add", "Inventory/Remove" }; } }
		protected override string EventName { get { return addRemove == AddRemove.Add ? "OnInventoryAdd" : "OnInventoryRemove"; } }
		protected override string ConditionHelp { get { return "Whenever " + ((itemID >= 0) ? GetItemName () : "an Inventory item") + " is " + ((addRemove == AddRemove.Add) ? "added to the Player's inventory." : "removed from the Player's Inventory."); } }


		public EventInventoryAddRemove (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, AddRemove _addRemove, int _itemID = -1)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			addRemove = _addRemove;
			itemID = _itemID;
		}


		public EventInventoryAddRemove () {}


		public override void Register ()
		{
			EventManager.OnInventoryAdd_Alt += OnInventoryAdd;
			EventManager.OnInventoryRemove_Alt += OnInventoryRemove;
		}


		public override void Unregister ()
		{
			EventManager.OnInventoryAdd_Alt -= OnInventoryAdd;
			EventManager.OnInventoryRemove_Alt -= OnInventoryRemove;
		}


		private void OnInventoryAdd (InvCollection invCollection, InvInstance invInstance, int amount)
		{
			if (!KickStarter.kickStarter.HasInitialisedAC || KickStarter.saveSystem.IsInitialisingAfterLoad) return;
			if (invCollection != KickStarter.runtimeInventory.PlayerInvCollection) return;
			if (addRemove == AddRemove.Add && (itemID < 0 || itemID == invInstance.ItemID))
			{
				Run (new object[] { invInstance.ItemID, amount });
			}
		}

		
		private void OnInventoryRemove (InvCollection invCollection, InvInstance invInstance, int amount)
		{
			if (invCollection != KickStarter.runtimeInventory.PlayerInvCollection) return;
			if (addRemove == AddRemove.Remove && (itemID < 0 || itemID == invInstance.ItemID))
			{
				Run (new object[] { invInstance.ItemID, amount });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.InventoryItem, "Inventory item"),
				new ParameterReference (ParameterType.Integer, "Amount")
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

		public override void AssignVariant (int variantIndex)
		{
			addRemove = (variantIndex == 0) ? AddRemove.Add : AddRemove.Remove;
		}


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