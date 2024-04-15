/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"RememberTrigger.cs"
 * 
 *	This script is attached to Trigger objects in the scene
 *	whose on/off state we wish to save. 
 * 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/** Attach this script to Trigger objects in the scene whose on/off state you wish to save. */
	[AddComponentMenu("Adventure Creator/Save system/Remember Trigger")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_trigger.html")]
	public class RememberTrigger : Remember
	{

		#region Variables

		/** Whether the Trigger should be enabled or not when the game begins */
		public AC_OnOff startState = AC_OnOff.On;
		[SerializeField] private AC_Trigger triggerToSave = null;
		
		#endregion


		#region CustomEvents

		protected override void OnInitialiseScene ()
		{
			if (isActiveAndEnabled && Trigger)
			{
				if (startState == AC_OnOff.On)
				{
					Trigger.TurnOn ();
				}
				else
				{
					Trigger.TurnOff ();
				}
			}
		}

		#endregion


		#region PublicFunctions

		public override string SaveData ()
		{
			if (Trigger == null) return string.Empty;

			TriggerData triggerData = new TriggerData ();
			triggerData.objectID = constantID;
			triggerData.savePrevented = savePrevented;

			Collider _collider = Trigger.GetComponent <Collider>();
			if (_collider)
			{
				triggerData.isOn = _collider.enabled;
			}
			else
			{
				Collider2D _collider2D = Trigger.GetComponent <Collider2D>();
				if (_collider2D)
				{
					triggerData.isOn = _collider2D.enabled;
				}
				else
				{
					triggerData.isOn = false;
				}
			}

			return Serializer.SaveScriptData <TriggerData> (triggerData);
		}
		

		public override void LoadData (string stringData)
		{
			if (Trigger == null) return;

			TriggerData data = Serializer.LoadScriptData <TriggerData> (stringData);
			if (data == null)
			{
				return;
			}
			SavePrevented = data.savePrevented; if (savePrevented) return;

			Collider _collider = Trigger.GetComponent<Collider>();
			if (_collider)
			{
				_collider.enabled = data.isOn;
			}
			else 
			{
				Collider2D _collider2D = Trigger.GetComponent<Collider2D>();
				if (_collider2D)
				{
					_collider2D.enabled = data.isOn;
				}
			}
		}


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			if (triggerToSave == null) triggerToSave = GetComponent<AC_Trigger> ();

			CustomGUILayout.Header ("Trigger");
			CustomGUILayout.BeginVertical ();
			triggerToSave = (AC_Trigger) CustomGUILayout.ObjectField<AC_Trigger> ("Trigger:", triggerToSave, true);
			startState = (AC_OnOff) CustomGUILayout.EnumPopup ("Trigger state on start:", startState, "", "The enabled state of the Trigger when the game begins");
			CustomGUILayout.EndVertical ();
		}

		#endif

		#endregion


		#region GetSet

		private AC_Trigger Trigger
		{
			get
			{
				if (triggerToSave == null || !Application.isPlaying)
				{
					triggerToSave = GetComponent<AC_Trigger> ();
				}
				return triggerToSave;
			}
		}

		#endregion

	}


	/**
	 * A data container used by the RememberTrigger script.
	 */
	[System.Serializable]
	public class TriggerData : RememberData
	{

		/** True if the Trigger is enabled */
		public bool isOn;


		/**
		 * The default Constructor.
		 */
		public TriggerData () { }

	}

}