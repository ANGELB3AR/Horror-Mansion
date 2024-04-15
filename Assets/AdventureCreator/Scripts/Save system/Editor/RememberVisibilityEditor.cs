#if UNITY_EDITOR

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberVisibility), true)]
	public class RememberVisibilityEditor : ConstantIDEditor
	{
		
		public override void OnInspectorGUI()
		{
			RememberVisibility _target = (RememberVisibility) target;
			_target.ShowGUI ();
			SharedGUI ();
		}
		
	}

}

#endif