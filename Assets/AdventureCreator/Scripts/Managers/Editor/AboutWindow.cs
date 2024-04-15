#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace AC
{

	[InitializeOnLoad]
	public static class AboutWindowLauncher
	{

		static AboutWindowLauncher ()
		{
			EditorApplication.update += RunOnce;
		}


		static void RunOnce ()
		{
			EditorApplication.update -= RunOnce;
			AboutWindow.AttemptAutoOpen ();
		}

	}


	public class AboutWindow : EditorWindow
	{

		private static AboutWindow window;
		private const string HasAttemptedAutoOpenKey = "AdventureCreator_HasAttemptedAutoOpenAboutWindow";
		private const string AutoOpenKey = "AdventureCreator_AutoOpenAboutWindow";

		private bool updateIsAvailable;
		private ManagerPackage package3DDemo;
		private ManagerPackage package2DDemo;

		private const int WindowWidth = 400;
		private const int WindowHeight = 510;

		private const int LogoWidth = 256;
		private const int LogoHeight = 128;
		
		private const int LogoY = 25;
		private const int ButtonWidth = 250;
		private const int ButtonHeight = 30;


		public static void AttemptAutoOpen ()
		{
			if (SessionState.GetBool (HasAttemptedAutoOpenKey, false))
			{
				return;
			}

			SessionState.SetBool (HasAttemptedAutoOpenKey, true);

			if (EditorPrefs.GetBool (AutoOpenKey, true))
			{
				Init ();
			}
		}


		[MenuItem ("Adventure Creator/About", false, 20)]
		public static void Init ()
		{
			if (window != null)
			{
				return;
			}

			window = EditorWindow.GetWindowWithRect <AboutWindow> (new Rect (0, 0, WindowWidth, WindowHeight), true, "Adventure Creator", true);
			window.titleContent.text = "Adventure Creator";

			window.package2DDemo = AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/2D Demo/ManagerPackage.asset", typeof (ManagerPackage)) as ManagerPackage;
			window.package3DDemo = AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Demo/ManagerPackage.asset", typeof (ManagerPackage)) as ManagerPackage;

			UpdateChecker.CheckForUpdate (new System.Action<bool> (OnCompleteUpdateCheck));
		}


		private static void OnCompleteUpdateCheck (bool updateAvailable)
		{
			if (window != null)
			{
				window.updateIsAvailable = updateAvailable;
			}
		}


		private Rect GetCentredRect (int y, int width, int height)
		{
			float x = (WindowWidth - width) * 0.5f;
			return new Rect (x, y, width, height);
		}


		private void OnGUI ()
		{
			GUILayout.BeginVertical (CustomStyles.thinBox, GUILayout.ExpandWidth (true), GUILayout.ExpandHeight (true));

			if (Resource.ACLogo)
			{
				GUI.DrawTexture (GetCentredRect (25, LogoWidth, LogoHeight), Resource.ACLogo);
			}
			else
			{
				GUILayout.Label ("Adventure Creator",  CustomStyles.managerHeader);
			}

			string updateText = "v" + AdventureCreator.version;
			if (updateIsAvailable) updateText += " - update avaiable!";
			GUI.Label (new Rect (0, LogoY + LogoHeight + 10, WindowWidth, 20), updateText,  CustomStyles.smallCentre);
		
			if (!ACInstaller.IsInstalled ())
			{
				if (GUI.Button (GetCentredRect (200, ButtonWidth, ButtonHeight), "Auto-configure Unity project settings"))
				{
					ACInstaller.DoInstall ();
				}
			}
			else
			{
				GUI.Label (GetCentredRect (200, WindowWidth, 20), "<b>Get started</b>", CustomStyles.smallCentre);
				if (GUI.Button (GetCentredRect (220, ButtonWidth, ButtonHeight), "New Game Wizard"))
				{
					NewGameWizardWindow.Init ();
				}

				GUI.Label (GetCentredRect (260, WindowWidth, 20), "<b>Showcase</b>",  CustomStyles.smallCentre);
				if (package2DDemo && GUI.Button (new Rect (75, 280, ButtonWidth * 0.5f - 2, ButtonHeight), "2D Demo"))
				{
					AdventureCreator.RefreshActions ();

					if (!ACInstaller.IsInstalled ())
					{
						ACInstaller.DoInstall ();
					}

					if (UnityVersionHandler.GetCurrentSceneName () != "Park")
					{
						if (UnityVersionHandler.SaveSceneIfUserWants ())
						{
							UnityEditor.SceneManagement.EditorSceneManager.OpenScene ("Assets/AdventureCreator/2D Demo/Scenes/Park.unity");
						}
					}

					AdventureCreator.Init ();
				}
				if (package3DDemo && GUI.Button (new Rect (WindowWidth * 0.5f + 2, 280, ButtonWidth * 0.5f - 2, ButtonHeight), "3D Demo"))
				{
					AdventureCreator.RefreshActions ();

					if (!ACInstaller.IsInstalled ())
					{
						ACInstaller.DoInstall ();
					}

					if (UnityVersionHandler.GetCurrentSceneName () != "Basement")
					{
						if (UnityVersionHandler.SaveSceneIfUserWants ())
						{
							UnityEditor.SceneManagement.EditorSceneManager.OpenScene ("Assets/AdventureCreator/Demo/Scenes/Basement.unity");
						}
					}

					AdventureCreator.Init ();
				}
			}
			
			GUI.enabled = true;

			GUI.Label (GetCentredRect (320, WindowWidth, 20), "<b>Resources</b>",  CustomStyles.smallCentre);
			if (GUI.Button (GetCentredRect (340, ButtonWidth, 30), "Tutorials"))
			{
				Application.OpenURL (Resource.tutorialsLink);
			}

			if (GUI.Button (GetCentredRect (370, ButtonWidth, 30), "Manual"))
			{
				Application.OpenURL (System.Environment.CurrentDirectory + "/" + Resource.MainFolderPath + "/Manual.pdf");
			}

			if (GUI.Button (GetCentredRect (400, ButtonWidth, 30), "Downloads"))
			{
				Application.OpenURL (Resource.scriptingGuideLink);
			}

			if (GUI.Button (GetCentredRect (430, ButtonWidth, 30), "Scripting API"))
			{
				Application.OpenURL (Resource.scriptingGuideLink);
			}

			GUILayout.EndVertical ();

			bool autoOpen = EditorPrefs.GetBool (AutoOpenKey, true);
			autoOpen = EditorGUILayout.ToggleLeft ("Open this window at startup?", autoOpen);
			EditorPrefs.SetBool (AutoOpenKey, autoOpen);
		}

	}

}

#endif