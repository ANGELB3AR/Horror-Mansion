#if UNITY_EDITOR

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberConversation), true)]
	public class RememberConversationEditor : ConstantIDEditor
	{
		
		public override void OnInspectorGUI()
		{
			RememberConversation _target = (RememberConversation) target;
			_target.ShowGUI ();
			SharedGUI ();
		}
		
	}

}

#endif