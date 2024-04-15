#if UNITY_EDITOR

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberSceneItem), true)]
	public class RememberSceneItemEditor : ConstantIDEditor
	{

		public override void OnInspectorGUI ()
		{
			RememberSceneItem _target = (RememberSceneItem) target;
			_target.ShowGUI ();
			SharedGUI ();
		}

	}

}

#endif