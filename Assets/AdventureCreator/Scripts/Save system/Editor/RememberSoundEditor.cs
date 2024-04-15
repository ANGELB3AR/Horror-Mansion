#if UNITY_EDITOR

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberSound), true)]
	public class RememberSoundEditor : ConstantIDEditor
	{

		public override void OnInspectorGUI ()
		{
			RememberSound _target = (RememberSound) target;
			_target.ShowGUI ();
			SharedGUI ();
		}

	}

}

#endif