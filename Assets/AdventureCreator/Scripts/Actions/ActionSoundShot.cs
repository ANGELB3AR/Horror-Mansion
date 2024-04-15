/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionSoundShot.cs"
 * 
 *	This action plays an AudioClip without the need for a Sound object.
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
	public class ActionSoundShot : Action
	{
		
		public int constantID = 0;
		public int parameterID = -1;
		public Transform origin;
		protected Transform runtimeOrigin;

		public bool playFromDefaultSound;
		public AudioSource audioSource;
		public int audioSourceConstantID = 0;
		public int audioSourceParameterID = -1;
		protected AudioSource runtimeAudioSource;
		
		public AudioClip audioClip;
		public int audioClipParameterID = -1;


		public override ActionCategory Category { get { return ActionCategory.Sound; }}
		public override string Title { get { return "Play one-shot"; }}
		public override string Description { get { return "Plays an AudioClip once without the need for a Sound object."; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			runtimeOrigin = AssignFile (parameters, parameterID, constantID, origin);
			audioClip = (AudioClip) AssignObject <AudioClip> (parameters, audioClipParameterID, audioClip);
			runtimeAudioSource = (AudioSource) AssignFile <AudioSource> (parameters, audioSourceParameterID, audioSourceConstantID, audioSource);
		}
		
		
		public override float Run ()
		{
			if (audioClip == null)
			{
				return 0f;
			}

			if (!isRunning)
			{
				if (playFromDefaultSound)
				{
					if (KickStarter.sceneSettings.defaultSound)
					{
						KickStarter.sceneSettings.defaultSound.SetMaxVolume ();
						KickStarter.sceneSettings.defaultSound.audioSource.PlayOneShot (audioClip);
					}
					else
					{
						LogWarning ("Cannot play Audio from Default Sound because no Default Sound is assigned in the Scene Manager");
						return 0f;
					}
				}
				else if (runtimeAudioSource)
				{
					runtimeAudioSource.PlayOneShot (audioClip, Options.GetSFXVolume ());
				}
				else
				{
					Vector3 originPos = KickStarter.CameraMainTransform.position;
					if (runtimeOrigin != null)
					{
						originPos = runtimeOrigin.position;
					}
					
					float volume = Options.GetSFXVolume ();
					AudioSource.PlayClipAtPoint (audioClip, originPos, volume);
				}

				if (willWait)
				{
					isRunning = true;
					return audioClip.length;
				}
			}
		
			isRunning = false;
			return 0f;
		}
		
		
		public override void Skip ()
		{
			if (audioClip == null)
			{
				return;
			}

			if (runtimeAudioSource)
			{
				// Can't stop audio in this case
			}
			else
			{
				AudioSource[] audioSources = UnityVersionHandler.FindObjectsOfType<AudioSource> ();
				foreach (AudioSource audioSource in audioSources)
				{
					if (audioSource.clip == audioClip && audioSource.isPlaying && audioSource.GetComponent<Sound>() == null)
					{
						audioSource.Stop ();
						return;
					}
				}
			}
		}

		
		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			AssetField ("Clip to play:", ref audioClip, parameters, ref audioClipParameterID);

			playFromDefaultSound = EditorGUILayout.Toggle ("Play from Default Sound?", playFromDefaultSound);
			if (!playFromDefaultSound)
			{
				ComponentField ("Audio source (optional):", ref audioSource, ref audioSourceConstantID, parameters, ref audioSourceParameterID);

				if (audioSource == null && audioSourceParameterID < 0)
				{
					ComponentField ("Position (optional):", ref origin, ref constantID, parameters, ref parameterID);
				}
			}

			willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			constantID = AssignConstantID (origin, constantID, parameterID);
		}
		
		
		public override string SetLabel ()
		{
			if (audioClip != null)
			{
				return audioClip.name;
			}
			return string.Empty;
		}

		
		public override bool ReferencesObjectOrID (GameObject gameObject, int id)
		{
			if (parameterID < 0)
			{
				if (origin && origin.gameObject == gameObject) return true;
				if (constantID == id && id != 0) return true;
			}
			return base.ReferencesObjectOrID (gameObject, id);
		}
		
		#endif


		/**
		 * <summary>Creates a new instance of the 'Sound: Play one-shot' Action</summary>
		 * <param name = "clipToPlay">The clip to play</param>
		 * <param name = "origin">Where to play the clip from</param>
		 * <param name = "waitUntilFinish">If True, then the Action will wait until the sound has finished playing</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionSoundShot CreateNew (AudioClip clipToPlay, Transform origin = null, bool waitUntilFinish = false)
		{
			ActionSoundShot newAction = CreateNew<ActionSoundShot> ();
			newAction.audioClip = clipToPlay;
			newAction.origin = origin;
			newAction.TryAssignConstantID (newAction.origin, ref newAction.constantID);
			newAction.willWait = waitUntilFinish;
			return newAction;
		}
		
	}
	
}