#if UNITY_2019_4_OR_NEWER && UNITY_EDITOR

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (EventRunner))]
	public class EventRunnerEditor : Editor
	{

		public override void OnInspectorGUI ()
		{
			EventRunner _target = (EventRunner) target;
			_target.ShowGUI ();
		}

	}

}

#endif