/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionMoveableCheck.cs"
 * 
 *	This action checks the position of a Drag object
 *	along a locked track
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
	public class ActionTrackCheck : ActionCheck
	{

		public Moveable_Drag dragObject;
		public int dragConstantID = 0;
		public int dragParameterID = -1;
		protected Moveable_Drag runtimeDragObject;

		public DragTrack dragTrack;
		public int dragTrackConstantID = 0;
		public int dragTrackParameterID = -1;
		protected DragTrack runtimeDragTrack;

		public float checkPosition;
		public int checkPositionParameterID = -1;

		public float errorMargin = 0.05f;
		public IntCondition condition;

		public int snapID = 0;
		public int snapParameterID = -1;

		[SerializeField] protected TrackCheckMethod method = TrackCheckMethod.PositionValue;
		protected enum TrackCheckMethod { PositionValue, WithinTrackRegion };


		public override ActionCategory Category { get { return ActionCategory.Moveable; }}
		public override string Title { get { return "Check track position"; }}
		public override string Description { get { return "Queries how far a Draggable object is along its track."; }}
		

		public override void AssignValues (List<ActionParameter> parameters)
		{
			runtimeDragObject = AssignFile <Moveable_Drag> (parameters, dragParameterID, dragConstantID, dragObject);
			runtimeDragTrack = AssignFile <DragTrack> (parameters, dragTrackParameterID, dragTrackConstantID, dragTrack);

			checkPosition = AssignFloat (parameters, checkPositionParameterID, checkPosition);
			checkPosition = Mathf.Max (0f, checkPosition);
			checkPosition = Mathf.Min (1f, checkPosition);

			snapID = AssignInteger (parameters, snapParameterID, snapID);
		}

			
		public override bool CheckCondition ()
		{
			if (runtimeDragObject == null) return false;

			if (runtimeDragTrack != null && runtimeDragObject.track != runtimeDragTrack)
			{
				return false;
			}

			if (method == TrackCheckMethod.PositionValue)
			{
				float actualPositionAlong = runtimeDragObject.GetPositionAlong ();

				switch (condition)
				{
					case IntCondition.EqualTo:
						if (actualPositionAlong > (checkPosition - errorMargin) && actualPositionAlong < (checkPosition + errorMargin))
						{
							return true;
						}
						break;

					case IntCondition.NotEqualTo:
						if (actualPositionAlong < (checkPosition - errorMargin) || actualPositionAlong > (checkPosition + errorMargin))
						{
							return true;
						}
						break;

					case IntCondition.LessThan:
						if (actualPositionAlong < checkPosition)
						{
							return true;
						}
						break;

					case IntCondition.MoreThan:
						if (actualPositionAlong > checkPosition)
						{
							return true;
						}
						break;
				}
			}
			else if (method == TrackCheckMethod.WithinTrackRegion)
			{
				if (runtimeDragObject.track != null)
				{
					return runtimeDragObject.track.IsWithinTrackRegion (runtimeDragObject.trackValue, snapID);
				}
			}

			return false;
		}


		#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			ComponentField ("Drag object:", ref dragObject, ref dragConstantID, parameters, ref dragParameterID);
			if (method == TrackCheckMethod.WithinTrackRegion)
			{
				if (dragParameterID > 0 || (dragObject && dragObject.dragMode != DragMode.LockToTrack))
				{
					EditorGUILayout.HelpBox ("The chosen Drag object must be in 'Lock To Track' mode", MessageType.Warning);
				}
			}

			ComponentField ("Track (optional):", ref dragTrack, ref dragConstantID, parameters, ref dragTrackParameterID);

			method = (TrackCheckMethod) EditorGUILayout.EnumPopup ("Method:", method);
			if (method == TrackCheckMethod.PositionValue)
			{
				condition = (IntCondition) EditorGUILayout.EnumPopup ("Condition:", condition);

				SliderField ("Position:", ref checkPosition, 0f, 1f, parameters, ref checkPositionParameterID);

				if (condition == IntCondition.EqualTo || condition == IntCondition.NotEqualTo)
				{
					errorMargin = EditorGUILayout.Slider ("Error margin:", errorMargin, 0f, 1f);
				}
			}
			else if (method == TrackCheckMethod.WithinTrackRegion)
			{
				if (dragObject == null)
				{
					EditorGUILayout.HelpBox ("A drag object must be assigned above for snap regions to display.", MessageType.Info);
				}
				else if (dragObject.dragMode != DragMode.LockToTrack)
				{
					EditorGUILayout.HelpBox ("The chosen Drag object is not locked to a Track.", MessageType.Warning);
				}
				else
				{
					ActionParameter[] filteredParameters = GetFilteredParameters (parameters, ParameterType.Integer);
					bool parameterOverride = SmartFieldStart ("Region ID:", filteredParameters, ref snapParameterID, "Region ID:");
					if (!parameterOverride)
					{
						List<string> labelList = new List<string>();
						int snapIndex = 0;

						DragTrack track = (dragTrack != null) ? dragTrack : dragObject.track;
						if (track && track.allTrackSnapData != null && track.allTrackSnapData.Count > 0)
						{ 
							for (int i=0; i<track.allTrackSnapData.Count; i++)
							{
								labelList.Add (track.allTrackSnapData[i].EditorLabel);
							
								if (track.allTrackSnapData[i].ID == snapID)
								{
									snapIndex = i;
								}
							}
						
							snapIndex = EditorGUILayout.Popup ("Region:", snapIndex, labelList.ToArray ());
							snapID = track.allTrackSnapData[snapIndex].ID;
						}
						else
						{
							EditorGUILayout.HelpBox("The chosen Drag object's Track has no Regions defined.", MessageType.Warning);
						}
					}
					SmartFieldEnd (filteredParameters, parameterOverride, ref snapParameterID);
				}
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			dragConstantID = AssignConstantID<Moveable_Drag> (dragObject, dragConstantID, dragParameterID);
		}


		public override string SetLabel ()
		{
			if (dragObject != null)
			{
				return (dragObject.gameObject.name + " " + condition.ToString () + " " + checkPosition);
			}
			return string.Empty;
		}


		public override bool ReferencesObjectOrID (GameObject gameObject, int id)
		{
			if (dragParameterID < 0)
			{
				if (dragObject && dragObject.gameObject == gameObject) return true;
				if (dragConstantID == id && id != 0) return true;
			}
			return base.ReferencesObjectOrID (gameObject, id);
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Object: Check track position' Action</summary>
		 * <param name = "dragObject">The moveable object to query</param>
		 * <param name = "trackPosition">The track position to check for</param>
		 * <param name = "condition">The condition to make</param>
		 * <param name = "errorMargin">The maximum difference between the queried track position, and the true track position, for the condition to be met</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionTrackCheck CreateNew (Moveable_Drag dragObject, float trackPosition, IntCondition condition = IntCondition.MoreThan, float errorMargin = 0.05f)
		{
			ActionTrackCheck newAction = CreateNew<ActionTrackCheck> ();
			newAction.method = TrackCheckMethod.PositionValue;
			newAction.dragObject = dragObject;
			newAction.TryAssignConstantID (newAction.dragObject, ref newAction.dragConstantID);
			newAction.checkPosition = trackPosition;
			newAction.errorMargin = errorMargin;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Object: Check track position' Action</summary>
		 * <param name = "dragObject">The moveable object to query</param>
		 * <param name = "trackRegionID">The ID of the track's region</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionTrackCheck CreateNew (Moveable_Drag dragObject, int trackRegionID)
		{
			ActionTrackCheck newAction = CreateNew<ActionTrackCheck> ();
			newAction.method = TrackCheckMethod.WithinTrackRegion;
			newAction.dragObject = dragObject;
			newAction.TryAssignConstantID (newAction.dragObject, ref newAction.dragConstantID);
			newAction.snapID = trackRegionID;
			return newAction;
		}

	}

}