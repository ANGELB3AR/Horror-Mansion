/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionHotspotCheckSelected.cs"
 * 
 *	This action is used to check the currently-selected Hotspot.
 * 
 */
 
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class ActionHotspotCheckSelected : ActionCheck
	{

		public Hotspot hotspot;
		public int constantID;
		public int parameterID = -1;
		private Hotspot runtimeHotspot;

		public bool includeLast = false;

		[SerializeField] protected SelectedCheckMethod selectedCheckMethod = SelectedCheckMethod.SpecificHotspot;
		public enum SelectedCheckMethod { SpecificHotspot, NoneSelected };

		public int selectedHotspotParameterID = -1;
		protected ActionParameter runtimeSelectedHotspotParameter;


		public override ActionCategory Category { get { return ActionCategory.Hotspot; }}
		public override string Title { get { return "Check selected"; }}
		public override string Description { get { return "Queries whether or not the chosen Hotspot, or no Hotspot, is currently selected."; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			runtimeHotspot = AssignFile (parameters, parameterID, constantID, hotspot);
			runtimeSelectedHotspotParameter = GetParameterWithID (parameters, selectedHotspotParameterID);
		}
		
		
		public override bool CheckCondition ()
		{
			Hotspot _hotspot = KickStarter.playerInteraction.GetActiveHotspot ();
			if (_hotspot == null && KickStarter.player && KickStarter.player.hotspotDetector != null && KickStarter.player.hotspotDetector.GetAllDetectedHotspots ().Length > 0)
			{
				_hotspot = KickStarter.player.hotspotDetector.GetAllDetectedHotspots ()[0];
			}
			if (_hotspot == null && selectedCheckMethod == SelectedCheckMethod.SpecificHotspot && includeLast)
			{
				_hotspot = KickStarter.playerInteraction.GetLastOrActiveHotspot ();
			}
						
			if (runtimeSelectedHotspotParameter != null && runtimeSelectedHotspotParameter.parameterType == ParameterType.GameObject)
			{
				if (_hotspot)
				{
					runtimeSelectedHotspotParameter.SetValue (_hotspot);
				}
				else
				{
					runtimeSelectedHotspotParameter.SetValue (-1);
				}
			}

			switch (selectedCheckMethod)
			{
				case SelectedCheckMethod.NoneSelected:
					return _hotspot == null;

				case SelectedCheckMethod.SpecificHotspot:
					return _hotspot == runtimeHotspot;
			}
			return false;
		}


		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			selectedCheckMethod = (SelectedCheckMethod) EditorGUILayout.EnumPopup ("Check selected Hotspot is:", selectedCheckMethod);

			if (selectedCheckMethod == SelectedCheckMethod.SpecificHotspot)
			{
				ComponentField ("Hotspot:", ref hotspot, ref constantID, parameters, ref parameterID);
				includeLast = EditorGUILayout.Toggle ("Include last-selected?", includeLast);
			}

			selectedHotspotParameterID = ChooseParameterGUI ("Send to parameter:", parameters, selectedHotspotParameterID, ParameterType.GameObject);
		}
		
		
		public override string SetLabel ()
		{
			switch (selectedCheckMethod)
			{
				case SelectedCheckMethod.NoneSelected:
					return "Nothing";

				case SelectedCheckMethod.SpecificHotspot:
					if (hotspot)
					{
						return hotspot.name;
					}
					break;
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
		 * <summary>Creates a new instance of the 'Hotspot: Check selected' Action, set to check if a specific Hotspot is selected</summary>
		 * <param name = "hotspot">The Hotspot to check for</param>
		 * <param name = "includeLastSelected">If True, the query will return 'True' if the last-selected Hotspot matches the itemID, even if it is not currently selected</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionHotspotCheckSelected CreateNew_SpecificHotspot (Hotspot hotspot, bool includeLastSelected = false)
		{
			ActionHotspotCheckSelected newAction = CreateNew<ActionHotspotCheckSelected> ();
			newAction.selectedCheckMethod = SelectedCheckMethod.SpecificHotspot;
			newAction.hotspot = hotspot;
			newAction.includeLast = includeLastSelected;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Hotspot: Check selected' Action, set to check if no Hotspot is selected</summary>
		 * <returns>The generated Action</returns>
		 */
		public static ActionHotspotCheckSelected CreateNew_NoneSelected ()
		{
			ActionHotspotCheckSelected newAction = CreateNew<ActionHotspotCheckSelected> ();
			newAction.selectedCheckMethod = SelectedCheckMethod.NoneSelected;
			return newAction;
		}

	}

}