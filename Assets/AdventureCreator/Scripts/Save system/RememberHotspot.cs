/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"RememberHotspot.cs"
 * 
 *	This script is attached to hotspot objects in the scene
 *	whose on/off state we wish to save. 
 * 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/** Attach this script to Hotspot objects in the scene whose state you wish to save. */
	[AddComponentMenu("Adventure Creator/Save system/Remember Hotspot")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_hotspot.html")]
	public class RememberHotspot : Remember
	{

		#region Variables

		[SerializeField] private Hotspot hotspotToSave = null;
		public AC_OnOff startState = AC_OnOff.On;

		#endregion


		#region CustomEvents

		protected override void OnInitialiseScene ()
		{
			if (Hotspot != null &&
				KickStarter.settingsManager &&
				isActiveAndEnabled)
			{
				if (startState == AC_OnOff.On)
				{
					Hotspot.TurnOn ();
				}
				else
				{
					Hotspot.TurnOff ();
				}
			}
		}

		#endregion


		#region PublicFunctions

		public override string SaveData ()
		{
			if (Hotspot == null) return string.Empty;

			HotspotData hotspotData = new HotspotData ();
			hotspotData.objectID = constantID;
			hotspotData.savePrevented = savePrevented;

			hotspotData.isOn = Hotspot.IsOn ();
			hotspotData.buttonStates = ButtonStatesToString (Hotspot);

			hotspotData.hotspotName = Hotspot.GetName (0);
			hotspotData.displayLineID = Hotspot.displayLineID;
			
			return Serializer.SaveScriptData <HotspotData> (hotspotData);
		}


		public override void LoadData (string stringData)
		{
			if (Hotspot == null) return;

			HotspotData data = Serializer.LoadScriptData <HotspotData> (stringData);
			if (data == null)
			{
				return;
			}
			SavePrevented = data.savePrevented; if (savePrevented) return;

			if (data.isOn)
			{
				Hotspot.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer);
			}
			else
			{
				Hotspot.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
			}

			if (Hotspot)
			{
				if (data.isOn)
				{
					Hotspot.TurnOn ();
				}
				else
				{
					Hotspot.TurnOff ();
				}

				StringToButtonStates (Hotspot, data.buttonStates);

				if (!string.IsNullOrEmpty (data.hotspotName))
				{
					Hotspot.SetName (data.hotspotName, data.displayLineID);
				}
				Hotspot.ResetMainIcon ();
			}
		}

		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			if (hotspotToSave == null) hotspotToSave = GetComponent<Hotspot> ();

			CustomGUILayout.Header ("Hotspot");
			CustomGUILayout.BeginVertical ();
			hotspotToSave = (Hotspot) CustomGUILayout.ObjectField<Hotspot> ("Hotspot to save:", hotspotToSave, true);
			startState = (AC_OnOff) CustomGUILayout.EnumPopup ("State on start:", startState, "The interactive state of the Hotspot when the game begins");
			CustomGUILayout.EndVertical ();
		}

		#endif

		#endregion


		#region PrivateFunctions

		private void StringToButtonStates (Hotspot hotspot, string stateString)
		{
			if (string.IsNullOrEmpty (stateString))
			{
				return;
			}

			string[] typesArray = stateString.Split (SaveSystem.pipe[0]);
			
			if (KickStarter.settingsManager == null || KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				// Look interactions
				if (hotspot.provideLookInteraction && hotspot.lookButton != null && !string.IsNullOrEmpty (typesArray[0]))
				{
					hotspot.SetButtonState (hotspot.lookButton, !SetButtonDisabledValue (typesArray [0]));
				}
			}

			if (hotspot.provideUseInteraction && hotspot.useButtons.Count > 0)
			{
				string[] usesArray = typesArray[1].Split (","[0]);
				
				for (int i=0; i<usesArray.Length; i++)
				{
					if (string.IsNullOrEmpty (usesArray[i])) continue;

					if (hotspot.useButtons.Count < i+1)
					{
						break;
					}

					hotspot.SetButtonState (hotspot.useButtons[i], !SetButtonDisabledValue (usesArray [i]));
				}
			}

			// Inventory interactions
			if (hotspot.provideInvInteraction && typesArray.Length > 2 && hotspot.invButtons.Count > 0)
			{
				string[] invArray = typesArray[2].Split (","[0]);
				
				for (int i=0; i<invArray.Length; i++)
				{
					if (string.IsNullOrEmpty (invArray[i])) continue;

					if (i < hotspot.invButtons.Count)
					{
						hotspot.SetButtonState (hotspot.invButtons[i], !SetButtonDisabledValue (invArray [i]));
					}
				}
			}
		}
		
		
		private string ButtonStatesToString (Hotspot hotspot)
		{
			System.Text.StringBuilder stateString = new System.Text.StringBuilder ();
			
			if (KickStarter.settingsManager == null || KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				// Single-use and Look interaction
				if (hotspot.provideLookInteraction)
				{
					stateString.Append (GetButtonDisabledValue (hotspot.lookButton));
				}
			}

			stateString.Append (SaveSystem.pipe);

			// Multi-use interactions
			if (hotspot.provideUseInteraction)
			{
				foreach (AC.Button button in hotspot.useButtons)
				{
					stateString.Append (GetButtonDisabledValue (button));
					
					if (hotspot.useButtons.IndexOf (button) < hotspot.useButtons.Count-1)
					{
						stateString.Append (",");
					}
				}
			}
				
			stateString.Append (SaveSystem.pipe);

			// Inventory interactions
			if (hotspot.provideInvInteraction)
			{
				foreach (AC.Button button in hotspot.invButtons)
				{
					stateString.Append (GetButtonDisabledValue (button));
					
					if (hotspot.invButtons.IndexOf (button) < hotspot.invButtons.Count-1)
					{
						stateString.Append (",");
					}
				}
			}
			
			return stateString.ToString ();
		}


		private string GetButtonDisabledValue (AC.Button button)
		{
			if (button != null && button.isDisabled)
			{
				return ("0");
			}
			
			return ("1");
		}
		
		
		private bool SetButtonDisabledValue (string text)
		{
			if (text == "1")
			{
				return false;
			}
			
			return true;
		}

		#endregion


		#region GetSet
		
		private Hotspot Hotspot
		{
			get
			{
				if (hotspotToSave == null)
				{
					hotspotToSave = GetComponent <Hotspot>();
				}
				return hotspotToSave;
			}
		}

		#endregion

	}


	/** A data container used by the RememberHotspot script. */
	[System.Serializable]
	public class HotspotData : RememberData
	{

		/** True if the Hotspot is enabled */
		public bool isOn;
		/** The enabled state of each Interaction */
		public string buttonStates;
		/** The ID number that references the Hotspot's name, as generated by the Speech Manager */
		public int displayLineID;
		/** The Hotspot's display name */
		public string hotspotName;

		/** The default Constructor. */
		public HotspotData () { }
	}

}