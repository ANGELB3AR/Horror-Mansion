using UnityEngine;

namespace AC
{

	public class EventContainerAddRemove : EventBase
	{

		[SerializeField] private Container container = null;
		[SerializeField] private AddRemove addRemove;
		public enum AddRemove { Add, Remove };


		public override string[] EditorNames { get { return new string[] { "Container/Add", "Container/Remove" }; } }
		protected override string EventName { get { return addRemove == AddRemove.Add ? "OnContainerAdd" : "OnContainerRemove"; } }
		protected override string ConditionHelp { get { return "Whenever an item is " + ((addRemove == AddRemove.Add) ? "added to " : "removed from ") + (container ? "container '" + container.name + "'." : "a Container."); } }


		public EventContainerAddRemove (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, Container _container, AddRemove _addRemove)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			container = _container;
			addRemove = _addRemove;
		}


		public EventContainerAddRemove () {}


		public override void Register ()
		{
			EventManager.OnContainerAdd += OnContainerAdd;
			EventManager.OnContainerRemove += OnContainerRemove;
		}


		public override void Unregister ()
		{
			EventManager.OnContainerAdd -= OnContainerAdd;
			EventManager.OnContainerRemove -= OnContainerRemove;
		}


		private void OnContainerAdd (Container _container, InvInstance containerItem)
		{
			if (addRemove == AddRemove.Add && (container == null || container == _container))
			{
				Run (new object[] { _container.gameObject, containerItem.ItemID });
			}
		}



		private void OnContainerRemove (Container _container, InvInstance containerItem)
		{
			if (addRemove == AddRemove.Remove && (container == null || container == _container))
			{
				Run (new object[] { _container.gameObject, containerItem.ItemID });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.GameObject, "Container"),
				new ParameterReference (ParameterType.InventoryItem, "Inventory item"),
			};
		}


#if UNITY_EDITOR

		public override void AssignVariant (int variantIndex)
		{
			addRemove = (variantIndex == 0) ? AddRemove.Add : AddRemove.Remove;
		}


		protected override bool HasConditions (bool isAssetFile)
		{
			return !isAssetFile;
		}


		protected override void ShowConditionGUI (bool isAssetFile)
		{
			if (!isAssetFile)
			{
				container = (Container) CustomGUILayout.ObjectField<Container> ("Container:", container, true);
			}
		}

#endif

	}

}