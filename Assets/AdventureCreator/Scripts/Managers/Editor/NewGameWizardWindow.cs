#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace AC
{

	public class NewGameWizardWindow : EditorWindow
	{

		#region Variables

		private PageType pageType, prevPageType;
		private enum PageType { Welcome, Path, GameName, Perspective, Movement, Input, Interface, Extras, Review, Complete, Error };

		private Vector2 scrollPosition = Vector2.zero;

		private int selectedTemplateIndex;
		private enum WizardPath { New, Modify };
		private WizardPath wizardPath;

		private NGWData data;

		private const int ScrollItemHeight = 20;
		public const int ScrollBoxWidth = 300;
		private const int ScrollBoxHeight = 300;
		public const int PreviewImageWidth = 275;
		public const int PreviewImageHeight = 180;
		public const int Padding = 80;

		private const int BottomButtonWidth = 140;
		private const int BottomButtonHeight = 40;
		
		private string errorText;

		private readonly List<Template> availableTemplates = new List<Template> ();
		private readonly List<Template> chosenTemplates = new List<Template> ();

		private string gameName = "My new game";
		private string projectName;

		#endregion


		#region Init

		[MenuItem ("Adventure Creator/Getting started/New Game wizard", false, -10)]
		public static void Init ()
		{
			NewGameWizardWindow window = EditorWindow.GetWindowWithRect <NewGameWizardWindow> (DefaultWindowRect, true, "New Game Wizard", true);
			window.titleContent.text = "New Game wizard";
			window.position = DefaultWindowRect;
			window.pageType = PageType.Welcome;
		}

		#endregion

		#region UnityStandards

		private void OnGUI ()
		{
			if (Event.current.type == EventType.Layout)
			{
				var rect = new Rect(0, 0, position.width, position.height);
				if (rect.Contains (Event.current.mousePosition))
				{
					Repaint ();
				}
			}

			switch (pageType)
			{
				case PageType.Welcome:
					ShowWelcomeGUI ();
					break;

				case PageType.Path:
					ShowPathGUI ();
					break;

				case PageType.GameName:
					ShowPageNumber (1);
					ShowGameNameGUI ();
					break;

				case PageType.Perspective:
					ShowPageNumber (2);
					ShowPerspectiveGUI ();
					break;

				case PageType.Movement:
					ShowPageNumber (3);
					ShowMovementGUI ();
					break;

				case PageType.Input:
					ShowPageNumber (4);
					ShowInputGUI ();
					break;

				case PageType.Interface:
					ShowPageNumber (5);
					ShowInterfaceGUI ();
					break;

				case PageType.Extras:
					if (wizardPath == WizardPath.New)
					{
						ShowPageNumber (6);
					}
					ShowExtrasGUI ();
					break;

				case PageType.Review:
					ShowPageNumber (7);
					ShowReviewGUI ();
					break;

				case PageType.Complete:
					ShowCompleteGUI ();
					break;

				case PageType.Error:
					ShowErrorGUI ();
					break;

				default:
					break;
			}
		}

		#endregion


		#region PrivateFunctions

		private void DrawHeader (string title, string description)
		{
			GUI.Label (new Rect (0, 30, position.width, 60), title, CustomStyles.managerHeader);
			GUI.Box (new Rect (Padding, 80, position.width - Padding - Padding, 0), "", CustomStyles.Header);
			GUI.Label (new Rect (Padding, 95, position.width - Padding - Padding, 40), description, LabelStyle);
		}


		private void PrevPage ()
		{
			int pageTypeInt = (int) pageType;
			pageTypeInt--;
			SetPageType ((PageType) pageTypeInt);
		}


		private void NextPage ()
		{
			int pageTypeInt = (int) pageType;
			pageTypeInt++;
			SetPageType ((PageType) pageTypeInt);
		}


		private bool ClickedBottomButton (float posX, string label)
		{
			return GUI.Button (new Rect (posX, position.height - BottomButtonHeight - 50, BottomButtonWidth, BottomButtonHeight), label, ButtonStyle);
		}


		private void ShowWelcomeGUI ()
		{
			if (Resource.ACLogo)
			{
				GUI.DrawTexture (new Rect ((position.width - 256) / 2, 40, 256, 128), Resource.ACLogo);
			}

			GUI.Label (new Rect (0, 200, position.width, 40), "Welcome to Adventure Creator!", CustomStyles.managerHeader);
			GUI.Label (new Rect (Padding, 260, position.width - (Padding * 2), 200), "The New Game Wizard can be used to generate your starting assets, to get you up and running as quickly as possible.\n\nThe assets and settings it creates aren't fixed, however: they can be amended at any time.", LabelStyle);

			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f, "Begin"))
			{
				SetPageType (PageType.Path);
			}
		}


		private void ShowPathGUI ()
		{
			DrawHeader ("Mode", "How would you like to use this wizard?");

			float boxWidth = position.width - (Padding * 2f);
			float textPadding = 20f;

			int customHeight = 155;
			GUI.Box (new Rect (Padding, customHeight + 45, boxWidth, 60), "", CustomStyles.Header);
			if (GUI.Button (new Rect (Padding, customHeight, boxWidth, 40), "New game", ButtonStyle))
			{
				wizardPath = WizardPath.New;				
				NextPage ();
			}
			GUI.Label (new Rect (Padding + textPadding, customHeight + 50, boxWidth - (textPadding * 2f), 40), "Create a set of Managers, tailored to your needs by answering a few questions.  Based on the answers, a series of optional add-ons may be suggested.", LabelStyle);

			int templateHeight = 360;
			GUI.Box (new Rect (Padding, templateHeight + 45, boxWidth, 60), "", CustomStyles.Header);
			if (GUI.Button (new Rect (Padding, templateHeight, boxWidth, 40), "Modify existing", ButtonStyle))
			{
				wizardPath = WizardPath.Modify;
				CheckValidForModify ();
			}
			GUI.Label (new Rect (Padding + textPadding, templateHeight + 50, boxWidth - (textPadding * 2f), 40), "Extend an existing project with optional add-ons.  It is recommended to back up your project first, as your Managers may be modified during installation.", LabelStyle);
		}


		private void ShowGameNameGUI ()
		{
			DrawHeader ("Game name", "Enter a name for your game.  This will be used for project filenames, as well as save-game data.");

			GUI.Box (new Rect (Padding, 160, position.width - Padding - Padding, 90), "", CustomStyles.Header);
			GUI.Label (new Rect (Padding + 40, 160, 120, 20), "Game name:", LabelStyle);
			gameName = GUI.TextField (new Rect (Padding + 20, 190, position.width - (Padding * 2f) - 40, 40), gameName, InputStyle);
			if (GUI.changed)
			{
				UpdateProjectName ();
			}

			GUI.Box (new Rect (Padding, 300, position.width - Padding - Padding, 90), "", CustomStyles.Header);
			GUI.Label (new Rect (Padding + 40, 300, 120, 20), "Install path:", LabelStyle);
			GUI.Label (new Rect (Padding + 20, 330, position.width - (Padding * 2f) - 40, 40), "/Assets/" + projectName, InputStyle);
			GUI.Label (new Rect (position.width - Padding - 50, 340, 40, 40), "", CustomStyles.FolderIcon);

			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f - 165, "Back"))
			{
				SetPageType (PageType.Path);
			}

			GUI.enabled = !string.IsNullOrEmpty (gameName);
			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f + 165, "Next"))
			{
				NextPage ();
			}
			GUI.enabled = true;
		}


		private void ShowPerspectiveGUI ()
		{
			DrawHeader ("Perspective", "What will the default camera perspective will be?  This can be overridden on a per-scene basis.");

			data.ShowPerspectiveGUI (position);

			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f - 165, "Back"))
			{
				PrevPage ();
			}

			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f + 165, "Next"))
			{
				NextPage ();
			}
			GUI.enabled = true;
		}


		private void ShowMovementGUI ()
		{
			DrawHeader ("Movement", "If your game features a Player character, how should they be moved around the scene?");

			data.ShowMovementGUI (position);

			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f - 165, "Back"))
			{
				PrevPage ();
			}

			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f + 165, "Next"))
			{
				NextPage ();
			}
			GUI.enabled = true;
		}


		private void ShowInputGUI ()
		{
			DrawHeader ("Input", "What is the primary input device used to play the game?");

			data.ShowInputGUI (position);

			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f - 165, "Back"))
			{
				PrevPage ();
			}

			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f + 165, "Next"))
			{
				NextPage ();
			}
			GUI.enabled = true;
		}


		private void ShowInterfaceGUI ()
		{
			DrawHeader ("Interactions", "Adventure games are all about interacting with Hotspots to get responses.  How should this be handled?");

			data.ShowInteractionGUI (position);

			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f - 165, "Back"))
			{
				PrevPage ();
			}

			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f + 165, "Next"))
			{
				NextPage ();
			}
			GUI.enabled = true;
		}


		private void ShowExtrasGUI ()
		{
			switch (wizardPath)
			{
				case WizardPath.New:
					DrawHeader ("Templates", "Templates extend your project with additional features and behaviour.  More can also be found on the Downloads page.");
					break;

				case WizardPath.Modify:
					DrawHeader ("Templates", "The following Templates can be applied.  Templates can also be found on the AC website's Downloads page.  It is recommended to back up your project first.");
					break;
			}

			int numTemplates = availableTemplates.Count;
			int totalScrollViewHeight = 30 * numTemplates;

			GUI.Box (new Rect (Padding, 160, 315, totalScrollViewHeight + 40), "", CustomStyles.Header);

			scrollPosition = GUI.BeginScrollView (new Rect (Padding + 10, 180, 295, 280), scrollPosition, new Rect (0, 0, ScrollBoxWidth - 20, totalScrollViewHeight));

			string[] templateLabels = new string[numTemplates];
			for (int i = 0; i < templateLabels.Length; i++)
			{
				templateLabels[i] = availableTemplates[i].Label;
			}

			for (int i = 0; i < numTemplates; i++)
			{
				Template template = availableTemplates[i];

				string errorText = string.Empty;
				bool canInstall = CanInstallTemplate (template) && template.MeetsDependencyRequirements ();
				if (!canInstall)
				{
					GUI.enabled = false;
				}

				bool isChosen = chosenTemplates.Contains (template);
				isChosen = GUI.Toggle (new Rect (5, i * 30, 30, 30), isChosen, "");
				GUI.enabled = true;

				if (isChosen && canInstall && !chosenTemplates.Contains (template))
				{
					chosenTemplates.Add (template);
				}
				else if (!isChosen && chosenTemplates.Contains (template))
				{
					chosenTemplates.Remove (template);
				}

				selectedTemplateIndex = GUI.SelectionGrid (new Rect (30, 0, 255, totalScrollViewHeight), selectedTemplateIndex, templateLabels, 1, ButtonStyle);

				//GUI.Label (new Rect (ScrollItemHeight + 10, 0, ScrollBoxWidth - (ScrollBoxHeight + 10 + ScrollItemHeight), ScrollItemHeight), subTemplateLabel);
			}

			GUI.EndScrollView ();
			
			if (selectedTemplateIndex < numTemplates)
			{
				Template template = availableTemplates[selectedTemplateIndex];
				ShowDetails (template);
			}

			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f - 165, "Back"))
			{
				if (wizardPath == WizardPath.Modify)
				{
					SetPageType (PageType.Path);
				}
				else
				{
					PrevPage ();
				}
			}

			
			if (wizardPath == WizardPath.Modify)
			{
				GUI.enabled = chosenTemplates.Count > 0;
				if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f + 165, "Create"))
				{
					Modify ();
				}
				GUI.enabled = true;
			}
			else
			{
				if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f + 165, "Next"))
				{
					NextPage ();
				}
			}
		}


		private void ShowReviewGUI ()
		{
			DrawHeader ("Review choices", "Your game files are ready to be created!  Before continuing, take a moment to check the values, as set by the options you've set.  You can amend them here, and at any time later.");

			if (wizardPath == WizardPath.New)
			{
				GUI.Box (new Rect (Padding, 160, position.width - Padding - Padding, 40), "", CustomStyles.Header);
				GUI.Label (new Rect (Padding + 20, 160, 120, 20), "Main settings:", LabelStyle);
				GUI.BeginGroup (new Rect (Padding, 205, position.width - Padding - Padding, 170));
				EditorGUIUtility.labelWidth = 180;
				gameName = EditorGUILayout.TextField (new GUIContent ("Game name:", "The name used for project folders and save-game files"), gameName);
				EditorGUIUtility.labelWidth = 0;
				data.ShowReviewGUI (position.width - Padding - Padding);
			}

			GUI.EndGroup ();

			int numTemplates = chosenTemplates.Count;
			int totalScrollViewHeight = ScrollItemHeight * numTemplates;

			GUI.Box (new Rect (Padding, 370, position.width - Padding - Padding, 40), "", CustomStyles.Header);
			GUI.Label (new Rect (Padding + 20, 370, 120, 20), "Templates", LabelStyle);
			GUI.BeginGroup (new Rect (Padding + 5, 385, position.width - Padding - Padding, 90));

			if (numTemplates > 0)
			{
				string templatesLabel = string.Empty;
				for (int i = 0; i < numTemplates; i++)
				{
					templatesLabel += chosenTemplates[i].Label;
					if (numTemplates > 0 && i < numTemplates - 1)
					{
						templatesLabel += ", ";
					}
					GUI.Label (new Rect (0, 0, position.width - (Padding * 2), 80), templatesLabel);
				}
			}
			else
			{
				GUI.Label (new Rect (0, 0, ScrollBoxWidth, 80), "- None");
			}
			GUI.EndGroup ();

			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f - 165, "Back"))
			{
				PrevPage ();
			}

			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f + 165, "Create"))
			{
				CreateFiles ();
			}
		}


		private void ShowCompleteGUI ()
		{
			switch (wizardPath)
			{
				case WizardPath.New:
					{
						DrawHeader ("Process complete", "Thanks for your patience - your game's Managers have now been generated!\n\nYou can find them loaded in the AC Game Editor window, which should now be open.  Each tab of this window controls a different aspect of your game - click through them to see what they do.\n\nYou can find your game files under 'Assets/" + projectName + "' in the Project window.  The ManagerPackage file in this directory can be double-clicked to quickly re-assign your Managers if they become unset.\n\nReady to get started?  Click below to learn how to use the Game Editor:");

						if (GUI.Button (new Rect ((position.width - 300) * 0.5f, 300, 300, BottomButtonHeight), "Tutorial: The Game Editor window", ButtonStyle))
						{
							Application.OpenURL ("https://www.adventurecreator.org/tutorials/game-editor-window");
						}
					}
					break;

				case WizardPath.Modify:
					{
						if (chosenTemplates.Count == 1)
						{
							DrawHeader ("Process complete", "The chosen Template was succesfully applied.");
						}
						else
						{
							DrawHeader ("Process complete", "The chosen Templates were succesfully applied.");
						}
					}
					break;

				default:
					break;
			}

			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f, "Close"))
			{
				Close ();
			}
		}

		private void ShowErrorGUI ()
		{
			DrawHeader ("Error", "The Wizard failed with the following error:");

			bool wordWrapBackup = GUI.skin.label.wordWrap;
			TextAnchor alignmentBackup = GUI.skin.label.alignment;
			GUI.skin.label.wordWrap = true;
			GUI.skin.label.alignment = TextAnchor.UpperLeft;
			GUI.Label (new Rect (Padding, 150, position.width - Padding - Padding, position.height - 100 - BottomButtonHeight - 150), errorText);
			wordWrapBackup = GUI.skin.label.wordWrap;
			GUI.skin.label.alignment = alignmentBackup;

			if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f, "Back"))
			{
				SetPageType (prevPageType);
			}
		}


		private void ShowDetails (Template template)
		{
			GUI.Box (new Rect (position.width - PreviewImageWidth - 35 - Padding, 160, PreviewImageWidth + 40, 320), "", CustomStyles.Header);

			if (template.PreviewTexture)
			{
				GUI.DrawTexture (new Rect (position.width - PreviewImageWidth - Padding - 15, 180, PreviewImageWidth, PreviewImageHeight), data.GetTemplateBackground (wizardPath == WizardPath.Modify), ScaleMode.StretchToFill);
				GUI.DrawTexture (new Rect (position.width - PreviewImageWidth - Padding - 15, 180, PreviewImageWidth, PreviewImageHeight), template.PreviewTexture, ScaleMode.StretchToFill);
			}
			GUI.Label (new Rect (position.width - PreviewImageWidth - Padding - 15, 380, PreviewImageWidth, 40), template.Label, CustomStyles.managerHeader);
			GUI.Label (new Rect (position.width - PreviewImageWidth - Padding - 15, 400, PreviewImageWidth, 80), template.PreviewText, LabelStyle);
		}


		private void UpdateProjectName ()
		{
			if (string.IsNullOrEmpty (gameName))
			{
				projectName = string.Empty;
				return;
			}

			projectName = gameName;
			while (projectName.Contains (" "))
			{
				int index = projectName.IndexOf (" ");
				if ((index + 1) < projectName.Length)
				{
					char c = projectName[index+1];
					c = char.ToUpper (c);
					projectName = projectName.Substring (0, index) + c + projectName.Substring (index + 2);
				}
				else
				{
					projectName = projectName.Substring (0, index);
				}
			}
				
			foreach (var c in Path.GetInvalidFileNameChars ()) 
			{ 
				projectName = projectName.Replace (c.ToString (), string.Empty); 
			}
		}


		private void SetPageType (PageType _pageType)
		{
			bool isForward = (int) _pageType > (int) pageType;

			UpdateProjectName ();
			EditorUtility.ClearProgressBar ();

			selectedTemplateIndex = 0;
			
			switch (_pageType)
			{
				case PageType.Perspective:
					GetDataAsset ();
					if (data)
					{
						data.RebuildPerspectiveOptions ();
					}
					break;

				case PageType.Movement:
					data.RebuildMovementOptions ();
					break;

				case PageType.Input:
					data.RebuildInputOptions ();
					break;

				case PageType.Interface:
					data.RebuildInteractionOptions ();
					break;
				
				case PageType.Extras:
					GetDataAsset ();
					if (data)
					{
						if (wizardPath == WizardPath.New)
						{
							data.PrepareReview ();
						}
						if (isForward)
						{
							UpdateAvailableTemplates ();
						}
						if (availableTemplates.Count == 0)
						{
							if (isForward)
							{
								if (wizardPath == WizardPath.Modify)
								{
									ThrowError ("No templates found!");
									return;
								}
								pageType = _pageType;
								NextPage ();
							}
							else
							{
								if (wizardPath == WizardPath.Modify)
								{
									SetPageType (PageType.Path);
									return;
								}
								pageType = _pageType;
								PrevPage ();
							}
							return;
						}
					}
					break;

				case PageType.Review:
					if (wizardPath == WizardPath.New)
					{
						data.PrepareReview ();
					}
					break;

				default:
					break;
			}

			prevPageType = pageType;
			pageType = _pageType;
		}


		private void CreateFiles ()
		{
			UpdateProjectName ();

			string managerPath = projectName + "/Managers";
			try
			{
				System.IO.Directory.CreateDirectory (Application.dataPath + "/" + managerPath);
			}
			catch (System.Exception e)
			{
				ThrowError ("Could not create directory: " + Application.dataPath + "/" + managerPath + ". Error: "+ e.ToString ());
				return;
			}

			try
			{
				ShowProgress (0);

				SceneManager newSceneManager = CustomAssetUtility.CreateAsset<SceneManager> ("SceneManager", managerPath);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/SceneManager.asset", projectName + "_SceneManager");

				ShowProgress (1);

				SettingsManager newSettingsManager = CustomAssetUtility.CreateAsset<SettingsManager> ("SettingsManager", managerPath);
				newSettingsManager.saveFileName = projectName;
				EditorUtility.SetDirty (newSettingsManager);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/SettingsManager.asset", projectName + "_SettingsManager");

				ShowProgress (2);

				ActionsManager newActionsManager = CustomAssetUtility.CreateAsset<ActionsManager> ("ActionsManager", managerPath);
				newActionsManager.defaultClass = -1;
				newActionsManager.defaultClassName = nameof (ActionPause);
				AdventureCreator.RefreshActions (newActionsManager);
				EditorUtility.SetDirty (newActionsManager);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/ActionsManager.asset", projectName + "_ActionsManager");

				ShowProgress (3);

				VariablesManager newVariablesManager = CustomAssetUtility.CreateAsset<VariablesManager> ("VariablesManager", managerPath);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/VariablesManager.asset", projectName + "_VariablesManager");

				ShowProgress (4);

				InventoryManager newInventoryManager = CustomAssetUtility.CreateAsset<InventoryManager> ("InventoryManager", managerPath);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/InventoryManager.asset", projectName + "_InventoryManager");

				ShowProgress (5);

				SpeechManager newSpeechManager = CustomAssetUtility.CreateAsset<SpeechManager> ("SpeechManager", managerPath);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/SpeechManager.asset", projectName + "_SpeechManager");
				newSpeechManager.ClearLanguages ();

				ShowProgress (6);

				CursorManager newCursorManager = CustomAssetUtility.CreateAsset<CursorManager> ("CursorManager", managerPath);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/CursorManager.asset", projectName + "_CursorManager");

				ShowProgress (7);

				MenuManager newMenuManager = CustomAssetUtility.CreateAsset<MenuManager> ("MenuManager", managerPath);
				AssetDatabase.RenameAsset ("Assets/" + managerPath + "/MenuManager.asset", projectName + "_MenuManager");

				ShowProgress (8);

				ManagerPackage newManagerPackage = CreateManagerPackage (projectName, newSceneManager, newSettingsManager, newActionsManager, newVariablesManager, newInventoryManager, newSpeechManager, newCursorManager, newMenuManager);

				string installPath = "Assets/" + projectName;
				data.Apply (installPath, newSettingsManager, newCursorManager, newMenuManager, newSpeechManager);

				for (int i = 0; i < chosenTemplates.Count; i++)
				{
					ShowProgress (9 + i);

					errorText = string.Empty;
					if (chosenTemplates[i].CanInstall (ref errorText))
					{
						ApplyTemplate (chosenTemplates[i]);
					}
					else
					{
						SetPageType (PageType.Error);
						return;
					}
				}

				AssetDatabase.SaveAssets ();
				EditorUtility.ClearProgressBar ();

				if (!string.IsNullOrEmpty (errorText))
				{
					ThrowError (errorText);
				}
				else
				{
					EditorGUIUtility.PingObject (newManagerPackage);
					SetPageType (PageType.Complete);
				}
			}
			catch (System.Exception e)
			{
				errorText = e.ToString ();
				ThrowError (e.ToString ());
			}
		}


		private void ApplyTemplate (Template template)
		{
			string installPath = "";
			if (template.RequiresInstallPath)
			{
				AssetDatabase.CreateFolder ("Assets/" + projectName, template.FolderName);
				installPath = "Assets/" + projectName + "/" + template.FolderName;
			}

			string scenePath = installPath;
			template.Apply (installPath, scenePath, wizardPath == WizardPath.New, OnFailApplyTemplate);
		}


		private void OnFailApplyTemplate (string _errorText)
		{
			errorText = _errorText;
		}


		private ManagerPackage CreateManagerPackage (string folder, SceneManager sceneManager, SettingsManager settingsManager, ActionsManager actionsManager, VariablesManager variablesManager, InventoryManager inventoryManager, SpeechManager speechManager, CursorManager cursorManager, MenuManager menuManager)
		{
			ManagerPackage managerPackage = CustomAssetUtility.CreateAsset<ManagerPackage> ("ManagerPackage", folder);
			AssetDatabase.RenameAsset ("Assets/" + folder + "/ManagerPackage.asset", folder + "_ManagerPackage");

			managerPackage.sceneManager = sceneManager;
			managerPackage.settingsManager = settingsManager;
			managerPackage.actionsManager = actionsManager;
			managerPackage.variablesManager = variablesManager;

			managerPackage.inventoryManager = inventoryManager;
			managerPackage.speechManager = speechManager;
			managerPackage.cursorManager = cursorManager;
			managerPackage.menuManager = menuManager;

			managerPackage.AssignManagers ();
			EditorUtility.SetDirty (managerPackage);
			AssetDatabase.SaveAssets ();
			EditorGUIUtility.PingObject (managerPackage);

			AdventureCreator.Init ();
			
			return managerPackage;
		}


		private void ShowProgress (int progressPortion)
		{
			int numProgressSections = chosenTemplates.Count;
			if (wizardPath == WizardPath.New)
			{
				numProgressSections += 8;
			}
			
			float progress = (float) progressPortion / (float) numProgressSections;
			progress = Mathf.Clamp01 (progress);

			EditorUtility.DisplayProgressBar ("Preparing game", "Please wait while your asset files are created.", progress);
		}


		private void GetDataAsset ()
		{
			string[] guids = AssetDatabase.FindAssets ("t:NGWData");
			if (guids == null || guids.Length == 0)
			{
				ThrowError ("Cannot find NGWData file");
				return;
			}

			foreach (string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath (guid);
				data = (NGWData) AssetDatabase.LoadAssetAtPath (path, typeof (NGWData));
				if (data) return;
			}

			if (data == null)
			{
				ThrowError ("Cannot find NGWData file");
			}
		}


		private void ThrowError (string _errorText)
		{
			errorText = _errorText;
			SetPageType (PageType.Error);
		}


		private void UpdateAvailableTemplates ()
		{
			EditorUtility.DisplayProgressBar ("Gathering templates", "Please wait while your project is searched for Templates.", 1f);

			availableTemplates.Clear ();
			chosenTemplates.Clear ();

			string[] guids = AssetDatabase.FindAssets ("t:AC.Template");
			foreach (string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath (guid);
				Template template = (Template) AssetDatabase.LoadAssetAtPath (path, typeof (Template));

				if (template && template.Category != TemplateCategory.None)
				{
					switch (wizardPath)
					{
						case WizardPath.New:
							if (template.CanSuggest (data))
							{
								availableTemplates.Add (template);

								if (template.SelectedByDefault && CanInstallTemplate (template))
								{
									chosenTemplates.Add (template);
								}
							}
							break;

						case WizardPath.Modify:
							if (template)
							{
								availableTemplates.Add (template);
							}
							break;
					}
				}
			}

			availableTemplates.Sort (delegate (Template a, Template b) {return ((int) a.Category * 10000 + a.OrderInCategory).CompareTo ((int) b.Category * 10000 + b.OrderInCategory); });

			EditorUtility.ClearProgressBar ();
		}


		private bool CanInstallTemplate (Template template)
		{
			TemplateCategory templateCategory = template.Category;
			if (templateCategory != TemplateCategory.None)
			{
				foreach (Template chosenTemplate in chosenTemplates)
				{
					if (template == chosenTemplate) continue;

					if (chosenTemplate.Category == templateCategory && chosenTemplate.IsExclusiveToCategory)
					{
						return false;
					}
				}
			}
			
			return true;
		}


		private void Modify ()
		{
			try
			{
				string installPath = "Assets";

				string managerPath = AssetDatabase.GetAssetPath (Resource.References.settingsManager);
				string[] pathArray = managerPath.Split('/');
				if (pathArray.Length >= 3)
				{
					installPath = string.Empty;
					for (int i = 0; i < pathArray.Length - 2; i++)
					{
						installPath += pathArray[i];
						if (i < pathArray.Length - 3)
						{
							installPath += "/";
						}
					}
				}

				for (int i = 0; i < chosenTemplates.Count; i++)
				{
					ShowProgress (i);

					errorText = string.Empty;
					if (chosenTemplates[i].CanInstall (ref errorText))
					{
						string templateInstallPath = installPath;

						if (chosenTemplates[i].RequiresInstallPath)
						{
							AssetDatabase.CreateFolder (installPath, chosenTemplates[i].FolderName);
							templateInstallPath += "/" + chosenTemplates[i].FolderName;
						}

						chosenTemplates[i].Apply (templateInstallPath, templateInstallPath, wizardPath == WizardPath.New, OnFailApplyTemplate);
					}
					else
					{
						SetPageType (PageType.Error);
						return;
					}
				}

				AssetDatabase.SaveAssets ();
				EditorUtility.ClearProgressBar ();

				if (!string.IsNullOrEmpty (errorText))
				{
					ThrowError (errorText);
				}
				else
				{
					SetPageType (PageType.Complete);
				}
			}
			catch (System.Exception e)
			{
				errorText = e.ToString ();
				ThrowError (e.ToString ());
			}
		}


		private void CheckValidForModify ()
		{
			List<Type> missingManagerTypes = new List<Type> ();
			if (Resource.References.sceneManager == null) missingManagerTypes.Add (typeof (SceneManager));
			if (Resource.References.settingsManager == null) missingManagerTypes.Add (typeof (SettingsManager));
			if (Resource.References.actionsManager == null) missingManagerTypes.Add (typeof (ActionsManager));
			if (Resource.References.variablesManager == null) missingManagerTypes.Add (typeof (VariablesManager));
			if (Resource.References.inventoryManager == null) missingManagerTypes.Add (typeof (InventoryManager));
			if (Resource.References.cursorManager == null) missingManagerTypes.Add (typeof (CursorManager));
			if (Resource.References.speechManager == null) missingManagerTypes.Add (typeof (SpeechManager));
			if (Resource.References.menuManager == null) missingManagerTypes.Add (typeof (MenuManager));

			if (missingManagerTypes.Count == 0)
			{
				SetPageType (PageType.Extras);
				return;
			}

			string error = "A full set of Managers must be assigned before the New Game Wizard can modify an existing project.  The following Managers are unassigned in the Game Editor:\n\n";
			foreach (var managerType in missingManagerTypes)
			{
				error += "- " + managerType.Name + "\n";
			}
			ThrowError (error);
		}


		private void ShowPageNumber (int number)
		{
			Rect pageRect = new Rect (position.width - 90, position.height - 25, 90, 25);
			GUI.Label (pageRect, "Step " + number + " of " + 7);
		}

		#endregion


		#region GetSet

		private static Rect DefaultWindowRect { get { return new Rect (300, 200, 800, 600); }}

		private GUIStyle InputStyle { get { return Resource.NodeSkin.customStyles[37]; }}
		public static GUIStyle LabelStyle { get { return Resource.NodeSkin.customStyles[EditorGUIUtility.isProSkin ? 38 : 39]; }}
		public static GUIStyle ButtonStyle { get { return Resource.NodeSkin.customStyles[40]; }}

		#endregion

	}

}

#endif