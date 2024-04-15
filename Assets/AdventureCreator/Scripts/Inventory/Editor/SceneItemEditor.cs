#if UNITY_EDITOR

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (SceneItem), true)]
	public class SceneItemEditor : Editor
	{

		public override void OnInspectorGUI ()
		{
			SceneItem _target = (SceneItem) target;
			_target.ShowGUI ();
		}

	}

}

#endif