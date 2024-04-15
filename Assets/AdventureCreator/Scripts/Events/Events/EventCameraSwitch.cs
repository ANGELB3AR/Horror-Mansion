using UnityEngine;

namespace AC
{

	public class EventCameraSwitch : EventBase
	{

		[SerializeField] private _Camera camera = null;


		public override string[] EditorNames { get { return new string[] { "Camera/Switch" }; } }
		protected override string EventName { get { return "OnSwitchCamera"; } }
		protected override string ConditionHelp { get { return "Whenever the active camera is changed" + (camera ? " to '" + camera.name + "'." : "."); } }


		public EventCameraSwitch (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, _Camera _camera)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			camera = _camera;
		}


		public EventCameraSwitch () {}
		
		
		public override void Register ()
		{
			EventManager.OnSwitchCamera += OnSwitchCamera;
		}


		public override void Unregister ()
		{
			EventManager.OnSwitchCamera -= OnSwitchCamera;
		}


		private void OnSwitchCamera (_Camera fromCamera, _Camera toCamera, float transitionTime)
		{
			if (camera == null || toCamera == camera)
			{
				Run (new object[] { toCamera.gameObject, transitionTime });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.GameObject, "Camera"),
				new ParameterReference (ParameterType.Float, "Transition time"),
			};
		}

#if UNITY_EDITOR

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