#if UNITY_2019_4_OR_NEWER

/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"EventRunner.cs"
 * 
 *	A component that allows ActionLists to run when events are invoked.
 * 
 */

using System.Collections.Generic;
using UnityEngine;

namespace AC
{

	/** A component that allows ActionLists to run when events are invoked. */
	[AddComponentMenu ("Adventure Creator/Logic/Event runner")]
	[HelpURL ("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_event_runner.html")]
	public class EventRunner : MonoBehaviour
    {

		#region Variables

		/** A list of ActionLists that run when common events are fired */
		[SerializeReference] public List<EventBase> events = new List<EventBase> ();

		#endregion


		#region UnityStandards

		private void OnEnable ()
		{
			if (KickStarter.settingsManager)
			{
				foreach (EventBase _event in events)
				{
					_event.Register ();
				}
			}
		}


		private void OnDisable ()
		{
			if (KickStarter.settingsManager)
			{
				foreach (EventBase _event in events)
				{
					_event.Unregister ();
				}
			}
		}

		#endregion


#if UNITY_EDITOR

		private EventsEditorData data;

		public void ShowGUI ()
		{
			if (data == null) data = new EventsEditorData ();
			data.ShowGUI (events, this);
		}

#endif

    }

}

#endif