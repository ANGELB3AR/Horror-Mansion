/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"RememberSound.cs"
 * 
 *	This script is attached to Sound objects in the scene
 *	we wish to save.
 * 
 */

using UnityEngine;
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

	/** Attach this script to Sound objects you wish to save. */
	[AddComponentMenu("Adventure Creator/Save system/Remember Sound")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_sound.html")]
	public class RememberSound : Remember
	{

		#region Variables

		public bool saveClip = true;
		[SerializeField] private Sound soundToSave = null;

		#endregion


		#region PublicFunctions

		public override string SaveData ()
		{
			if (Sound == null) return string.Empty;

			SoundData soundData = new SoundData();
			soundData.objectID = constantID;
			soundData.savePrevented = savePrevented;

			soundData = Sound.GetSaveData (soundData);
			
			return Serializer.SaveScriptData <SoundData> (soundData);
		}
		

		public override IEnumerator LoadDataCo (string stringData)
		{
			if (Sound == null) yield break;

			SoundData data = Serializer.LoadScriptData <SoundData> (stringData);
			if (data == null) yield break;
			
			SavePrevented = data.savePrevented;
			if (savePrevented || Sound is Music || Sound is Ambience)
			{
				yield break;
			}

			if (KickStarter.saveSystem.loadingGame == LoadingGame.No && Sound.surviveSceneChange)
			{
				yield break;
			}

			if (saveClip && data.isPlaying)
			{
				#if AddressableIsPresent

				if (KickStarter.settingsManager.saveAssetReferencesWithAddressables)
				{
					if (!string.IsNullOrEmpty (data.clipID))
					{
						var loadDataCoroutine = LoadDataFromAddressables (data);
						while (loadDataCoroutine.MoveNext ())
						{
							yield return loadDataCoroutine.Current;
						}
					}
					Sound.LoadData (data);
					yield break;
				}

				#endif

				if (Sound.audioSource)
				{
					Sound.audioSource.clip = AssetLoader.RetrieveAsset (Sound.audioSource.clip, data.clipID);
				}
			}

			Sound.LoadData (data);
		}


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			if (soundToSave == null) soundToSave = GetComponent<Sound> ();

			CustomGUILayout.Header ("Sound");
			CustomGUILayout.BeginVertical ();
			soundToSave = (Sound) CustomGUILayout.ObjectField<Sound> ("Sound to save:", soundToSave, true);
			saveClip = CustomGUILayout.ToggleLeft ("Save change in AudioClip asset?", saveClip, "If True, the currently-playing clip asset will be saved and restored.");
			CustomGUILayout.EndVertical ();
		}

		#endif

		#endregion


		#region PrivateFunctions

		#if AddressableIsPresent

		private IEnumerator LoadDataFromAddressables (SoundData data)
		{
			AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip> (data.clipID);
			yield return handle;
			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				Sound.audioSource.clip = handle.Result;
			}
			Addressables.Release (handle);
		}

		#endif

		#endregion


		#region GetSet

		private Sound Sound
		{
			get
			{
				if (soundToSave == null)
				{
					soundToSave = GetComponent <Sound>();
				}
				return soundToSave;
			}
		}

		#endregion

	}


	/** A data container used by the RememberSound script. */
	[System.Serializable]
	public class SoundData : RememberData
	{

		/** True if a sound is playing */
		public bool isPlaying;
		/** True if a sound is looping */
		public bool isLooping;
		/** How far along the track a sound is */
		public int samplePoint;
		/** A unique identifier for the currently-playing AudioClip */
		public string clipID;
		/** The relative volume on the Sound component */
		public float relativeVolume;
		/** The Sound's maximum volume (internally calculated) */
		public float maxVolume;
		/** The Sound's smoothed-out volume (internally calculated) */
		public float smoothVolume;

		/** The time remaining in a fade effect */
		public float fadeTime;
		/** The original time duration of the active fade effect */
		public float originalFadeTime;
		/** The fade type, where 0 = FadeIn, 1 = FadeOut */
		public int fadeType;
		/** The volume if the Sound's soundType is SoundType.Other */
		public float otherVolume;

		/** The Sound's new relative volume, if changing over time */
		public float targetRelativeVolume;
		/** The Sound's original relative volume, if changing over time */
		public float originalRelativeVolume;
		/** The time remaining in a change in relative volume */
		public float relativeChangeTime;
		/** The original time duration of the active change in relative volume */
		public float originalRelativeChangeTime;

		/** The default Constructor. */
		public SoundData () { }

	}
	
}