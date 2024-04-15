using UnityEngine;

namespace AC
{

	public class EventContainerOpenClose : EventBase
	{

		[SerializeField] private OpenClose openClose;
		[SerializeField] private Container container = null;
		public enum OpenClose { Open, Close };


		public override string[] EditorNames { get { return new string[] { "Container/Open", "Container/Close" }; } }
		protected override string EventName { get { return openClose == OpenClose.Open ? "OnContainerOpen" : "OnContainerClose"; } }
		protected override string ConditionHelp { get { return "Whenever " + ((container) ? "container '" + container.gameObject.name + "'" : "a Container") + " is " + ((openClose == OpenClose.Open) ? "opened." : "closed."); } }


		public EventContainerOpenClose (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, OpenClose _openClose, Container _container)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			openClose = _openClose;
			container = _container;
		}


		public EventContainerOpenClose () {}


		public override void Register ()
		{
			EventManager.OnContainerOpen += OnContainerOpen;
			EventManager.OnContainerClose += OnContainerClose;
		}


		public override void Unregister ()
		{
			EventManager.OnContainerOpen -= OnContainerOpen;
			EventManager.OnContainerClose -= OnContainerClose;
		}


		private void OnContainerOpen (Container _container)
		{
			if (openClose == OpenClose.Open && (container == null || container == _container))
			{
				Run (new object[] { _container.gameObject });
			}
		}


		private void OnContainerClose (Container _container)
		{
			if (openClose == OpenClose.Close && (container == null || container == _container))
			{
				Run (new object[] { _container.gameObject });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.GameObject, "Container")
			};
		}


#if UNITY_EDITOR

		public override void AssignVariant (int variantIndex)
		{
			openClose = (OpenClose) variantIndex;
		}


		protected override bool HasConditions (bool isAssetFile) { return !isAssetFile; }


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