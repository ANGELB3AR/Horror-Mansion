#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace AC
{

	public class AdventureCreator : EditorWindow
	{
		
		public const string version = "1.80.4";
	 
		private bool showScene = true;
		private bool showSettings = false;
		private bool showActions = false;
		private bool showGVars = false;
		private bool showInvItems = false;
		private bool showSpeech = false;
		private bool showCursor = false;
		private bool showMenu = false;
		
		private Vector2 scroll;
		

		[MenuItem ("Adventure Creator/Editors/Game Editor")]
		public static void Init ()
		{
			// Get existing open window or if none, make a new one:
			AdventureCreator window = (AdventureCreator) GetWindow (typeof (AdventureCreator));
			window.titleContent.text = "AC Game Editor";
		}


		private void OnEnable ()
		{
			RefreshActions ();
		}
		
		
		private void OnInspectorUpdate ()
		{
			Repaint ();
		}
		
		
		private void OnGUI ()
		{
			if (!ACInstaller.IsInstalled ())
			{
				ACInstaller.DoInstall ();
			}
			
			if (Resource.References)
			{
				GUILayout.Space (10);
				GUILayoutOption tabWidth = GUILayout.Width (this.position.width / 4f);

				GUILayout.BeginHorizontal ();
				
				if (GUILayout.Toggle (showScene, "Scene", "toolbarbutton", tabWidth))
				{
					SetTab (0);
				}
				if (GUILayout.Toggle (showSettings, "Settings", "toolbarbutton", tabWidth)) 
				{
					SetTab (1);
				}
				if (GUILayout.Toggle (showActions, "Actions", "toolbarbutton", tabWidth))
				{
					SetTab (2);
				}
				if (GUILayout.Toggle (showGVars, "Variables", "toolbarbutton", tabWidth))
				{
					SetTab (3);
				}
				
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				
				if (GUILayout.Toggle (showInvItems, "Inventory", "toolbarbutton", tabWidth))
				{
					SetTab (4);
				}
				if (GUILayout.Toggle (showSpeech, "Speech", "toolbarbutton", tabWidth))
				{
					SetTab (5);
				}
				if (GUILayout.Toggle (showCursor, "Cursor", "toolbarbutton", tabWidth))
				{
					SetTab (6);
				}
				if (GUILayout.Toggle (showMenu, "Menu", "toolbarbutton", tabWidth))
				{
					SetTab (7);
				}
		
				GUILayout.EndHorizontal ();
				GUILayout.Space (5);

				if (showScene)
				{
					GUILayout.Label ("Scene manager",  CustomStyles.managerHeader);

					if (KickStarter.sceneSettings && KickStarter.sceneSettings.requiredManagerPackage && KickStarter.sceneSettings.requiredManagerPackage.sceneManager)
					{
						GUI.enabled = false;
						EditorGUILayout.ObjectField ("Asset file: ", KickStarter.sceneManager, typeof (SceneManager), false);
						GUI.enabled = true;

						DrawManagerSpace ();
						if (Resource.References.sceneManager)
						{
							EditorGUILayout.HelpBox ("This scene has its own Scene Manager, shown below - close the scene to return to the global Scene Manager.", MessageType.Warning);
						}
						else
						{
							EditorGUILayout.HelpBox ("This scene has its own Scene Manager, shown below - to create your own, close the scene and run the New Game Wizard.", MessageType.Warning);
						}

						scroll = GUILayout.BeginScrollView (scroll);
						KickStarter.sceneManager.ShowGUI (this.position);
						GUILayout.EndScrollView ();
					}
					else
					{
						EditorGUI.BeginChangeCheck ();
						Resource.References.sceneManager = (SceneManager) EditorGUILayout.ObjectField ("Asset file: ", Resource.References.sceneManager, typeof (SceneManager), false);
						if (EditorGUI.EndChangeCheck ())
						{
							KickStarter.ClearManagerCache ();
						}
						DrawManagerSpace ();

						if (!Resource.References.sceneManager)
						{
							AskToCreate <SceneManager> ("Scene Manager");
						}
						else
						{
							scroll = GUILayout.BeginScrollView (scroll);
							Resource.References.sceneManager.ShowGUI (this.position);
							GUILayout.EndScrollView ();
						}
					}
				}
				
				else if (showSettings)
				{
					GUILayout.Label ("Settings manager",  CustomStyles.managerHeader);

					if (KickStarter.sceneSettings && KickStarter.sceneSettings.requiredManagerPackage && KickStarter.sceneSettings.requiredManagerPackage.settingsManager)
					{
						GUI.enabled = false;
						EditorGUILayout.ObjectField ("Asset file: ", KickStarter.settingsManager, typeof (SettingsManager), false);
						GUI.enabled = true;

						DrawManagerSpace ();
						if (Resource.References.settingsManager)
						{
							EditorGUILayout.HelpBox ("This scene has its own Settings Manager, shown below - close the scene to return to the global Settings Manager.", MessageType.Warning);
						}
						else
						{
							EditorGUILayout.HelpBox ("This scene has its own Settings Manager, shown below - to create your own, close the scene and run the New Game Wizard.", MessageType.Warning);
						}

						scroll = GUILayout.BeginScrollView (scroll);
						KickStarter.settingsManager.ShowGUI ();
						GUILayout.EndScrollView ();
					}
					else
					{
						EditorGUI.BeginChangeCheck ();
						Resource.References.settingsManager = (SettingsManager) EditorGUILayout.ObjectField ("Asset file: ", Resource.References.settingsManager, typeof (SettingsManager), false);
						if (EditorGUI.EndChangeCheck ())
						{
							KickStarter.ClearManagerCache ();
						}
						DrawManagerSpace ();

						if (!Resource.References.settingsManager)
						{
							AskToCreate <SettingsManager> ("Settings Manager");
						}
						else
						{
							scroll = GUILayout.BeginScrollView (scroll);
							Resource.References.settingsManager.ShowGUI ();
							GUILayout.EndScrollView ();
						}
					}
				}
				
				else if (showActions)
				{
					GUILayout.Label ("Actions manager",  CustomStyles.managerHeader);

					if (KickStarter.sceneSettings && KickStarter.sceneSettings.requiredManagerPackage && KickStarter.sceneSettings.requiredManagerPackage.actionsManager)
					{
						GUI.enabled = false;
						EditorGUILayout.ObjectField ("Asset file: ", KickStarter.actionsManager, typeof (ActionsManager), false);
						GUI.enabled = true;

						DrawManagerSpace ();
						if (Resource.References.actionsManager)
						{
							EditorGUILayout.HelpBox ("This scene has its own Actions Manager, shown below - close the scene to return to the global Actions Manager.", MessageType.Warning);
						}
						else
						{
							EditorGUILayout.HelpBox ("This scene has its own Actions Manager, shown below - to create your own, close the scene and run the New Game Wizard.", MessageType.Warning);
						}

						scroll = GUILayout.BeginScrollView (scroll);
						KickStarter.actionsManager.ShowGUI (this.position);
						GUILayout.EndScrollView ();
					}
					else
					{
						EditorGUI.BeginChangeCheck ();
						Resource.References.actionsManager = (ActionsManager) EditorGUILayout.ObjectField ("Asset file: ", Resource.References.actionsManager, typeof (ActionsManager), false);
						if (EditorGUI.EndChangeCheck ())
						{
							KickStarter.ClearManagerCache ();
						}
						DrawManagerSpace ();

						if (!Resource.References.actionsManager)
						{
							AskToCreate <ActionsManager> ("Actions Manager");
						}
						else
						{
							scroll = GUILayout.BeginScrollView (scroll);
							Resource.References.actionsManager.ShowGUI (this.position);
							GUILayout.EndScrollView ();
						}
					}
				}
				
				else if (showGVars)
				{
					GUILayout.Label ("Variables manager",  CustomStyles.managerHeader);

					if (KickStarter.sceneSettings && KickStarter.sceneSettings.requiredManagerPackage && KickStarter.sceneSettings.requiredManagerPackage.variablesManager)
					{
						GUI.enabled = false;
						EditorGUILayout.ObjectField ("Asset file: ", KickStarter.variablesManager, typeof (VariablesManager), false);
						GUI.enabled = true;

						DrawManagerSpace ();
						if (Resource.References.variablesManager)
						{
							EditorGUILayout.HelpBox ("This scene has its own Variables Manager, shown below - close the scene to return to the global Variables Manager.", MessageType.Warning);
						}
						else
						{
							EditorGUILayout.HelpBox ("This scene has its own Variables Manager, shown below - to create your own, close the scene and run the New Game Wizard.", MessageType.Warning);
						}

						scroll = GUILayout.BeginScrollView (scroll);
						KickStarter.variablesManager.ShowGUI ();
						GUILayout.EndScrollView ();
					}
					else
					{
						EditorGUI.BeginChangeCheck ();
						Resource.References.variablesManager = (VariablesManager) EditorGUILayout.ObjectField ("Asset file: ", Resource.References.variablesManager, typeof (VariablesManager), false);
						if (EditorGUI.EndChangeCheck ())
						{
							KickStarter.ClearManagerCache ();
						}
						DrawManagerSpace ();
						
						if (!Resource.References.variablesManager)
						{
							AskToCreate <VariablesManager> ("Variables Manager");
						}
						else
						{
							scroll = GUILayout.BeginScrollView (scroll);
							Resource.References.variablesManager.ShowGUI ();
							GUILayout.EndScrollView ();
						}
					}
				}
				
				else if (showInvItems)
				{
					GUILayout.Label ("Inventory manager",  CustomStyles.managerHeader);

					if (KickStarter.sceneSettings && KickStarter.sceneSettings.requiredManagerPackage && KickStarter.sceneSettings.requiredManagerPackage.inventoryManager)
					{
						GUI.enabled = false;
						EditorGUILayout.ObjectField ("Asset file: ", KickStarter.inventoryManager, typeof (InventoryManager), false);
						GUI.enabled = true;

						DrawManagerSpace ();
						if (Resource.References.inventoryManager)
						{
							EditorGUILayout.HelpBox ("This scene has its own Inventory Manager, shown below - close the scene to return to the global Inventory Manager.", MessageType.Warning);
						}
						else
						{
							EditorGUILayout.HelpBox ("This scene has its own Inventory Manager, shown below - to create your own, close the scene and run the New Game Wizard.", MessageType.Warning);
						}

						scroll = GUILayout.BeginScrollView (scroll);
						KickStarter.inventoryManager.ShowGUI (this.position);
						GUILayout.EndScrollView ();
					}
					else
					{
						EditorGUI.BeginChangeCheck ();
						Resource.References.inventoryManager = (InventoryManager) EditorGUILayout.ObjectField ("Asset file: ", Resource.References.inventoryManager, typeof (InventoryManager), false);
						if (EditorGUI.EndChangeCheck ())
						{
							KickStarter.ClearManagerCache ();
						}
						DrawManagerSpace ();

						if (!Resource.References.inventoryManager)
						{
							AskToCreate <InventoryManager> ("Inventory Manager");
						}
						else
						{
							scroll = GUILayout.BeginScrollView (scroll);
							Resource.References.inventoryManager.ShowGUI (this.position);
							GUILayout.EndScrollView ();
						}
					}
				}
				
				else if (showSpeech)
				{
					GUILayout.Label ("Speech manager",  CustomStyles.managerHeader);

					if (KickStarter.sceneSettings && KickStarter.sceneSettings.requiredManagerPackage && KickStarter.sceneSettings.requiredManagerPackage.speechManager)
					{
						GUI.enabled = false;
						EditorGUILayout.ObjectField ("Asset file: ", KickStarter.speechManager, typeof (SpeechManager), false);
						GUI.enabled = true;

						DrawManagerSpace ();
						if (Resource.References.speechManager)
						{
							EditorGUILayout.HelpBox ("This scene has its own Speech Manager, shown below - close the scene to return to the global Speech Manager.", MessageType.Warning);
						}
						else
						{
							EditorGUILayout.HelpBox ("This scene has its own Speech Manager, shown below - to create your own, close the scene and run the New Game Wizard.", MessageType.Warning);
						}

						scroll = GUILayout.BeginScrollView (scroll);
						KickStarter.speechManager.ShowGUI (this.position);
						GUILayout.EndScrollView ();
					}
					else
					{
						EditorGUI.BeginChangeCheck ();
						Resource.References.speechManager = (SpeechManager) EditorGUILayout.ObjectField ("Asset file: ", Resource.References.speechManager, typeof (SpeechManager), false);
						if (EditorGUI.EndChangeCheck ())
						{
							KickStarter.ClearManagerCache ();
						}
						DrawManagerSpace ();

						if (!Resource.References.speechManager)
						{
							AskToCreate <SpeechManager> ("Speech Manager");
						}
						else
						{
							scroll = GUILayout.BeginScrollView (scroll);
							Resource.References.speechManager.ShowGUI (this.position);
							GUILayout.EndScrollView ();
						}
					}
				}
				
				else if (showCursor)
				{
					GUILayout.Label ("Cursor manager",  CustomStyles.managerHeader);

					if (KickStarter.sceneSettings && KickStarter.sceneSettings.requiredManagerPackage && KickStarter.sceneSettings.requiredManagerPackage.cursorManager)
					{
						GUI.enabled = false;
						EditorGUILayout.ObjectField ("Asset file: ", KickStarter.cursorManager, typeof (CursorManager), false);
						GUI.enabled = true;

						DrawManagerSpace ();
						if (Resource.References.cursorManager)
						{
							EditorGUILayout.HelpBox ("This scene has its own Cursor Manager, shown below - close the scene to return to the global Cursor Manager.", MessageType.Warning);
						}
						else
						{
							EditorGUILayout.HelpBox ("This scene has its own Cursor Manager, shown below - to create your own, close the scene and run the New Game Wizard.", MessageType.Warning);
						}

						scroll = GUILayout.BeginScrollView (scroll);
						KickStarter.cursorManager.ShowGUI ();
						GUILayout.EndScrollView ();
					}
					else
					{
						EditorGUI.BeginChangeCheck ();
						Resource.References.cursorManager = (CursorManager) EditorGUILayout.ObjectField ("Asset file: ", Resource.References.cursorManager, typeof (CursorManager), false);
						if (EditorGUI.EndChangeCheck ())
						{
							KickStarter.ClearManagerCache ();
						}
						DrawManagerSpace ();

						if (!Resource.References.cursorManager)
						{
							AskToCreate <CursorManager> ("Cursor Manager");
						}
						else
						{
							scroll = GUILayout.BeginScrollView (scroll);
							Resource.References.cursorManager.ShowGUI ();
							GUILayout.EndScrollView ();
						}
					}
				}
				
				else if (showMenu)
				{
					GUILayout.Label ("Menu manager",  CustomStyles.managerHeader);

					if (KickStarter.sceneSettings && KickStarter.sceneSettings.requiredManagerPackage && KickStarter.sceneSettings.requiredManagerPackage.menuManager)
					{
						GUI.enabled = false;
						EditorGUILayout.ObjectField ("Asset file: ", KickStarter.menuManager, typeof (MenuManager), false);
						GUI.enabled = true;

						DrawManagerSpace ();
						if (Resource.References.menuManager)
						{
							EditorGUILayout.HelpBox ("This scene has its own Menu Manager, shown below - close the scene to return to the global Menu Manager.", MessageType.Warning);
						}
						else
						{
							EditorGUILayout.HelpBox ("This scene has its own Menu Manager, shown below - to create your own, close the scene and run the New Game Wizard.", MessageType.Warning);
						}

						scroll = GUILayout.BeginScrollView (scroll);
						KickStarter.menuManager.ShowGUI ();
						GUILayout.EndScrollView ();
					}
					else
					{
						EditorGUI.BeginChangeCheck ();
						Resource.References.menuManager = (MenuManager) EditorGUILayout.ObjectField ("Asset file: ", Resource.References.menuManager, typeof (MenuManager), false);
						if (EditorGUI.EndChangeCheck ())
						{
							KickStarter.ClearManagerCache ();
						}
						DrawManagerSpace ();

						if (!Resource.References.menuManager)
						{
							AskToCreate <MenuManager> ("Menu Manager");
						}
						else
						{
							scroll = GUILayout.BeginScrollView (scroll);
							Resource.References.menuManager.ShowGUI ();
							GUILayout.EndScrollView ();
						}
					}
				}

				Resource.References.viewingMenuManager = showMenu;

				EditorGUILayout.Separator ();
				GUILayout.Box (string.Empty, GUILayout.ExpandWidth (true), GUILayout.Height(1));
				GUILayout.Label ("Adventure Creator - Version " + AdventureCreator.version, EditorStyles.miniLabel);
			}
			else
			{
				MissingReferencesGUI ();
			}
			
			if (GUI.changed)
			{
				if (showActions)
				{
					RefreshActions ();
				}

				EditorUtility.SetDirty (this);
				if (Resource.References != null)
				{
					EditorUtility.SetDirty (Resource.References);
				}
			}
		}


		public static void MissingReferencesGUI ()
		{
			EditorStyles.label.wordWrap = true;
			GUILayout.Label ("Error - missing References",  CustomStyles.managerHeader);
			EditorGUILayout.Space ();
			EditorGUILayout.HelpBox ("A 'References' file must be present in the directory '" + Resource.DefaultReferencesPath + "' - please click to create one.", MessageType.Warning);

			if (GUILayout.Button ("Create 'References' file"))
			{
				CustomAssetUtility.CreateAsset<References> ("References", Resource.DefaultReferencesPath);
			}
		}


		private void DrawManagerSpace ()
		{
			EditorGUILayout.Space ();
			EditorGUILayout.Separator ();
			GUILayout.Box (string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(1));
		}
		
		
		private void SetTab (int tab)
		{
			showScene = false;
			showSettings = false;
			showActions = false;
			showGVars = false;
			showInvItems = false;
			showSpeech = false;
			showCursor = false;
			showMenu = false;
			
			if (tab == 0)
			{
				showScene = true;
			}
			else if (tab == 1)
			{
				showSettings = true;
			}
			else if (tab == 2)
			{
				showActions = true;
			}
			else if (tab == 3)
			{
				showGVars = true;
			}
			else if (tab == 4)
			{
				showInvItems = true;
			}
			else if (tab == 5)
			{
				showSpeech = true;
			}
			else if (tab == 6)
			{
				showCursor = true;
			}
			else if (tab == 7)
			{
				showMenu = true;
			}
		}
		
		
		private void AskToCreate<T> (string obName) where T : ScriptableObject
		{
			EditorStyles.label.wordWrap = true;
			EditorGUILayout.HelpBox ("A " + obName + " is required for AC games to run.  The New Game Wizard can be used to create your game's Managers.", MessageType.Info);
			EditorGUILayout.Space ();

			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Run the New Game Wizard", GUILayout.Height (30)))
			{
				NewGameWizardWindow.Init ();
			}

			bool hasAnyManagers = KickStarter.sceneManager || KickStarter.settingsManager || KickStarter.actionsManager || KickStarter.variablesManager || KickStarter.inventoryManager || KickStarter.speechManager || KickStarter.cursorManager || KickStarter.menuManager;

			if (hasAnyManagers && GUILayout.Button ("Create " + obName, GUILayout.Height (30)))
			{
				try
				{
					ScriptableObject t = CustomAssetUtility.CreateAsset<T> (obName);
					Undo.RecordObject (Resource.References, "Assign " + obName);
					
					if (t is SceneManager)
					{
						Resource.References.sceneManager = (SceneManager) t;
					}
					else if (t is SettingsManager)
					{
						Resource.References.settingsManager = (SettingsManager) t;
					}
					else if (t is ActionsManager)
					{
						Resource.References.actionsManager = (ActionsManager) t;
						RefreshActions ();
					}
					else if (t is VariablesManager)
					{
						Resource.References.variablesManager = (VariablesManager) t;
					}
					else if (t is InventoryManager)
					{
						Resource.References.inventoryManager = (InventoryManager) t;
					}
					else if (t is SpeechManager)
					{
						Resource.References.speechManager = (SpeechManager) t;
					}
					else if (t is CursorManager)
					{
						Resource.References.cursorManager = (CursorManager) t;
					}
					else if (t is MenuManager)
					{
						Resource.References.menuManager = (MenuManager) t;
					}
				}
				catch
				{
					ACDebug.LogWarning ("Could not create " + obName + ".");
				}
			}
			EditorGUILayout.EndHorizontal ();
		}


		public static void RefreshActions (ActionsManager actionsManager = null)
		{
			if (actionsManager == null)
			{
				actionsManager = KickStarter.actionsManager;
			}

			if (actionsManager == null)
			{
				return;
			}

			// Collect data to transfer
			List<ActionType> oldActionTypes = new List<ActionType>();
			foreach (ActionType actionType in actionsManager.AllActions)
			{
				oldActionTypes.Add (actionType);
			}

			actionsManager.AllActions.Clear ();

			// Load default Actions
			AddActionsFromFolder (actionsManager, actionsManager.FolderPath, oldActionTypes);

			for (int i=0; i<actionsManager.customFolderPaths.Count; i++)
			{
				string customFolderPath = actionsManager.customFolderPaths[i];

				// Discount duplicates
				bool ignoreMe = false;
				for (int j=0; j<i; j++)
				{
					if (actionsManager.customFolderPaths[j] == customFolderPath)
					{
						ignoreMe = true;
					}
				}

				if (ignoreMe) continue;

				if (!string.IsNullOrEmpty (customFolderPath) && actionsManager.FolderPath != ("Assets/" + customFolderPath))
				{
					try
					{
						AddActionsFromFolder (actionsManager, "Assets/" + customFolderPath, oldActionTypes);
					}
					catch (System.Exception e)
					{
						ACDebug.LogWarning ("Can't access directory " + "Assets/" + customFolderPath + " - does it exist?\n\nException: " + e);
					}
				}
			}
			
			actionsManager.AllActions.Sort (delegate(ActionType i1, ActionType i2) { return i1.GetFullTitle (true).CompareTo(i2.GetFullTitle (true)); });
		}


		private static void AddActionsFromFolder (ActionsManager actionsManager, string folderPath, List<ActionType> oldActionTypes)
		{
			DirectoryInfo dir = new DirectoryInfo (folderPath);

			if (!dir.Exists)
			{
				Debug.LogWarning ("Cannot add Actions from folder '" + folderPath + "', because the directory does not exist!");
				return;
			}

			FileInfo[] info = dir.GetFiles ("*.cs");
			foreach (FileInfo f in info)
			{
				if (f.Name.StartsWith ("._")) continue;

				try
				{
					MonoScript script = AssetDatabase.LoadAssetAtPath <MonoScript> (folderPath + "/" + f.Name);
					if (script == null) continue;

					if (script.GetClass () != null && script.GetClass ().BaseType != null && (script.GetClass ().BaseType == typeof (AC.Action) || script.GetClass ().BaseType.IsSubclassOf (typeof (AC.Action))))
					{
						#if AC_ActionListPrefabs
						System.Runtime.Remoting.ObjectHandle handle = System.Activator.CreateInstance ("Assembly-CSharp", "AC." + script.name);
						Action tempAction = (Action) handle.Unwrap();
						#else
						Action tempAction = (Action) CreateInstance (script.GetClass ());
						#endif

						if (tempAction == null) continue;
						ActionType newActionType = new ActionType (script.GetClass ().FullName, tempAction);
						
						// Transfer back data
						foreach (ActionType oldActionType in oldActionTypes)
						{
							if (newActionType.IsMatch (oldActionType))
							{
								newActionType.color = oldActionType.color;
								newActionType.isEnabled = oldActionType.isEnabled;
								if (newActionType.color == new Color (0f, 0f, 0f, 0f)) newActionType.color = Color.white;
								if (newActionType.color.a < 1f) newActionType.color = new Color (newActionType.color.r, newActionType.color.g, newActionType.color.b, 1f);
							}
						}
						
						actionsManager.AllActions.Add (newActionType);
					}
				}
				catch (System.Exception e)
				{
					MonoScript script = AssetDatabase.LoadAssetAtPath <MonoScript> (folderPath + "/" + f.Name);
					Debug.LogWarning ("Error loading Action " + f.Name + ". \nException: " + e, script);
				}
			}
		}

	}

}

#endif