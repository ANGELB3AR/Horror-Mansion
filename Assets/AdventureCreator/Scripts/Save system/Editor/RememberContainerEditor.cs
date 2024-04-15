#if UNITY_EDITOR

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberContainer), true)]
	public class RememberContainerEditor : ConstantIDEditor
	{

		public override void OnInspectorGUI ()
		{
			RememberContainer _target = (RememberContainer) target;
			_target.ShowGUI ();
			SharedGUI ();
		}

	}

}

#endif