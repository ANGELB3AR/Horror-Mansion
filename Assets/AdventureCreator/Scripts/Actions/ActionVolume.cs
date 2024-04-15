/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionVolume.cs"
 * 
 *	This action alters the "relative volume" of any Sound script
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
	public class ActionVolume : Action
	{
		
		public int constantID = 0;
		public int parameterID = -1;
		public Sound soundObject;
		protected Sound runtimeSoundObject;
		
		public float newRelativeVolume = 1f;
		public int newRelativeVolumeParameterID = -1;

		public float changeTime = 0f;
		public int changeTimeParameterID = -1;


		public override ActionCategory Category { get { return ActionCategory.Sound; }}
		public override string Title { get { return "Change volume"; }}
		public override string Description { get { return "Alters the 'relative volume' of any Sound object."; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			runtimeSoundObject = AssignFile <Sound> (parameters, parameterID, constantID, soundObject);
			newRelativeVolume = AssignFloat (parameters, newRelativeVolumeParameterID, newRelativeVolume);
			changeTime = AssignFloat (parameters, changeTimeParameterID, changeTime);
		}
		
		
		public override float Run ()
		{
			if (!isRunning)
			{
				if (runtimeSoundObject)
				{
					runtimeSoundObject.ChangeRelativeVolume (newRelativeVolume, changeTime);
					
					if (willWait && changeTime > 0f)
					{
						isRunning = true;
						return changeTime;
					}
				}
			}
			else
			{
				isRunning = false;
			}

			return 0f;
		}
				
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			ComponentField ("Sound object:", ref soundObject, ref constantID, parameters, ref parameterID);
			SliderField ("New relative volume:", ref newRelativeVolume, 0f, 1f, parameters, ref newRelativeVolumeParameterID);
			SliderField ("Change time (s):", ref changeTime, 0f, 10f, parameters, ref changeTimeParameterID);

			if (changeTime > 0f)
			{
				willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (saveScriptsToo)
			{
				AddSaveScript <RememberSound> (soundObject);
			}
			constantID = AssignConstantID<Sound> (soundObject, constantID, parameterID);
		}
		
		
		public override string SetLabel ()
		{
			if (soundObject != null)
			{
				return soundObject.name + " to " + newRelativeVolume.ToString ();
			}
			return string.Empty;
		}


		public override bool ReferencesObjectOrID (GameObject gameObject, int id)
		{
			if (parameterID < 0)
			{
				if (soundObject && soundObject.gameObject == gameObject) return true;
				return (constantID == id && id != 0);
			}
			return base.ReferencesObjectOrID (gameObject, id);
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Sound: Change volume' Action</summary>
		 * <param name = "sound">The Sound object to affect</param>
		 * <param name = "newRelativeVolume">The Sound's new relative volume value</param>
		 * <param name = "transitionTime">The time, in seconds, to take when transitioning from the old to the new volume</param>
		 * <param name = "waitUntilFinish">If True, then the Action will wait until the transition is complete</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionVolume CreateNew (Sound sound, float newRelativeVolume, float transitionTime = 0.5f, bool waitUntilFinish = false)
		{
			ActionVolume newAction = CreateNew<ActionVolume> ();
			newAction.soundObject = sound;
			newAction.TryAssignConstantID (newAction.soundObject, ref newAction.constantID);
			newAction.newRelativeVolume = newRelativeVolume;
			newAction.changeTime = transitionTime;
			newAction.willWait = waitUntilFinish;
			return newAction;
		}
		
	}
	
}