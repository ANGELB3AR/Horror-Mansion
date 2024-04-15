/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"RememberVideoPlayer.cs"
 * 
 *	This script is attached to VideoPlayer objects
 *	whose playback state we wish to save.
 * 
 */

#define ALLOW_VIDEO
using System.Collections;
#if ALLOW_VIDEO
using UnityEngine;
using UnityEngine.Video;
#if AddressableIsPresent
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
#endif

namespace AC
{
	
	/** Attach this to GameObjects whose VideoPlayer's playback state you wish to save. */
	[AddComponentMenu("Adventure Creator/Save system/Remember Video Player")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_video_player.html")]
	public class RememberVideoPlayer : Remember
	{

		/** If True, the VideoClip assigned in the VideoPlayer component will be stored in save game files. */
		public bool saveClipAsset;

		private VideoPlayer videoPlayer;
		private double loadTime;
		private bool playAfterLoad;


		public override string SaveData ()
		{
			VideoPlayerData videoPlayerData = new VideoPlayerData ();
			videoPlayerData.objectID = constantID;
			videoPlayerData.savePrevented = savePrevented;

			if (VideoPlayer)
			{
				videoPlayerData.isPlaying = videoPlayer.isPlaying;
				videoPlayerData.currentFrame = videoPlayer.frame;
				videoPlayerData.currentTime = videoPlayer.time;

				if (saveClipAsset)
				{
					if (videoPlayer.clip)
					{
						videoPlayerData.clipAssetID = AssetLoader.GetAssetInstanceID (videoPlayer.clip);
					}
				}
			}

			return Serializer.SaveScriptData <VideoPlayerData> (videoPlayerData);
		}
		

		public override IEnumerator LoadDataCo (string stringData)
		{
			VideoPlayerData data = Serializer.LoadScriptData <VideoPlayerData> (stringData);
			if (data == null) yield break;
			SavePrevented = data.savePrevented; if (savePrevented) yield break;

			if (VideoPlayer)
			{
				#if AddressableIsPresent
				if (saveClipAsset && KickStarter.settingsManager.saveAssetReferencesWithAddressables && !string.IsNullOrEmpty (data.clipAssetID))
				{
					var loadDataCoroutine = LoadDataFromAddressable (data);
					while (loadDataCoroutine.MoveNext ())
					{
						yield return loadDataCoroutine.Current;
					}

					yield break;
				}
				#endif

				LoadDataFromResources (data);
			}
		}


		#if AddressableIsPresent

		private IEnumerator LoadDataFromAddressable (VideoPlayerData data)
		{
			AsyncOperationHandle<VideoClip> handle = Addressables.LoadAssetAsync<VideoClip> (data.clipAssetID);
			yield return handle;
			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				videoPlayer.clip = handle.Result;
			}
			Addressables.Release (handle);

			LoadRemainingData (data);
		}

		#endif


		private void LoadDataFromResources (VideoPlayerData data)
		{
			if (saveClipAsset)
			{
				VideoClip _clip = AssetLoader.RetrieveAsset (videoPlayer.clip, data.clipAssetID);
				if (_clip)
				{
					videoPlayer.clip = _clip;
				}
			}
			
			LoadRemainingData (data);
		}


		private void LoadRemainingData (VideoPlayerData data)
		{
			videoPlayer.time = data.currentTime;

			if (data.isPlaying)
			{
				if (!videoPlayer.isPrepared)
				{
					loadTime = data.currentTime;
					playAfterLoad = true;
					videoPlayer.prepareCompleted += OnPrepareVideo;
					videoPlayer.Prepare ();
				}
				else
				{
					videoPlayer.Play ();
				}
			}
			else
			{
				if (data.currentTime > 0f)
				{
					if (!videoPlayer.isPrepared)
					{
						loadTime = data.currentTime;
						playAfterLoad = false;
						videoPlayer.prepareCompleted += OnPrepareVideo;
						videoPlayer.Prepare ();
					}
					else
					{
						videoPlayer.Pause ();
					}
				}
				else
				{
					videoPlayer.Stop ();
				}
			}
		}


		private void OnPrepareVideo (VideoPlayer videoPlayer)
		{
			videoPlayer.time = loadTime;
			if (playAfterLoad)
			{
				videoPlayer.Play ();
			}
			else
			{
				videoPlayer.Pause ();
			}
		}


		private VideoPlayer VideoPlayer
		{
			get
			{
				if (videoPlayer == null)
				{
					videoPlayer = GetComponent <VideoPlayer>();
				}
				return videoPlayer;
			}
		}

	}


	/** A data container used by the RememberVisibility script. */
	[System.Serializable]
	public class VideoPlayerData : RememberData
	{

		/* True if the video is currently playing */
		public bool isPlaying;
		/* The current frame number (this is now deprecated) */
		public long currentFrame;
		/** The current time */
		public double currentTime;
		/** The Instance ID of the current clip asset */
		public string clipAssetID;


		/** The default Constructor. */
		public VideoPlayerData () { }

	}

}

#endif