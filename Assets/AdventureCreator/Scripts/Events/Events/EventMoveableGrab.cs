using UnityEngine;

namespace AC
{

	public class EventMoveableGrab : EventBase
	{

		[SerializeField] private GrabDrop grabDrop;
		[SerializeField] private DragBase draggable = null;
		public enum GrabDrop { Grabbed, Dropped };


		public override string[] EditorNames { get { return new string[] { "Draggable/Grab", "Draggable/Drop" }; } }
		protected override string EventName { get { return grabDrop == GrabDrop.Grabbed ? "OnGrabMoveable" : "OnDropMoveable"; } }
		protected override string ConditionHelp { get { return "Whenever a Draggable is " + grabDrop.ToString ().ToLower () + "."; } }


		public EventMoveableGrab (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, GrabDrop _grabDrop, DragBase _draggable)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			grabDrop = _grabDrop;
			draggable = _draggable;
		}


		public EventMoveableGrab () {}


		public override void Register ()
		{
			EventManager.OnGrabMoveable += OnGrabMoveable;
			EventManager.OnDropMoveable += OnDropMoveable;
		}


		public override void Unregister ()
		{
			EventManager.OnGrabMoveable -= OnGrabMoveable;
			EventManager.OnDropMoveable -= OnDropMoveable;
		}


		private void OnDropMoveable (DragBase dragBase)
		{
			if (grabDrop == GrabDrop.Dropped && (draggable == null || dragBase == draggable))
			{
				Run (new object[] { dragBase });
			}
		}


		private void OnGrabMoveable (DragBase dragBase)
		{
			if (grabDrop == GrabDrop.Grabbed && (draggable == null || dragBase == draggable))
			{
				Run (new object[] { dragBase });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.GameObject, "Draggable")
			};
		}


#if UNITY_EDITOR
		
		protected override bool HasConditions (bool isAssetFile) { return !isAssetFile; }


		protected override void ShowConditionGUI (bool isAssetFile)
		{
			if (!isAssetFile)
			{
				draggable = (DragBase) CustomGUILayout.ObjectField<DragBase> ("Draggable:", draggable, true);
			}
		}

		public override void AssignVariant (int variantIndex)
		{
			grabDrop = (variantIndex == 0) ? GrabDrop.Grabbed : GrabDrop.Dropped;
		}

#endif

	}

}