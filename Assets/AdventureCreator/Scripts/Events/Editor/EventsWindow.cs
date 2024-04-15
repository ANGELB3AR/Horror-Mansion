#if UNITY_2019_4_OR_NEWER && UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace AC
{

	public class EventsWindow : EditorWindow
	{

		private EventsEditorData data;
		private SettingsManager settingsManager;


		[MenuItem ("Adventure Creator/Editors/Events Editor", false, 5)]
		public static void Init ()
		{
			EventsWindow window = (EventsWindow) GetWindow (typeof (EventsWindow));
			window.titleContent.text = "Events";
			window.position = new Rect (300, 200, 450, 490);
			window.minSize = new Vector2 (300, 180);
		}


		private void OnGUI ()
		{
			settingsManager = KickStarter.settingsManager;
			if (settingsManager == null)
			{
				EditorGUILayout.HelpBox ("A Settings Manager must be assigned before this window can display correctly.", MessageType.Warning);
				return;
			}

			EditorGUILayout.LabelField ("Global events", CustomStyles.managerHeader);

			if (data == null) data = new EventsEditorData ();
			data.ShowGUI (KickStarter.settingsManager.events, KickStarter.settingsManager);
		}

	}

}

#endif