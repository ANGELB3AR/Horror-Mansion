#if UNITY_EDITOR

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberTimeline), true)]
	public class RememberTimelineEditor : ConstantIDEditor
	{
		
		public override void OnInspectorGUI ()
		{
			RememberTimeline _target = (RememberTimeline) target;
			_target.ShowGUI ();
			SharedGUI ();
		}
		
	}

}

#endif