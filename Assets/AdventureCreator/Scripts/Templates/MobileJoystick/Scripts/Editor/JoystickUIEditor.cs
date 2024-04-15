#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace AC.Templates.MobileJoystick
{

	[CustomEditor (typeof (JoystickUI))]
	public class JoystickUIEditor : Editor
	{

		public override void OnInspectorGUI ()
		{
			JoystickUI _target = (JoystickUI) target;
			_target.ShowGUI ();

			if (GUI.changed)
			{
				EditorUtility.SetDirty (_target);
			}
		}

	}

}

#endif