/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionInteraction.cs"
 * 
 *	This Action can enable and disable
 *	a Hotspot's individual Interactions.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionInteractionCheck : ActionCheck
	{
		
		public int parameterID = -1;
		public int constantID = 0;
		public Hotspot hotspot;
		protected Hotspot runtimeHotspot;
		
		public InteractionType interactionType;
		public int number = 0;


		public override ActionCategory Category { get { return ActionCategory.Hotspot; }}
		public override string Title { get { return "Check interaction enabled"; }}
		public override string Description { get { return "Checks the enabled state of individual Interactions on a Hotspot."; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			runtimeHotspot = AssignFile <Hotspot> (parameters, parameterID, constantID, hotspot);
		}
		
		
		public override bool CheckCondition ()
		{
			if (runtimeHotspot == null)
			{
				return false;
			}
			
			switch (interactionType)
			{
				case InteractionType.Use:
					if (runtimeHotspot.useButtons.Count > number)
					{
						return !runtimeHotspot.useButtons[number].isDisabled;
					}
					else
					{
						ACDebug.LogWarning ("Cannot check Hotspot " + runtimeHotspot.gameObject.name + "'s Use button " + number.ToString () + " because it doesn't exist!", runtimeHotspot);
					}
					break;

				case InteractionType.Examine:
					return !runtimeHotspot.lookButton.isDisabled;

				case InteractionType.Inventory:
					if (runtimeHotspot.invButtons.Count > number)
					{
						return !runtimeHotspot.invButtons[number].isDisabled;
					}
					else
					{
						ACDebug.LogWarning ("Cannot check Hotspot " + runtimeHotspot.gameObject.name + "'s Inventory button " + number.ToString () + " because it doesn't exist!", runtimeHotspot);
					}
					break;

				default:
					break;
			}
			
			return false;
		}

		
		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			if (KickStarter.settingsManager)
			{
				ComponentField ("Hotspot to check:", ref hotspot, ref constantID, parameters, ref parameterID);
				
				interactionType = (InteractionType) EditorGUILayout.EnumPopup ("Interaction to check:", interactionType);
				
				if ((!isAssetFile && hotspot != null) || isAssetFile)
				{
					if (interactionType == InteractionType.Use)
					{
						if (isAssetFile)
						{
							number = EditorGUILayout.IntField ("Use interaction:", number);
						}
						else if (KickStarter.cursorManager)
						{
							// Multiple use interactions
							List<string> labelList = new List<string>();
							
							foreach (AC.Button button in hotspot.useButtons)
							{
								labelList.Add (hotspot.useButtons.IndexOf (button) + ": " + KickStarter.cursorManager.GetLabelFromID (button.iconID, 0));
							}
							
							number = EditorGUILayout.Popup ("Use interaction:", number, labelList.ToArray ());
						}
						else
						{
							EditorGUILayout.HelpBox ("A Cursor Manager is required.", MessageType.Warning);
						}
					}
					else if (interactionType == InteractionType.Inventory)
					{
						if (isAssetFile)
						{
							number = EditorGUILayout.IntField ("Inventory interaction:", number);
						}
						else if (KickStarter.inventoryManager)
						{
							List<string> labelList = new List<string>();
							
							foreach (AC.Button button in hotspot.invButtons)
							{
								labelList.Add (hotspot.invButtons.IndexOf (button) + ": " + KickStarter.inventoryManager.GetLabel (button.invID));
							}
							
							number = EditorGUILayout.Popup ("Inventory interaction:", number, labelList.ToArray ());
						}
						else
						{
							EditorGUILayout.HelpBox ("An Inventory Manager is required.", MessageType.Warning);
						}
					}
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("A Settings Manager is required for this Action.", MessageType.Warning);
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			constantID = AssignConstantID<Hotspot> (hotspot, constantID, parameterID);
		}

		
		public override string SetLabel ()
		{
			if (hotspot != null)
			{
				return hotspot.name + " - " + interactionType;
			}
			return string.Empty;
		}


		public override bool ReferencesObjectOrID (GameObject _gameObject, int id)
		{
			if (parameterID < 0)
			{
				if (hotspot && hotspot.gameObject == _gameObject) return true;
				if (constantID == id) return true;
			}
			return base.ReferencesObjectOrID (_gameObject, id);
		}
		
		#endif


		/**
		 * <summary>Creates a new instance of the 'Hotspot: Check interaction enabled' Action</summary>
		 * <param name = "hotspotToCheck">The Hotspot to check</param>
		 * <param name = "interactionType">The interaction type to check</param>
		 * <param name = "index">The index number of the interaction to check within the given interactionType</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionInteractionCheck CreateNew (Hotspot hotspotToCheck, InteractionType interactionType, int index)
		{
			ActionInteractionCheck newAction = CreateNew<ActionInteractionCheck> ();
			newAction.hotspot = hotspotToCheck;
			newAction.TryAssignConstantID (newAction.hotspot, ref newAction.constantID);
			newAction.interactionType = interactionType;
			newAction.number = index;
			return newAction;
		}
		
	}
	
}