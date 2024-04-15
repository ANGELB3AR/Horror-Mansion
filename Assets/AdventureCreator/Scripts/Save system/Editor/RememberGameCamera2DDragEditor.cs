#if UNITY_EDITOR

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberGameCamera2DDrag), true)]
	public class RememberGameCamera2DDragEditor : ConstantIDEditor
	{

		public override void OnInspectorGUI ()
		{
			RememberGameCamera2DDrag _target = (RememberGameCamera2DDrag) target;
			_target.ShowGUI ();
			SharedGUI ();
		}

	}

}

#endif