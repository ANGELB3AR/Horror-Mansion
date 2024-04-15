using UnityEngine;

namespace AC
{

	public class EventInventorySelect : EventBase
	{

		[SerializeField] private SelectDeselect selectDeselect;
		public enum SelectDeselect { Select, Deselect};
		[SerializeField] private int itemID = -1;


		public override string[] EditorNames { get { return new string[] { "Inventory/Select", "Inventory/Deselect" }; } }
		protected override string EventName { get { return selectDeselect == SelectDeselect.Select ? "OnInventorySelect" : "OnInventoryDeselect"; } }
		protected override string ConditionHelp { get { return "Whenever " + ((itemID >= 0) ? GetItemName () : "an Inventory item") + " is " + ((selectDeselect == SelectDeselect.Select) ? "selected." : "deselected."); } }


		public EventInventorySelect (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, SelectDeselect _selectDeselect, int _itemID)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			selectDeselect = _selectDeselect;
			itemID = _itemID;
		}


		public EventInventorySelect () {}


		public override void Register ()
		{
			EventManager.OnInventorySelect_Alt += OnInventorySelect;
			EventManager.OnInventoryDeselect_Alt += OnInventoryDeselect;
		}


		public override void Unregister ()
		{
			EventManager.OnInventorySelect_Alt -= OnInventorySelect;
			EventManager.OnInventoryDeselect_Alt -= OnInventoryDeselect;
		}


		private void OnInventorySelect (InvCollection invCollection, InvInstance invInstance)
		{
			if (selectDeselect == SelectDeselect.Select && (itemID < 0 || itemID == invInstance.ItemID))
			{
				Run (new object[] { invInstance.ItemID });
			}
		}

		
		private void OnInventoryDeselect (InvCollection invCollection, InvInstance invInstance)
		{
			if (selectDeselect == SelectDeselect.Deselect && (itemID < 0 || itemID == invInstance.ItemID))
			{
				Run (new object[] { invInstance.ItemID });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.InventoryItem, "Inventory item")
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
			selectDeselect = (SelectDeselect) variantIndex;
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