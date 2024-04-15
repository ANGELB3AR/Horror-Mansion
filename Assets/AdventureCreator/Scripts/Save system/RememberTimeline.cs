/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"RememberTimeline.cs"
 * 
 *	This script is attached to PlayableDirector objects in the scene
 *	we wish to save (Unity 2017+ only).
 * 
 */

using UnityEngine;
#if TimelineIsPresent
using UnityEngine.Timeline;
#endif
using UnityEngine.Playables;
using System.Collections;
#if AddressableIsPresent
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/** Attach this script to PlayableDirector objects you wish to save. */
	[AddComponentMenu("Adventure Creator/Save system/Remember Timeline")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_timeline.html")]
	public class RememberTimeline : Remember
	{

		#region Variables

		/** If True, the GameObjects bound to the Timeline will be stored in save game files */
		public bool saveBindings;
		/** If True, the Timeline asset assigned in the PlayableDirector's Timeline field will be stored in save game files. */
		public bool saveTimelineAsset;
		/** If True, and the Timeline was not playing when it was saved, it will be evaluated at its playback point - causing the effects of it running at that single frame to be restored */
		public bool evaluateWhenStopped;
		[SerializeField] private PlayableDirector playableDirectorToSave = null;

		#endregion


		#region PublicFunctions

		public override string SaveData ()
		{
			if (PlayableDirector == null) return string.Empty;

			TimelineData timelineData = new TimelineData ();
			timelineData.objectID = constantID;
			timelineData.savePrevented = savePrevented;

			timelineData.isPlaying = (PlayableDirector.state == PlayState.Playing);
			timelineData.currentTime = PlayableDirector.time;
			timelineData.trackObjectData = string.Empty;
			timelineData.timelineAssetID = string.Empty;

			if (PlayableDirector.playableAsset)
			{
				#if TimelineIsPresent
				TimelineAsset timeline = (TimelineAsset) PlayableDirector.playableAsset;

				if (timeline)
				{
					if (saveTimelineAsset)
					{
						timelineData.timelineAssetID = AssetLoader.GetAssetInstanceID (timeline);
					}

					if (saveBindings)
					{
						int[] bindingIDs = new int[timeline.outputTrackCount];
						for (int i=0; i<bindingIDs.Length; i++)
						{
							TrackAsset trackAsset = timeline.GetOutputTrack (i);
							GameObject trackObject = PlayableDirector.GetGenericBinding (trackAsset) as GameObject;
							bindingIDs[i] = 0;
							if (trackObject)
							{
								ConstantID cIDComponent = trackObject.GetComponent <ConstantID>();
								if (cIDComponent)
								{
									bindingIDs[i] = cIDComponent.constantID;
								}
							}
						}

						for (int i=0; i<bindingIDs.Length; i++)
						{
							timelineData.trackObjectData += bindingIDs[i].ToString ();
							if (i < (bindingIDs.Length - 1))
							{
								timelineData.trackObjectData += ",";
							}
						}
					}
				}
				#endif
			}

			return Serializer.SaveScriptData <TimelineData> (timelineData);
		}
		

		public override IEnumerator LoadDataCo (string stringData)
		{
			if (PlayableDirector == null) yield break;

			TimelineData data = Serializer.LoadScriptData <TimelineData> (stringData);
			if (data == null) yield break;
			SavePrevented = data.savePrevented; if (savePrevented) yield break;

			#if AddressableIsPresent

			if (saveTimelineAsset && KickStarter.settingsManager.saveAssetReferencesWithAddressables && !string.IsNullOrEmpty (data.timelineAssetID))
			{
				var loadDataCoroutine = LoadDataFromAddressable (data);
				while (loadDataCoroutine.MoveNext ())
				{
					yield return loadDataCoroutine.Current;
				}

				LoadRemainingData (data);
				yield break;
			}

			#endif

			LoadDataFromResources (data);
			LoadRemainingData (data);
		}


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			if (playableDirectorToSave == null) playableDirectorToSave = GetComponent<PlayableDirector> ();

			CustomGUILayout.Header ("Timeline");
			CustomGUILayout.BeginVertical ();
			playableDirectorToSave = (PlayableDirector) CustomGUILayout.ObjectField<PlayableDirector> ("PlayableDirector to save:", playableDirectorToSave, true);
			saveBindings = CustomGUILayout.Toggle ("Save bindings?", saveBindings, "", "If True, the GameObjects bound to the Timeline will be stored in save game files.");
			saveTimelineAsset = CustomGUILayout.Toggle ("Save Timeline asset?", saveTimelineAsset, "", "If True, the Timeline asset assigned in the PlayableDirector's Timeline field will be stored in save game files.");
			if (saveTimelineAsset)
			{
				EditorGUILayout.HelpBox ("Both the original and new 'Timeline' assets will need placing in a Resources folder.", MessageType.Info);
			}
			evaluateWhenStopped = CustomGUILayout.Toggle ("Evaluate when stopped?", evaluateWhenStopped, "", "If True, and the Timeline was not playing when it was saved, it will be evaluated at its playback point - causing the effects of it running at that single frame to be restored");
			CustomGUILayout.EndVertical ();
		}

		#endif

		#endregion


		#region PrivateFunctions

		private IEnumerator LoadDataFromAddressable (TimelineData data)
		{
			#if AddressableIsPresent && TimelineIsPresent
			AsyncOperationHandle<TimelineAsset> handle = Addressables.LoadAssetAsync<TimelineAsset> (data.timelineAssetID);
			yield return handle;
			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				PlayableDirector.playableAsset = handle.Result;
			}
			Addressables.Release (handle);
			#else
			yield break;
			#endif
		}


		private void LoadDataFromResources (TimelineData data)
		{
			#if TimelineIsPresent
			if (PlayableDirector.playableAsset)
			{
				TimelineAsset timeline = (TimelineAsset) PlayableDirector.playableAsset;

				if (timeline)
				{
					if (saveTimelineAsset)
					{
						TimelineAsset _timeline = AssetLoader.RetrieveAsset (timeline, data.timelineAssetID);
						if (_timeline)
						{
							PlayableDirector.playableAsset = _timeline;
							timeline = _timeline;
						}
					}
				}
			}
			#endif
		}


		private void LoadRemainingData (TimelineData data)
		{
			#if TimelineIsPresent
			if (PlayableDirector.playableAsset)
			{
				TimelineAsset timeline = (TimelineAsset) PlayableDirector.playableAsset;

				if (timeline)
				{
					if (saveBindings && !string.IsNullOrEmpty (data.trackObjectData))
					{
						string[] bindingIDs = data.trackObjectData.Split (","[0]);

						for (int i=0; i<bindingIDs.Length; i++)
						{
							int bindingID = 0;
							if (int.TryParse (bindingIDs[i], out bindingID))
							{
								if (bindingID != 0)
								{
									var track = timeline.GetOutputTrack (i);
									if (track)
									{
										ConstantID savedObject = ConstantID.GetComponent (bindingID, gameObject.scene, true);
										if (savedObject)
										{
											PlayableDirector.SetGenericBinding (track, savedObject.gameObject);
										}
									}
								}
							}
						}
					}
				}
			}
			#endif

			PlayableDirector.time = data.currentTime;
			if (data.isPlaying)
			{
				PlayableDirector.Play ();
			}
			else
			{
				PlayableDirector.Stop ();

				if (evaluateWhenStopped)
				{
					PlayableDirector.Evaluate ();
				}
			}
		}

		#endregion


		#region GetSet

		private PlayableDirector PlayableDirector
		{
			get
			{
				if (playableDirectorToSave == null)
				{
					playableDirectorToSave = GetComponent <PlayableDirector>();
				}
				return playableDirectorToSave;
			}
		}

		#endregion

	}


	/** A data container used by the RememberTimeline script. */
	[System.Serializable]
	public class TimelineData : RememberData
	{

		/** True if the Timline is playing */
		public bool isPlaying;
		/** The current time along the Timeline */
		public double currentTime;
		/** Which objects are loaded into the tracks */
		public string trackObjectData;
		/** The Instance ID of the current Timeline asset */
		public string timelineAssetID;

		
		/** The default Constructor. */
		public TimelineData () { }

	}
	
}