#if UNITY_EDITOR

#if UNITY_2018_3_OR_NEWER
#define USE_WEB_REQUEST
#endif

using UnityEngine;
using UnityEditor;

#if USE_WEB_REQUEST
using UnityEngine.Networking;
#endif

namespace AC
{

	[InitializeOnLoad]
	static class UpdateChecker
	{

		private const string versionLink = "https://adventurecreator.org/files/currentversion.txt";
		private const string dialogHeader = "AC update checker";

		#if USE_WEB_REQUEST
		private static UnityWebRequest www;
		#else
		private static WWW www;
		#endif
		private static System.Action<bool> onComplete;
		 

		public static void CheckForUpdate (System.Action<bool> _onComplete)
		{
			if (!IsChecking ())
			{
				onComplete = _onComplete;
				#if USE_WEB_REQUEST
				www = UnityWebRequest.Get (versionLink);
				www.SendWebRequest ();
				#else
				www = new WWW (versionLink);
				#endif
				EditorApplication.update += DoUpdateCheck;
			}
		}


		public static bool IsChecking ()
		{
			return (www != null);
		}


		private static void DoUpdateCheck ()
		{
			if (IsChecking ())
			{
				if (!www.isDone)
				{
					return;
				}

				try
				{
					#if USE_WEB_REQUEST
					if (string.IsNullOrEmpty (www.error) && !www.downloadHandler.text.Contains ("404"))
					{
						string newVersion = www.downloadHandler.text;
					#else
					if (string.IsNullOrEmpty (www.error) && !www.text.Contains ("404"))
					{
						string newVersion = www.text;
					#endif
						bool isNewer = CompareVersion (newVersion, AdventureCreator.version);
						if (isNewer)
						{
							if (onComplete != null)
							{
								onComplete.Invoke (true);
							}
							else
							{
								NeedUpdate (newVersion);
							}
						}
						else
						{
							if (onComplete != null)
							{
								onComplete.Invoke (false);
							}
							else
							{
								UpToDate ();
							}
						}
					}
					else
					{
						if (onComplete != null)
						{
							onComplete.Invoke (false);
						}
						else
						{
							OnFail ();
						}
					}
				}
				catch (System.Exception e)
				{
					if (onComplete != null)
					{
						onComplete.Invoke (false);
					}
					else
					{
						OnFail (e.ToString ());
					}
				}
				
				www = null;
			}

			EditorApplication.update -= DoUpdateCheck;
		}


		private static bool CompareVersion (string onlineVersion, string thisVersion)
		{
			int[] onlineVersionArray = VersionToArray (onlineVersion);
			int[] thisVersionArray = VersionToArray (thisVersion);

			if (onlineVersionArray.Length >= 2 && thisVersionArray.Length >= 2)
			{
				int onlineVersionInt = (onlineVersionArray[0] * 10000) + (onlineVersionArray[1] * 100) + onlineVersionArray[2];
				int thisVersionInt = (thisVersionArray[0] * 10000) + (thisVersionArray[1] * 100) + thisVersionArray[2];
				return onlineVersionInt > thisVersionInt;
			}
			return false;
		}


		private static int[] VersionToArray (string version)
		{
			string[] stringArray = version.Split ("."[0]);
			int[] intArray = new int[stringArray.Length];

			for (int i=0; i<stringArray.Length; i++)
			{
				int a = -1;
				int.TryParse (stringArray[i], out a);
				intArray[i] = a;
			}

			return intArray;
		}


		private static void NeedUpdate (string version)
		{
			if (EditorUtility.DisplayDialog (dialogHeader, "Update available!  New version: " + version + ". Download?", "OK", "Cancel"))
			{
				Application.OpenURL (Resource.assetLink);
			}
		}


		private static void UpToDate ()
		{
			EditorUtility.DisplayDialog (dialogHeader, "You're up to date!", "OK");
		}


		private static void OnFail (string error = "")
		{
			string errorMessage = "Failed to connect to server.";
			if (!string.IsNullOrEmpty (error))
			{
				errorMessage += "\n" + error;
			}

			EditorUtility.DisplayDialog (dialogHeader, errorMessage, "OK");
		}

	}
}


#endif