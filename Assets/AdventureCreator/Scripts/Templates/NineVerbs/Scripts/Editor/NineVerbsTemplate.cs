#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AC.Templates.NineVerbs
{

	public class NineVerbsTemplate : Template
	{

		#region Variables

		[SerializeField] private CursorManager defaultCursorManager = null;
		[SerializeField] private MenuManager defaultMenuManager = null;

		#endregion


		#region PublicFunctions

		public override bool CanInstall (ref string errorText)
		{
			if (Resource.References.settingsManager == null)
			{
				errorText = "No Settings Manager assigned";
				return false;
			}

			if (Resource.References.menuManager == null || defaultMenuManager == null)
			{
				errorText = "No Menu Manager assigned";
				return false;
			}

			if (Resource.References.menuManager == defaultCursorManager)
			{
				errorText = "Wrong Menu Manager assigned";
				return false;
			}

			if (Resource.References.cursorManager == null || defaultCursorManager == null)
			{
				errorText = "No Cursor Manager assigned";
				return false;
			}

			if (Resource.References.cursorManager == defaultCursorManager)
			{
				errorText = "Wrong Cursor Manager assigned";
				return false;
			}

			return true;
		}


		public override bool CanSuggest (NGWData data)
		{
			return data.CameraPerspective == CameraPerspective.TwoD;
		}

		#endregion


		#region ProtectedFunctions

		protected override void MakeChanges (string installPath, bool canDeleteOldAssets, System.Action onComplete, System.Action<string> onFail)
		{
			Undo.RecordObjects (new UnityEngine.Object[] { Resource.References.settingsManager, Resource.References.cursorManager, Resource.References.menuManager, Resource.References.speechManager }, "");

			// Settings
			Resource.References.settingsManager.blackOutWhenInitialising = true;
			Resource.References.settingsManager.interactionMethod = AC_InteractionMethod.ChooseInteractionThenHotspot;
			Resource.References.settingsManager.autoCycleWhenInteract = true;
			Resource.References.settingsManager.allowDefaultinteractions = true;
			Resource.References.settingsManager.inventoryInteractions = InventoryInteractions.Multiple;
			Resource.References.settingsManager.allowDefaultInventoryInteractions = true;
			Resource.References.settingsManager.selectInvWithUnhandled = true;
			Resource.References.settingsManager.selectInvWithIconID = 0;
			Resource.References.settingsManager.giveInvWithUnhandled = true;
			Resource.References.settingsManager.giveInvWithIconID = 4;
			Resource.References.settingsManager.reverseInventoryCombinations = true;
			Resource.References.settingsManager.inventoryDisableDefined = true;
			Resource.References.settingsManager.inventoryDisableUnhandled = true;
			Resource.References.settingsManager.inventoryDisableLeft = true;
			Resource.References.settingsManager.inventoryActiveEffect = InventoryActiveEffect.Pulse;
			Resource.References.settingsManager.hotspotDetection = HotspotDetection.MouseOver;
			EditorUtility.SetDirty (Resource.References.settingsManager);

			// Cursor
			Resource.References.cursorManager.allowWalkCursor = false;
			Resource.References.cursorManager.syncWalkCursorWithInteraction = false;
			Resource.References.cursorManager.addWalkPrefix = true;
			Resource.References.cursorManager.inventoryHandling = InventoryHandling.ChangeHotspotLabel;
			Resource.References.cursorManager.onlyShowInventoryLabelOverHotspots = false;
			Resource.References.cursorManager.allowInteractionCursor = false;
			Resource.References.cursorManager.cycleCursors = false;
			Resource.References.cursorManager.allowIconInput = true;
			Resource.References.cursorManager.cursorIcons.Clear ();

			foreach (CursorIcon defaultIcon in defaultCursorManager.cursorIcons)
			{
				CursorIcon newIcon = new CursorIcon ();
				newIcon.Copy (defaultIcon, false);
				Resource.References.cursorManager.cursorIcons.Add (newIcon);
			}
			Resource.References.cursorManager.SyncCursorInteractions ();

			AssetDatabase.CreateFolder (installPath, "ActionLists");

			for (int i = 0; i < Resource.References.cursorManager.cursorIcons.Count; i++)
			{
				if (i >= Resource.References.cursorManager.unhandledCursorInteractions.Count) continue;

				string speechText = "";
				string iconLabel = Resource.References.cursorManager.cursorIcons[i].label;
				switch (iconLabel)
				{
					case "Use":
						speechText = "I can't use that.";
						break;
					
					case "Talk to":
						speechText = "There's no response.";
						break;

					case "Look at":
						speechText = "Doesn't look like anything to me.";
						break;

					case "Pick up":
						speechText = "I can't pick that up.";
						break;

					case "Give":
						speechText = "That doesn't need giving.";
						break;
					
					case "Open":
						speechText = "It doesn't seem to open.";
						break;

					case "Close":
						speechText = "It doesn't seem to close.";
						break;

					case "Pull":
						speechText = "I can't pull that.";
						break;

					case "Push":
						speechText = "I can't push that.";
						break;

					default:
						break;
				}

				if (!string.IsNullOrEmpty (speechText))
				{
					ActionListAsset unhandledActionList = CreateUnhandledInteractionActionList (iconLabel.Replace (" ", "") + "_Unhandled", installPath + "/ActionLists", speechText);
					Resource.References.cursorManager.unhandledCursorInteractions[i] = unhandledActionList;
				}
			}

			CursorIconBase pointerIcon = new CursorIconBase ();
			pointerIcon.Copy (defaultCursorManager.pointerIcon);
			Resource.References.cursorManager.pointerIcon = pointerIcon;

			Resource.References.cursorManager.addHotspotPrefix = true;
			CursorIconBase mouseOverIcon = new CursorIconBase ();
			mouseOverIcon.Copy (defaultCursorManager.mouseOverIcon);
			Resource.References.cursorManager.mouseOverIcon = mouseOverIcon;

			Resource.References.cursorManager.lookCursor_ID = defaultCursorManager.lookCursor_ID;
			EditorUtility.SetDirty (Resource.References.cursorManager);

			// Menu
			CopyMenus (installPath, defaultMenuManager, Resource.References.menuManager, canDeleteOldAssets ? ExistingMenuBehaviour.Delete : ExistingMenuBehaviour.Rename);

			RemoveExistingMenu ("Hotspot", !canDeleteOldAssets);
			RemoveExistingMenu ("InGame", !canDeleteOldAssets);
			RemoveExistingMenu ("Inventory", !canDeleteOldAssets);
			
			EditorUtility.SetDirty (Resource.References.menuManager);

			// Speech
			Resource.References.speechManager.scrollSubtitles = false;
			Resource.References.speechManager.scrollNarration = false;
			Resource.References.speechManager.displayForever = false;
			Resource.References.speechManager.displayNarrationForever = false;
			EditorUtility.SetDirty (Resource.References.speechManager);

			// Input
			AddInputButton ("Pause", "space");
			AddInputButton ("DefaultInteraction", "mouse 1");
			AddInputButton ("Icon_Give", "g");
			AddInputButton ("Icon_Open", "o");
			AddInputButton ("Icon_Close", "c");
			AddInputButton ("Icon_Pickup", "p");
			AddInputButton ("Icon_Talkto", "t");
			AddInputButton ("Icon_Lookat", "l");
			AddInputButton ("Icon_Use", "u");
			AddInputButton ("Icon_Push", "s");
			AddInputButton ("Icon_Pull", "y");
			
			onComplete.Invoke ();
		}

		#endregion


		#region PrivateFunctions

		private void AddInputButton (string name, string button)
		{
			ACInstaller.InputAxis defulatInteractionInputAxis = new ACInstaller.InputAxis ()
			{
				name = name,
				positiveButton = button,
			};
			ACInstaller.AddAxis (defulatInteractionInputAxis);
		}


		private ActionListAsset CreateUnhandledInteractionActionList (string assetName, string installPath, string speechText)
		{
			List<Action> actions = new List<Action>
			{
				ActionSpeech.CreateNew_Player (speechText),
			};

			ActionListAsset newAsset = ActionListAsset.CreateFromActions (assetName, installPath, actions, ActionListType.RunInBackground);
			return newAsset;
		}

		#endregion
		

		#region GetSet

		public override string Label { get { return "Nine Verbs interface"; }}
		public override string PreviewText { get { return "A classic 'Nine verbs' interface, inspired by the classic LucasArts adventure games of the 90s."; }}
		public override Type[] AffectedManagerTypes { get { return new Type[] { typeof (SettingsManager), typeof (CursorManager), typeof (MenuManager), typeof (SpeechManager) }; }}
		public override bool RequiresInstallPath { get { return true; }}
		public override string FolderName { get { return "NineVerbs"; }}
		public override TemplateCategory Category { get { return TemplateCategory.Interface; }}
		public override bool IsExclusiveToCategory { get { return true; }}

		#endregion

	}


	[CustomEditor (typeof (NineVerbsTemplate))]
	public class NineVerbsTemplateEditor : TemplateEditor
	{}

}

#endif