/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"RememberTrack.cs"
 * 
 *	This script is attached to Drag Track objects you wish to save.
 * 
 */

using UnityEngine;
using System.Text;

namespace AC
{

	/**
	 * This script is attached to Drag Track objects you wish to save.
	 */
	[AddComponentMenu("Adventure Creator/Save system/Remember Track")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_moveable.html")]
	public class RememberTrack : Remember
	{

		#region Variables

		[SerializeField] private DragTrack trackToSave = null;

		#endregion


		#region PublicFunctions

		public override string SaveData()
		{
			if (Track == null) return string.Empty;

			TrackData data = new TrackData ();

			data.objectID = constantID;
			data.savePrevented = savePrevented;

			if (Track.allTrackSnapData != null)
			{
				StringBuilder stateString = new StringBuilder ();

				foreach (TrackSnapData trackSnapData in Track.allTrackSnapData)
				{
					stateString.Append (trackSnapData.ID.ToString ());
					stateString.Append (SaveSystem.colon);
					stateString.Append (trackSnapData.IsEnabled ? "1" : "0");
					stateString.Append (SaveSystem.pipe);
				}

				data.enabledStates = stateString.ToString();
			}

			return Serializer.SaveScriptData<MoveableData> (data);
		}


		public override void LoadData (string stringData)
		{
			if (Track == null) return;

			TrackData data = Serializer.LoadScriptData <TrackData> (stringData);
			if (data == null)
			{
				return;
			}
			SavePrevented = data.savePrevented; if (savePrevented) return;

			if (Track.allTrackSnapData != null)
			{
				string[] valuesArray = data.enabledStates.Split (SaveSystem.pipe[0]);
				for (int i = 0; i < Track.allTrackSnapData.Count; i++)
				{
					if (i < valuesArray.Length)
					{
						string[] chunkData = valuesArray[i].Split (SaveSystem.colon[0]);
						if (chunkData != null && chunkData.Length == 2)
						{
							int _regionID = 0;
							if (int.TryParse (chunkData[0], out _regionID))
							{
								TrackSnapData snapData = Track.GetSnapData(_regionID);
								if (snapData != null)
								{
									int _isEnabled = 1;
									if (int.TryParse (chunkData[1], out _isEnabled))
									{
										snapData.IsEnabled = (_isEnabled == 1);
									}
								}
							}
						}
					}
				}
			}
		}
		

		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			if (trackToSave == null) trackToSave = GetComponent<DragTrack> ();

			CustomGUILayout.Header ("Track");
			CustomGUILayout.BeginVertical ();
			trackToSave = (DragTrack) CustomGUILayout.ObjectField<DragTrack> ("Track to save:", trackToSave, true);
			CustomGUILayout.EndVertical ();
		}

		#endif

		#endregion


		#region GetSet

		private DragTrack Track
		{
			get
			{
				if (trackToSave == null || !Application.isPlaying)
				{
					trackToSave = GetComponent<DragTrack> ();
				}
				return trackToSave;
			}
		}

		#endregion

	}


	/**
	 * A data container used by the RememberTrack script.
	 */
	[System.Serializable]
	public class TrackData : RememberData
	{

		/** True if the object is on */
		public bool isOn;

		/** Data related to the enabled states of the regions along the Track */
		public string enabledStates;


		/**
		 * The default Constructor.
		 */
		public TrackData() { }

	}

}