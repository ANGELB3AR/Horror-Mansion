#if UNITY_EDITOR

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberTrack), true)]
	public class RememberTrackEditor : ConstantIDEditor
	{

		public override void OnInspectorGUI ()
		{
			RememberTrack _target = (RememberTrack) target;
			_target.ShowGUI ();
			SharedGUI ();
		}

	}

}

#endif