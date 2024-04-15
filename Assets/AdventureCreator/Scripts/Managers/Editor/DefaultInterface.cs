#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AC
{

	public class DefaultInterface
	{

		#region PublicFunctions

		public static void Apply (NGWData data, string installPath, CursorManager cursorManager, MenuManager menuManager, SpeechManager speechManager)
		{	
			AssetDatabase.CreateFolder (installPath, "UI");
			installPath = installPath + "/UI";
			AssetDatabase.CreateFolder (installPath, "ActionLists");

			bool directControlMenus = data.InputMethod == InputMethod.KeyboardOrController;
			MenuSource menuSource = data.MenuSource;

			// Cursor
			foreach (CursorIcon defaultIcon in data.DefaultCursorManager.cursorIcons)
			{
				CursorIcon newIcon = new CursorIcon ();
				newIcon.Copy (defaultIcon, false);
				cursorManager.cursorIcons.Add (newIcon);
			}

			if (data.DefaultCursorManager.uiCursorPrefab && data.DefaultCursorManager.uiCursorPrefab.GetComponent<UnityUICursor> ())
			{
				UnityUICursor newUnityUICursor = Template.CopyAsset<UnityUICursor> (installPath, data.DefaultCursorManager.uiCursorPrefab.GetComponent<UnityUICursor> (), ".prefab");
				cursorManager.uiCursorPrefab = newUnityUICursor.gameObject;
			}

			CursorIconBase pointerIcon = new CursorIconBase ();
			pointerIcon.Copy (data.DefaultCursorManager.pointerIcon);
			cursorManager.pointerIcon = pointerIcon;

			cursorManager.lookCursor_ID = data.DefaultCursorManager.lookCursor_ID;
				
			cursorManager.allowMainCursor = true;
			cursorManager.allowInteractionCursor = true;

			// Menu
			menuManager.keyboardControlWhenPaused = directControlMenus;
			menuManager.keyboardControlWhenDialogOptions = directControlMenus;

			menuManager.drawOutlines = data.DefaultMenuManager.drawOutlines;
			menuManager.drawInEditor = data.DefaultMenuManager.drawInEditor;
			menuManager.pauseTexture = data.DefaultMenuManager.pauseTexture;

			Template.CopyMenus (installPath, data.DefaultMenuManager, menuManager, Template.ExistingMenuBehaviour.Delete);

			foreach (Menu menu in menuManager.menus)
			{
				menu.menuSource = menuSource;

				if (menu.pauseWhenEnabled || menu.appearType == AppearType.DuringConversation)
				{
					bool autoSelectUI = directControlMenus;
					menu.autoSelectFirstVisibleElement = autoSelectUI;
				}
			}

			string actionListPath = installPath + "/ActionLists";

			ActionListAsset asset_pauseGame = CreateActionList_PauseGame (actionListPath);
			ActiveInput activeInput = Template.CreateActiveInput ("Pause game", "Menu", FlagsGameState.Normal | FlagsGameState.Cutscene | FlagsGameState.DialogOptions, asset_pauseGame);
			
			ActionListAsset asset_quitButton = CreateActionList_QuitButton (actionListPath);
			ActionListAsset asset_setupPauseMenu = CreateActionList_SetupPauseMenu (actionListPath);
			Menu pauseMenu = menuManager.GetMenuWithName ("Pause");
			if (pauseMenu)
			{
				pauseMenu.actionListOnTurnOn = asset_setupPauseMenu;

				MenuElement quitElement = pauseMenu.GetElementWithName ("Quit");
				if (quitElement && quitElement is MenuButton)
				{
					MenuButton quitButton = quitElement as MenuButton;
					quitButton.actionList = asset_quitButton;
				}
			}

			ActionListAsset asset_createNewProfile = CreateActionList_CreateNewProfile (actionListPath);
			ActionListAsset asset_deleteActiveProfile = CreateActionList_DeleteActiveProfile (actionListPath);
			ActionListAsset asset_setupProfilesMenu = CreateActionList_SetupProfilesMenu (actionListPath);
			Menu profilesMenu = menuManager.GetMenuWithName ("Profiles");
			if (profilesMenu)
			{
				profilesMenu.actionListOnTurnOn = asset_setupProfilesMenu;

				MenuElement newElement = profilesMenu.GetElementWithName ("New");
				if (newElement && newElement is MenuButton)
				{
					MenuButton newButton = newElement as MenuButton;
					newButton.actionList = asset_createNewProfile;
				}

				MenuElement deleteElement = profilesMenu.GetElementWithName ("DeleteActiveProfile");
				if (deleteElement && deleteElement is MenuButton)
				{
					MenuButton deleteButton = deleteElement as MenuButton;
					deleteButton.actionList = asset_deleteActiveProfile;
				}
			}

			ActionListAsset asset_closeCrafting = CreateActionList_CloseCrafting (actionListPath);
			ActionListAsset asset_doCrafting = CreateActionList_DoCrafting (actionListPath);
			Menu craftingMenu = menuManager.GetMenuWithName ("Crafting");
			if (craftingMenu)
			{
				craftingMenu.actionListOnTurnOff = asset_closeCrafting;

				MenuElement createElement = craftingMenu.GetElementWithName ("Create");
				if (createElement && createElement is MenuButton)
				{
					MenuButton createButton = createElement as MenuButton;
					createButton.actionList = asset_doCrafting;
				}
			}

			ActionListAsset asset_hideSelectedObjective = CreateActionList_HideSelectedObjective (actionListPath);
			ActionListAsset asset_showSelectedObjective = CreateActionList_ShowSelectedObjective (actionListPath);
			Menu objectivesMenu = menuManager.GetMenuWithName ("Objectives");
			if (objectivesMenu)
			{
				objectivesMenu.actionListOnTurnOn = asset_hideSelectedObjective;

				MenuElement objectivesElement = objectivesMenu.GetElementWithName ("Objectives");
				if (objectivesElement && objectivesElement is MenuInventoryBox)
				{
					MenuInventoryBox objectivesList = objectivesElement as MenuInventoryBox;
					objectivesList.actionListOnClick = asset_showSelectedObjective;
				}
			}

			ActionListAsset asset_takeAllContainerItems = CreateActionList_TakeAllContainerItems (actionListPath);
			Menu containerMenu = menuManager.GetMenuWithName ("Container");
			if (containerMenu)
			{
				MenuElement takeElement = containerMenu.GetElementWithName ("TakeAll");
				if (takeElement && takeElement is MenuButton)
				{
					MenuButton takeButton = takeElement as MenuButton;
					takeButton.actionList = asset_takeAllContainerItems;
				}
			}

			// Speech
			if (speechManager)
			{
				speechManager.previewMenuName = "Subtitles";
			}
		}

		#endregion


		#region PrivateFunctions

		private static ActionListAsset CreateActionList_PauseGame (string installPath)
		{
			List<Action> actions = new List<Action>
			{
				ActionMenuState.CreateNew_TurnOnMenu ("Pause", false),
			};

			ActionListAsset newAsset = ActionListAsset.CreateFromActions ("ActiveInput_PauseGame", installPath, actions);
			newAsset.actionListType = ActionListType.RunInBackground;
			
			return newAsset;
		}


		private static ActionListAsset CreateActionList_CloseCrafting (string installPath)
		{
			List<Action> actions = new List<Action>
			{
				ActionInventoryCrafting.CreateNew (ActionInventoryCrafting.ActionCraftingMethod.ClearRecipe),
			};

			ActionListAsset newAsset = ActionListAsset.CreateFromActions ("ClearRecipe", installPath, actions);
			newAsset.actionListType = ActionListType.RunInBackground;
			
			return newAsset;
		}


		private static ActionListAsset CreateActionList_CreateNewProfile (string installPath)
		{
			List<Action> actions = new List<Action>
			{
				ActionManageProfiles.CreateNew_CreateProfile (),
			};

			ActionListAsset newAsset = ActionListAsset.CreateFromActions ("CreateNewProfile", installPath, actions);
			newAsset.actionListType = ActionListType.RunInBackground;

			return newAsset;
		}


		private static ActionListAsset CreateActionList_DeleteActiveProfile (string installPath)
		{
			List<Action> actions = new List<Action>
			{
				ActionManageProfiles.CreateNew_DeleteProfile (DeleteProfileType.ActiveProfile, string.Empty, string.Empty, 0),
			};

			ActionListAsset newAsset = ActionListAsset.CreateFromActions ("DeleteActiveProfile", installPath, actions);
			newAsset.actionListType = ActionListType.RunInBackground;

			return newAsset;
		}


		private static ActionListAsset CreateActionList_SetupPauseMenu (string installPath)
		{
			ActionInventorySelect deselectInventory = ActionInventorySelect.CreateNew_DeselectActive ();
			ActionSaveCheck saveCheck = ActionSaveCheck.CreateNew_IsSavingPossible ();
			ActionMenuState showSave = ActionMenuState.CreateNew_SetElementVisibility ("Pause", "Save", true);
			ActionMenuState hideSave = ActionMenuState.CreateNew_SetElementVisibility ("Pause", "Save", false);

			List<Action> actions = new List<Action>
			{
				deselectInventory,
				saveCheck,
				showSave,
				hideSave
			};

			deselectInventory.SetOutput (new ActionEnd (saveCheck));
			saveCheck.SetOutputs (new ActionEnd (showSave), new ActionEnd (hideSave));
			showSave.SetOutput (new ActionEnd (true));
			hideSave.SetOutput (new ActionEnd (true));

			ActionListAsset newAsset = ActionListAsset.CreateFromActions ("SetupPauseMenu", installPath, actions);
			newAsset.actionListType = ActionListType.RunInBackground;

			return newAsset;
		}


		private static ActionListAsset CreateActionList_DoCrafting (string installPath)
		{
			List<Action> actions = new List<Action>
			{
				ActionInventoryCrafting.CreateNew (ActionInventoryCrafting.ActionCraftingMethod.CreateRecipe),
			};

			ActionListAsset newAsset = ActionListAsset.CreateFromActions ("CreateRecipe", installPath, actions);
			newAsset.actionListType = ActionListType.RunInBackground;

			return newAsset;
		}


		private static ActionListAsset CreateActionList_HideSelectedObjective (string installPath)
		{
			List<Action> actions = new List<Action>
			{
				ActionMenuState.CreateNew_SetElementVisibility ("Objectives", "SelectedTitle", false),
				ActionMenuState.CreateNew_SetElementVisibility ("Objectives", "SelectedStateType", false),
				ActionMenuState.CreateNew_SetElementVisibility ("Objectives", "SelectedDescription", false),
				ActionMenuState.CreateNew_SetElementVisibility ("Objectives", "SelectedTexture", false),
			};

			ActionListAsset newAsset = ActionListAsset.CreateFromActions ("HideSelectedObjective", installPath, actions);
			newAsset.actionListType = ActionListType.RunInBackground;

			return newAsset;
		}


		private static ActionListAsset CreateActionList_QuitButton (string installPath)
		{
			List<Action> actions = new List<Action>
			{
				ActionEndGame.CreateNew_QuitGame (),
			};

			ActionListAsset newAsset = ActionListAsset.CreateFromActions ("QuitButton", installPath, actions);
			newAsset.actionListType = ActionListType.RunInBackground;

			return newAsset;
		}


		private static ActionListAsset CreateActionList_SetupProfilesMenu (string installPath)
		{
			ActionSaveCheck saveCheck1 = ActionSaveCheck.CreateNew_NumberOfProfiles (1, IntCondition.MoreThan);

			ActionMenuState hideDeleteButton = ActionMenuState.CreateNew_SetElementVisibility ("Profiles", "DeleteActiveProfile", false);

			ActionMenuState showDeleteButton = ActionMenuState.CreateNew_SetElementVisibility ("Profiles", "DeleteActiveProfile", true);
			ActionSaveCheck saveCheck2 = ActionSaveCheck.CreateNew_NumberOfProfiles (10, IntCondition.LessThan);

			ActionMenuState hideNewButton = ActionMenuState.CreateNew_SetElementVisibility ("Profiles", "New", false);

			ActionMenuState showNewButton = ActionMenuState.CreateNew_SetElementVisibility ("Profiles", "New", true);

			List<Action> actions = new List<Action>
			{
				saveCheck1,
				hideDeleteButton,
				showDeleteButton,
				saveCheck2,
				hideNewButton,
				showNewButton,
			};

			saveCheck1.SetOutputs (new ActionEnd (showDeleteButton), new ActionEnd (hideDeleteButton));
			hideDeleteButton.SetOutput (new ActionEnd (true));
			showDeleteButton.SetOutput (new ActionEnd (saveCheck2));
			saveCheck2.SetOutputs (new ActionEnd (showNewButton), new ActionEnd (hideNewButton));
			hideNewButton.SetOutput (new ActionEnd (true));
			showNewButton.SetOutput (new ActionEnd (true));

			ActionListAsset newAsset = ActionListAsset.CreateFromActions ("SetupProfilesMenu", installPath, actions);
			newAsset.actionListType = ActionListType.RunInBackground;

			return newAsset;
		}


		private static ActionListAsset CreateActionList_ShowSelectedObjective (string installPath)
		{
			List<Action> actions = new List<Action>
			{
				ActionMenuState.CreateNew_SetElementVisibility ("Objectives", "SelectedTitle", true),
				ActionMenuState.CreateNew_SetElementVisibility ("Objectives", "SelectedStateType", true),
				ActionMenuState.CreateNew_SetElementVisibility ("Objectives", "SelectedDescription", true),
				ActionMenuState.CreateNew_SetElementVisibility ("Objectives", "SelectedTexture", true),
			};

			ActionListAsset newAsset = ActionListAsset.CreateFromActions ("ShowSelectedObjective", installPath, actions);
			newAsset.actionListType = ActionListType.RunInBackground;

			return newAsset;
		}


		private static ActionListAsset CreateActionList_TakeAllContainerItems (string installPath)
		{
			List<Action> actions = new List<Action>
			{
				ActionContainerSet.CreateNew_RemoveAll (null, true),
				ActionMenuState.CreateNew_TurnOffMenu ("Crafting"),
			};

			ActionListAsset newAsset = ActionListAsset.CreateFromActions ("TakeAllContainerItems", installPath, actions);
			newAsset.actionListType = ActionListType.RunInBackground;

			return newAsset;
		}

		#endregion

	}

}

#endif