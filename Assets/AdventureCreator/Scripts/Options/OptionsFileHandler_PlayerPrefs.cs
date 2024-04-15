using UnityEngine;

namespace AC
{

	/** A file handler that stores Options data in PlayerPrefs */
	public class OptionsFileHandler_PlayerPrefs : iOptionsFileHandler
	{

		private const string activeProfileKey = "AC_ActiveProfile";


		public void SaveOptions (int profileID, string dataString, bool showLog)
		{
			string prefKeyName = GetPrefKeyName (profileID);

			PlayerPrefs.SetString (prefKeyName, dataString);

			#if UNITY_PS4 || UNITY_SWITCH || UNITY_WEBGL
			PlayerPrefs.Save ();
			#endif

			if (showLog)
			{
				ACDebug.Log ("PlayerPrefs Key '" + prefKeyName + "' saved");
			}
		}


		public string LoadOptions (int profileID, bool showLog)
		{
			string prefKeyName = GetPrefKeyName (profileID);
			string dataString = PlayerPrefs.GetString (prefKeyName);

			if (!string.IsNullOrEmpty (dataString))
			{
				if (showLog)
				{
					ACDebug.Log ("PlayerPrefs Key '" + prefKeyName + "' loaded");
				}
			}

			return dataString;
		}


		public void DeleteOptions (int profileID)
		{
			string prefKeyName = GetPrefKeyName (profileID);

			if (PlayerPrefs.HasKey (prefKeyName))
			{
				PlayerPrefs.DeleteKey (prefKeyName);
				ACDebug.Log ("PlayerPrefs Key '" + prefKeyName + "' deleted");
			}
		}


		public int GetActiveProfile ()
		{
			return PlayerPrefs.GetInt (activeProfileKey, 0);
		}


		public void SetActiveProfile (int profileID)
		{
			PlayerPrefs.SetInt (activeProfileKey, profileID);
		}


		public bool DoesProfileExist (int profileID)
		{
			string prefKeyName = GetPrefKeyName (profileID);
			return PlayerPrefs.HasKey (prefKeyName);
		}


		private string GetPrefKeyName (int profileID)
		{
			string profileName = "Profile";
			if (KickStarter.settingsManager && !string.IsNullOrEmpty (KickStarter.settingsManager.SavePrefix))
			{
				profileName = KickStarter.settingsManager.SavePrefix;
				profileName = profileName.Replace (" ", "_");
			}

			return ("AC_" + profileName + "_" + profileID.ToString ());
		}

	}

}