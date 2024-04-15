using UnityEngine;

namespace AC
{

	public class EventCameraSplitScreen : EventBase
	{

		[SerializeField] private _Camera camera = null;
		[SerializeField] private StartStop startStop;
		public enum StartStop { Start, Stop };


		public override string[] EditorNames { get { return new string[] { "Camera/Split-screen/Start", "Camera/Split-screen/Stop" }; } }
		protected override string EventName { get { return "OnCameraSplitScreen" + startStop.ToString (); } }
		protected override string ConditionHelp { get { return "Whenever " + (camera ? "camera '" + camera.gameObject.name + "'s": "a camera's") + " split-screen effect " + startStop.ToString ().ToLower () + "s."; } }


		public EventCameraSplitScreen (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, _Camera _camera, StartStop _startStop)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			camera = _camera;
			startStop = _startStop;
		}


		public EventCameraSplitScreen () {}
		
		
		public override void Register ()
		{
			EventManager.OnCameraSplitScreenStart += OnCameraSplitScreenStart;
			EventManager.OnCameraSplitScreenStop += OnCameraSplitScreenStop;
		}


		public override void Unregister ()
		{
			EventManager.OnCameraSplitScreenStart -= OnCameraSplitScreenStart;
			EventManager.OnCameraSplitScreenStop -= OnCameraSplitScreenStop;
		}


		private void OnCameraSplitScreenStart (_Camera camera, CameraSplitOrientation splitOrientation, float splitAmountMain, float splitAmountOther, bool isTopLeftSplit)
		{
			if (startStop == StartStop.Start)
			{
				Run (new object[] { camera.gameObject });
			}
		}


		private void OnCameraSplitScreenStop (_Camera camera)
		{
			if (startStop == StartStop.Stop)
			{
				Run (new object[] { camera.gameObject });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.GameObject, "Camera"),
			};
		}


#if UNITY_EDITOR

		public override void AssignVariant (int variantIndex)
		{
			startStop = (StartStop) variantIndex;
		}


		protected override bool HasConditions (bool isAssetFile) { return !isAssetFile; }


		protected override void ShowConditionGUI (bool isAssetFile)
		{
			if (!isAssetFile)
			{
				camera = (_Camera) CustomGUILayout.ObjectField<_Camera> ("Camera:", camera, true);
			}
		}

#endif

	}

}