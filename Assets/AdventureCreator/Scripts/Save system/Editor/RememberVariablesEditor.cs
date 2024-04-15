#if UNITY_EDITOR

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberVariables), true)]
	public class RememberVariablesEditor : ConstantIDEditor
	{

		public override void OnInspectorGUI ()
		{
			RememberVariables _target = (RememberVariables) target;
			_target.ShowGUI ();
			SharedGUI ();
		}

	}

}

#endif