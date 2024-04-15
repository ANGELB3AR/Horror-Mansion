using UnityEngine;

namespace AC
{

	public class EventCameraShake : EventBase
	{

		[SerializeField] private _Camera camera = null;


		public override string[] EditorNames { get { return new string[] { "Camera/Shake" }; } }
		protected override string EventName { get { return "OnShakeCamera"; } }
		protected override string ConditionHelp { get { return "Whenever " + (camera ? "camera '" + camera.name + "'" : "the active camera") + " is shaken."; } }


		public EventCameraShake (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, _Camera _camera)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			camera = _camera;
		}


		public EventCameraShake () {}


		public override void Register ()
		{
			EventManager.OnShakeCamera += OnShakeCamera;
		}
		

		public override void Unregister ()
		{
			EventManager.OnShakeCamera -= OnShakeCamera;
		}


		private void OnShakeCamera (float intensity, float duration)
		{
			if (camera == null || KickStarter.mainCamera.attachedCamera == camera)
			{
				Run (new object[] { duration });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.Float, "Duration"),
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