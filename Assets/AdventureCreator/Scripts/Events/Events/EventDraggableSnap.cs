using UnityEngine;

namespace AC
{

	public class EventDraggableSnap : EventBase
	{

		[SerializeField] private DragBase draggable = null;
		[SerializeField] private DragTrack dragTrack = null;


		public override string[] EditorNames { get { return new string[] { "Draggable/Snap" }; } }
		protected override string EventName { get { return "OnDraggableSnap"; } }
		protected override string ConditionHelp { get { return "Whenever a Draggable snaps to a track region"; } }


		public EventDraggableSnap (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, DragBase _draggable, DragTrack _dragTrack)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			draggable = _draggable;
			dragTrack = _dragTrack;
		}


		public EventDraggableSnap () {}


		public override void Register ()
		{
			EventManager.OnDraggableSnap += OnDraggableSnap;
		}


		public override void Unregister ()
		{
			EventManager.OnDraggableSnap -= OnDraggableSnap;
		}


		private void OnDraggableSnap (DragBase dragBase, DragTrack track, TrackSnapData trackSnapData)
		{
			if ((draggable == null || dragBase == draggable) && (track == null || track == dragTrack))
			{
				Run (new object[] { dragBase, dragTrack, trackSnapData.ID });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.GameObject, "Draggable"),
				new ParameterReference (ParameterType.GameObject, "Track"),
				new ParameterReference (ParameterType.Integer, "Region ID"),
			};
		}


#if UNITY_EDITOR
		
		protected override bool HasConditions (bool isAssetFile) { return !isAssetFile; }


		protected override void ShowConditionGUI (bool isAssetFile)
		{
			if (!isAssetFile)
			{
				dragTrack = (DragTrack) CustomGUILayout.ObjectField<DragTrack> ("Track:", dragTrack, true);
				draggable = (DragBase) CustomGUILayout.ObjectField<DragBase> ("Draggable:", draggable, true);
			}
		}

#endif

	}

}