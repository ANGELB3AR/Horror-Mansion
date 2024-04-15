using UnityEngine;

namespace AC
{

	public class EventInventoryCombine : EventBase
	{

		[SerializeField] private int itemID = -1;


		public override string[] EditorNames { get { return new string[] { "Inventory/Combine" }; } }
		protected override string EventName { get { return "OnInventoryCombine"; } }
		protected override string ConditionHelp { get { return "Whenever " + ((itemID >= 0) ? GetItemName () : "an Inventory item") + " is combined with another."; } }


		public EventInventoryCombine (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, int _itemID)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			itemID = _itemID;
		}


		public EventInventoryCombine () {}


		public override void Register ()
		{
			EventManager.OnInventoryCombine_Alt += OnInventoryCombine;
		}

		
		public override void Unregister ()
		{
			EventManager.OnInventoryCombine_Alt -= OnInventoryCombine;
		}


		private void OnInventoryCombine (InvInstance invInstanceA, InvInstance invInstanceB)
		{
			if (itemID < 0 || itemID == invInstanceA.ItemID)
			{
				Run (new object[] { invInstanceA.ItemID, invInstanceB.ItemID });
			}
		}

		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.InventoryItem, "Inventory item A"),
				new ParameterReference (ParameterType.Integer, "Inventory item B"),
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