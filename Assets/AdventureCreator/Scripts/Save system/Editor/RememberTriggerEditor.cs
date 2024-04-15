#if UNITY_EDITOR

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberTrigger), true)]
	public class RememberTriggerEditor : ConstantIDEditor
	{
		
		public override void OnInspectorGUI ()
		{
			RememberTrigger _target = (RememberTrigger) target;
			_target.ShowGUI ();
			SharedGUI ();
		}
		
	}

}

#endif