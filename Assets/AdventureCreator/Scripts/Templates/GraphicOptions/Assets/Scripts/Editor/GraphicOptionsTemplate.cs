#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor.Animations;
using UnityEditor;

namespace AC.Templates.GraphicOptions
{

	public class GraphicOptionsTemplate : Template
	{

		#region Variables

		[SerializeField] private MenuManager menuManager = null;
		private const string EventName = "Apply graphic options";

		#endregion


		#region PublicFunctions

		public override bool CanInstall (ref string errorText)
		{
			if (Resource.References.settingsManager == null)
			{
				errorText = "No Settings Manager assigned";
				return false;
			}

			if (Resource.References.variablesManager == null)
			{
				errorText = "No Variables Manager assigned";
				return false;
			}

			if (menuManager == null || Resource.References.menuManager == null)
			{
				errorText = "No Menu Manager assigned";
				return false;
			}

			if (Resource.References.menuManager == menuManager)
			{
				errorText = "Wrong Menu Manager assigned";
				return false;
			}

			return true;
		}

		#endregion


		#region ProtectedFunctions

		protected override void MakeChanges (string installPath, bool canDeleteOldAssets, System.Action onComplete, System.Action<string> onFail)
		{
			Undo.RecordObjects (new UnityEngine.Object[] { Resource.References.settingsManager, Resource.References.variablesManager, Resource.References.menuManager }, "");

			// Variables
			GVar dataVar = GetOrCreateGlobalVariable ("GraphicOptionsData", VariableType.String);
			dataVar.link = VarLink.OptionsData;
			dataVar.canTranslate = false;

			// Menu
			CopyMenus (installPath, menuManager, Resource.References.menuManager, canDeleteOldAssets ? ExistingMenuBehaviour.Delete : ExistingMenuBehaviour.Rename);
			Menu graphicOptionsMenu = Resource.References.menuManager.GetMenuWithName ("GraphicOptions");
			if (graphicOptionsMenu == null || graphicOptionsMenu.PrefabCanvas == null)
			{
				onFail.Invoke ("No menu");
				return;
			}

			Menu optionsMenu = Resource.References.menuManager.GetMenuWithName ("Options");
			if (optionsMenu != null)
			{
				string backButtonName = "";
				if (optionsMenu.GetElementWithName ("Back"))
				{
					backButtonName = "Back";
				}
				else if (optionsMenu.GetElementWithName ("BackButton"))
				{
					backButtonName = "BackButton";
				}

				if (!string.IsNullOrEmpty (backButtonName))
				{
					if (canDeleteOldAssets || EditorUtility.DisplayDialog ("Update Options menu?", "Would you like the Options menu to be updated with an additional Button to access the Graphic Options menu?", "Yes", "No"))
					{
						MenuButton optionsButton = DuplicateButton (Resource.References.menuManager, "Options", backButtonName, "GraphicOptions");
						if (optionsButton != null)
						{
							optionsButton.label = "Graphic options";
							optionsButton.buttonClickType = AC_ButtonClickType.Crossfade;
							optionsButton.switchMenuTitle = graphicOptionsMenu.title;
							EditorUtility.SetDirty (Resource.References.menuManager);
							AssetDatabase.SaveAssets ();
						}
					}
				}
			}

			ActionListAsset spawnClickPrefabActionList = CreateActionList ("ApplyGraphicOptions", installPath, graphicOptionsMenu.PrefabCanvas.gameObject);

			EventBeginGame newEvent = new EventBeginGame (Resource.References.settingsManager. GetNextAvailableEventID (), EventName, spawnClickPrefabActionList, new int[] { 0 });
			Resource.References.settingsManager.events.Add (newEvent);

			EditorUtility.SetDirty (Resource.References.settingsManager);

			onComplete.Invoke ();
		}

		#endregion


		#region PrivateFunctions
		
		private ActionListAsset CreateActionList (string assetName, string installPath, GameObject uiPrefab)
		{
			GraphicOptionsUI graphicOptionsUI = uiPrefab.GetComponent<GraphicOptionsUI> ();

			var unityEvent = new UnityEvent ();
			var methodDelegate = System.Delegate.CreateDelegate (typeof (UnityAction), graphicOptionsUI, "Apply") as UnityAction;
			UnityEditor.Events.UnityEventTools.AddPersistentListener (unityEvent, methodDelegate);

			List<Action> actions = new List<Action>
			{
				ActionEvent.CreateNew (unityEvent),
			};

			ActionListAsset newAsset = ActionListAsset.CreateFromActions (assetName, installPath, actions, ActionListType.RunInBackground);
			return newAsset;
		}

		#endregion


		#region GetSet

		public override string Label { get { return "Graphic options"; }}
		public override string PreviewText { get { return "Adds a Graphic Options menu, available from Options."; }}
		public override Type[] AffectedManagerTypes { get { return new Type[] { typeof (SettingsManager), typeof (VariablesManager), typeof (MenuManager) }; }}
		public override bool RequiresInstallPath { get { return true; }}
		public override string FolderName { get { return "GraphicOptions"; }}
		public override TemplateCategory Category { get { return TemplateCategory.Misc; }}

		#endregion

	}


	[CustomEditor (typeof (GraphicOptionsTemplate))]
	public class GraphicOptionsTemplateEditor : TemplateEditor
	{}

}

#endif