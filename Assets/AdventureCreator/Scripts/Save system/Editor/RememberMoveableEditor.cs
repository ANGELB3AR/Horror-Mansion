#if UNITY_EDITOR

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberMoveable), true)]
	public class RememberMoveableEditor : ConstantIDEditor
	{
		
		public override void OnInspectorGUI ()
		{
			RememberMoveable _target = (RememberMoveable) target;
			_target.ShowGUI ();
			SharedGUI ();
		}
		
	}

}

#endif