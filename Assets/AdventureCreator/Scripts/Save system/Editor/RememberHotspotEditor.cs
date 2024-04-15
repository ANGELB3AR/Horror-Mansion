#if UNITY_EDITOR

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberHotspot), true)]
	public class RememberHotspotEditor : ConstantIDEditor
	{
		
		public override void OnInspectorGUI()
		{
			RememberHotspot _target = (RememberHotspot) target;
			_target.ShowGUI ();
			SharedGUI ();
		}

	}

}

#endif