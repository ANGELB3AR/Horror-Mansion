#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace AC
{


	public abstract class Template : ScriptableObject
	{

		#region Variables

		[SerializeField] protected SceneAsset[] exampleScenes = new SceneAsset[0];
		[SerializeField] private Texture2D previewTexture = null;

		#endregion


		#region PublicFunctions

		public void Apply (string installPath, bool recordUndo = true, bool reportCompletion = true, bool canDeleteOldAssets = false)
		{
			string errorText = "";
			if (!CanInstall (ref errorText))
			{
				ACDebug.LogWarning ("Error importing template " + Label + ": " + errorText);
				return;
			}

			ShowProgress (0f);

			int group = -1;
			if (recordUndo)
			{
				Undo.SetCurrentGroupName ("Apply " + Label + " template");
				group = Undo.GetCurrentGroup ();
			}
		
			MakeChanges (installPath, canDeleteOldAssets, () => OnComplete (installPath, reportCompletion, group), (string message) => OnFail (message, reportCompletion, group));
		}


		public void Apply (string installPath, string scenePath, bool canDeleteOldAssets, System.Action<string> onFail)
		{
			MakeChanges (installPath, canDeleteOldAssets, () => CreateExampleScenes (scenePath), onFail);
		}

		public abstract bool CanInstall (ref string errorText);

		public virtual bool CanSuggest (NGWData data) { return true; }

		public virtual bool MeetsDependencyRequirements () { return true; }

		#endregion


		#region ProtectedFunctions

		protected abstract void MakeChanges (string installPath, bool canDeleteOldAssets, System.Action onComplete, System.Action<string> onFail);


		protected void ShowProgress (float progress)
		{
			EditorUtility.DisplayProgressBar ("Applying '" + Label + "' template", "Please wait while the Template is being applied.", progress);
		}


		private void CreateExampleScenes (string installPath)
		{
			string sceneToOpen = "";
			foreach (SceneAsset exampleScene in exampleScenes)
			{
				if (exampleScene == null) continue;

				CopyAsset<SceneAsset> (installPath, exampleScene, ".unity");

				if (string.IsNullOrEmpty (sceneToOpen))
				{
					sceneToOpen = installPath + "/" + exampleScene.name + ".unity";
				}
			}
			
			if (!string.IsNullOrEmpty (sceneToOpen) && UnityVersionHandler.SaveSceneIfUserWants ())
			{
				UnityEditor.SceneManagement.EditorSceneManager.OpenScene (sceneToOpen);
			}
		}


		protected void OnComplete (string installPath, bool reportAsDialogue, int group)
		{
			// Example scenes
			string sceneToOpen = "";
			foreach (SceneAsset exampleScene in exampleScenes)
			{
				if (exampleScene)
					CopyAsset<SceneAsset> (installPath, exampleScene, ".unity");

				if (string.IsNullOrEmpty (sceneToOpen))
				{
					sceneToOpen = installPath + "/" + exampleScene.name + ".unity";
				}
			}

			EditorUtility.ClearProgressBar ();
			AssetDatabase.SaveAssets ();

			if (reportAsDialogue)
			{
				EditorUtility.DisplayDialog (Label + " template", "The template has been succesfully applied.", "OK");
			}
			
			if (RequiresInstallPath)
			{
				Debug.Log (Label + " template was succesfully applied in '" + installPath + "'.");
			}
			else
			{
				Debug.Log (Label + " template was succesfully applied.");
			}

			if (group >= 0)
			{
				Undo.CollapseUndoOperations (group);
			}

			if (reportAsDialogue && !string.IsNullOrEmpty (sceneToOpen))
			{
				if (EditorUtility.DisplayDialog (Label + " template", "This template has an example scene.  Would you like to open it?", "Yes", "No"))
				{
					if (UnityVersionHandler.SaveSceneIfUserWants ())
					{
						UnityEditor.SceneManagement.EditorSceneManager.OpenScene (sceneToOpen);
					}
				}
			}
		}


		protected void OnFail (string message, bool reportAsDialogue, int group)
		{
			EditorUtility.ClearProgressBar ();

			if (reportAsDialogue)
			{
				EditorUtility.DisplayDialog (Label + " template", "The template encountered an issue and could not be completed. Reason: " + message, "OK");
			}
			else
			{
				Debug.LogWarning (Label + " template could not be succesfully applied. Reason: " + message);
			}

			if (group >= 0)
			{
				Undo.CollapseUndoOperations (group);
			}
		}


		public enum ExistingMenuBehaviour { Ignore, Rename, Delete}
		public static bool CopyMenus (string installPath, MenuManager source, MenuManager destination, ExistingMenuBehaviour existingMenuBehaviour = ExistingMenuBehaviour.Rename)
		{
			foreach (Menu defaultMenu in source.menus)
			{
				Menu newMenu = ScriptableObject.CreateInstance <Menu>();
				newMenu.Copy (defaultMenu, true, true);
				newMenu.Recalculate ();

				if (defaultMenu.PrefabCanvas)
				{
					string oldPath = AssetDatabase.GetAssetPath (defaultMenu.PrefabCanvas.gameObject);
					string newPath = installPath + "/" + defaultMenu.PrefabCanvas.name + ".prefab";

					if (AssetDatabase.CopyAsset (oldPath, newPath))
					{
						AssetDatabase.ImportAsset (newPath);
						GameObject canvasObNewPrefab = (GameObject) AssetDatabase.LoadAssetAtPath (newPath, typeof (GameObject));
						newMenu.PrefabCanvas = canvasObNewPrefab.GetComponent <Canvas>();
					}
					else
					{
						newMenu.PrefabCanvas = null;
						return false;
					}
					newMenu.rectTransform = null;
				}

				foreach (MenuElement newElement in newMenu.elements)
				{
					if (newElement != null)
					{
						AssetDatabase.AddObjectToAsset (newElement, destination);
						newElement.hideFlags = HideFlags.HideInHierarchy;
					}
					else
					{
						return false;
					}
				}

				if (newMenu != null)
				{
					AssetDatabase.AddObjectToAsset (newMenu, destination);
					newMenu.hideFlags = HideFlags.HideInHierarchy;

					int menuID = newMenu.id;
					List<int> existingIDs = new List<int>();
					foreach (Menu menu in destination.menus)
					{
						if (menu == null) continue;
						existingIDs.Add (menu.ID);
					}

					existingIDs.Sort ();
					foreach (int _id in existingIDs)
					{
						if (menuID == _id)
						{
							menuID ++;
						}
					}
					newMenu.id = menuID;

					if (existingMenuBehaviour != ExistingMenuBehaviour.Ignore)
					{
						bool foundCopy = false;

						for (int i = 0; i < destination.menus.Count; i++)
						{
							if (destination.menus[i].title == defaultMenu.title)
							{
								Menu existingMenu = destination.menus[i];
								if (existingMenuBehaviour == ExistingMenuBehaviour.Rename)
								{
									existingMenu.isLocked = true;
									existingMenu.title += " (Old)";
									destination.menus.Insert (i, newMenu);
								}
								else if (existingMenuBehaviour == ExistingMenuBehaviour.Delete)
								{
									foreach (MenuElement element in existingMenu.elements)
									{
										if (element != null)
										{
											Undo.DestroyObjectImmediate (element);
										}
									}

									destination.menus.RemoveAt (i);
									Undo.DestroyObjectImmediate (existingMenu);
									
									destination.menus.Insert (i, newMenu);
								}
								foundCopy = true;
							}
						}

						if (!foundCopy)
						{
							destination.menus.Add (newMenu);
						}
						continue;
					}
					else
					{
						destination.menus.Add (newMenu);
					}
				}
				else
				{
					return false;
				}
			}
			return true;
		}


		protected void InstallActionsFolder (UnityEngine.Object folderObj)
		{
			string folderPath = AssetDatabase.GetAssetPath (folderObj);
			if (folderPath.StartsWith ("Assets/"))
			{
				folderPath = folderPath.Substring ("Assets/".Length);
			}
			
			if (!KickStarter.actionsManager.customFolderPaths.Contains (folderPath))
			{
				KickStarter.actionsManager.customFolderPaths[KickStarter.actionsManager.customFolderPaths.Count - 1] = folderPath;
				EditorUtility.SetDirty (KickStarter.actionsManager);
			}
		}


		protected void RemoveActionsFolder (UnityEngine.Object folderObj)
		{
			string folderPath = AssetDatabase.GetAssetPath (folderObj);
			if (folderPath.StartsWith ("Assets/"))
			{
				folderPath = folderPath.Substring ("Assets/".Length);
			}

			if (KickStarter.actionsManager.customFolderPaths.Contains (folderPath))
			{
				KickStarter.actionsManager.customFolderPaths.Remove (folderPath);
				EditorUtility.SetDirty (KickStarter.actionsManager);
			}
		}


		protected void RemoveExistingMenu (string menuName, bool hideOnly)
		{
			Menu menu = KickStarter.menuManager.GetMenuWithName (menuName);
			if (menu != null)
			{
				if (hideOnly)
				{
					menu.isLocked = true;
					menu.title += " (Old)";
				}
				else
				{
					foreach (MenuElement element in menu.elements)
					{
						if (element != null)
						{
							Undo.DestroyObjectImmediate (element);
						}
					}

					KickStarter.menuManager.menus.Remove (menu);
					Undo.DestroyObjectImmediate (menu);
					AssetDatabase.SaveAssets ();
					KickStarter.menuManager.CleanUpAsset ();
				}
			}
		}


		public static T CopyAsset<T> (string installPath, T original, string extension) where T : UnityEngine.Object
		{
			string newPath = installPath + "/" + original.name + extension;
			string oldPath = AssetDatabase.GetAssetPath (original);

			if (!AssetDatabase.CopyAsset (oldPath, newPath))
			{
				return null;
			}
			T newAsset = AssetDatabase.LoadAssetAtPath (newPath, typeof (T)) as T;
			return newAsset;
		}

		
		protected InvItem GetOrCreateItem (string label, Texture2D texture)
		{
			InvItem existingItem = KickStarter.inventoryManager.GetItem (label);
			if (existingItem != null)
			{
				return existingItem;
			}

			InvItem newItem = KickStarter.inventoryManager.CreateNewItem ();
			newItem.label = label;
			newItem.tex = texture;
			EditorUtility.SetDirty (KickStarter.inventoryManager);
			return newItem;
		}


		protected InvVar GetOrCreateItemProperty (string label, VariableType variableType)
		{
			InvVar existingProperty = KickStarter.inventoryManager.GetProperty (label);
			if (existingProperty != null && existingProperty.type == variableType)
			{
				return existingProperty;
			}

			InvVar newProperty = KickStarter.inventoryManager.CreateNewProperty ();
			newProperty.label = label;
			newProperty.type = variableType;
			EditorUtility.SetDirty (KickStarter.inventoryManager);
			return newProperty;
		}


		protected InvBin GetOrCreateItemCategory (string label)
		{
			InvBin existingCategory = KickStarter.inventoryManager.GetCategory (label);
			if (existingCategory != null)
			{
				return existingCategory;
			}

			InvBin newCategory = KickStarter.inventoryManager.CreateNewCategory ();
			newCategory.label = label;
			EditorUtility.SetDirty (KickStarter.inventoryManager);
			return newCategory;
		}


		protected GVar GetOrCreateGlobalVariable (string label, VariableType variableType)
		{
			GVar existingVar = KickStarter.variablesManager.GetVariable (label);
			if (existingVar != null && existingVar.type == variableType)
			{
				return existingVar;
			}

			GVar newVar = KickStarter.variablesManager.CreateNewVariable ();
			newVar.label = label;
			newVar.type = variableType;
			EditorUtility.SetDirty (KickStarter.variablesManager);
			return newVar;
		}


		public static ActiveInput CreateActiveInput (string label, string input, FlagsGameState gameStateFlags, ActionListAsset actionListAsset)
		{
			List<int> idArray = new List<int> ();
			foreach (ActiveInput activeInput in KickStarter.settingsManager.activeInputs)
			{
				idArray.Add (activeInput.ID);
			}
			idArray.Sort ();

			ActiveInput newActiveInput = new ActiveInput (idArray.ToArray ());
			newActiveInput.label = label;
			newActiveInput.inputName = input;
			newActiveInput.actionListAsset = actionListAsset;
			newActiveInput.gameStateFlags = gameStateFlags;

			KickStarter.settingsManager.activeInputs.Add (newActiveInput);

			return newActiveInput;
		}


		protected MenuButton DuplicateButton (MenuManager menuManager, string menuName, string elementName, string newName)
		{
			Menu menu = menuManager.GetMenuWithName (menuName);
			if (menu != null)
			{
				MenuButton originalButton = (MenuButton) menu.GetElementWithName (elementName);
				if (originalButton != null)
				{
					MenuButton newButton = (MenuButton) originalButton.DuplicateSelf (true, true);
					newButton.title = newName;

					if (menu.PrefabCanvas && originalButton.linkedUiID != 0)
					{
						GameObject canvasInstance = (GameObject) PrefabUtility.InstantiatePrefab (menu.PrefabCanvas.gameObject);
						UnityEngine.UI.Button originalButtonUI = Serializer.GetGameObjectComponent <UnityEngine.UI.Button> (originalButton.linkedUiID, canvasInstance);
						if (originalButtonUI)
						{
							GameObject newButtonOb = Instantiate (originalButtonUI.gameObject);
							newButtonOb.name = newName;
							newButtonOb.transform.SetParent (originalButtonUI.transform.parent);
							newButtonOb.GetComponent<RectTransform> ().localScale = Vector3.one;
							ConstantID newButtonConstantID = newButtonOb.GetComponent<ConstantID> ();
							newButtonConstantID.SetNewID_Prefab ();
							newButton.linkedUiID = newButtonConstantID.constantID;
							PrefabUtility.ApplyPrefabInstance (canvasInstance, InteractionMode.AutomatedAction);
						}

						DestroyImmediate (canvasInstance);
					}
					
					menu.elements.Add (newButton);
					menu.Recalculate ();

					newButton.hideFlags = HideFlags.HideInHierarchy;
					AssetDatabase.AddObjectToAsset (newButton, menu);
					AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (newButton));
					AssetDatabase.SaveAssets ();

					return newButton;
				}
			}
			
			return null;
		}

		#endregion


		#region PrivateFunctions

		private Type[] GetMissingManagerTypes ()
		{
			List<Type> missingTypes = new List<Type> ();
			foreach (Type type in AffectedManagerTypes)
			{
				if (type == typeof (SceneManager) && KickStarter.sceneManager == null) missingTypes.Add (typeof (SceneManager));
				if (type == typeof (SettingsManager) && KickStarter.settingsManager == null) missingTypes.Add (typeof (SettingsManager));
				if (type == typeof (ActionsManager) && KickStarter.actionsManager == null) missingTypes.Add (typeof (ActionsManager));
				if (type == typeof (VariablesManager) && KickStarter.variablesManager == null) missingTypes.Add (typeof (VariablesManager));
				if (type == typeof (InventoryManager) && KickStarter.inventoryManager == null) missingTypes.Add (typeof (InventoryManager));
				if (type == typeof (SpeechManager) && KickStarter.speechManager == null) missingTypes.Add (typeof (SpeechManager));
				if (type == typeof (CursorManager) && KickStarter.cursorManager == null) missingTypes.Add (typeof (CursorManager));
				if (type == typeof (MenuManager) && KickStarter.menuManager == null) missingTypes.Add (typeof (MenuManager));
			}
			return missingTypes.ToArray ();
		}

		#endregion


		#region GetSet

		public abstract string Label { get; }
		public virtual string FolderName { get { return Label.Replace (" ", "").Replace ("-", ""); }}
		public virtual string PreviewText { get { return "No description"; } }
		public Texture2D PreviewTexture { get { return previewTexture; } }
		public abstract Type[] AffectedManagerTypes { get; }
		public virtual bool RequiresInstallPath { get { return exampleScenes.Length > 0; }}
		public virtual bool SelectedByDefault { get { return false; }}
		public virtual TemplateCategory Category { get { return TemplateCategory.None; }}
		public virtual int OrderInCategory { get { return 0; }}
		public virtual bool IsExclusiveToCategory { get { return false; }}

		#endregion

	}


	[CustomEditor (typeof (Template))]
	public class TemplateEditor : Editor
	{

		public override void OnInspectorGUI ()
		{
			Template _target = (Template) target;

			CustomGUILayout.Header ("AC template: " + _target.Label);

			if (!string.IsNullOrEmpty (_target.PreviewText))
			{
				EditorGUILayout.Space ();
				CustomGUILayout.HelpBox (_target.PreviewText, MessageType.Info);
			}

			EditorGUILayout.Space ();

			EditorGUILayout.BeginHorizontal ();

			string errorText = "";
			bool meetsRequirements = _target.CanInstall (ref errorText);
			if (!meetsRequirements)
			{
				if (!string.IsNullOrEmpty (errorText))
				{
					EditorGUILayout.HelpBox ("Cannot install due to the following:\n- " + errorText, MessageType.Warning);
				}
				else
				{
					EditorGUILayout.HelpBox ("Cannot install right now", MessageType.Warning);
				}
			}
			else
			{
				if (GUILayout.Button ("Apply", GUILayout.Height (40f)))
				{
					string preface = "It is recommended to back up your project first.";

					if (_target.AffectedManagerTypes.Length > 0)
					{
						string managerList = "\n";
						foreach (Type type in _target.AffectedManagerTypes) managerList += "\n" + "- " + type.Name;
						preface = "This will make changes to the following Managers: " + managerList + "\n\n" + preface;
					}

					bool doApply = UnityEditor.EditorUtility.DisplayDialog ("Apply '" + _target.Label + "' template?", preface, "Yes", "No");
					if (doApply)
					{
						string installPath = "Assets/";
						if (_target.RequiresInstallPath)
						{
							if (KickStarter.settingsManager)
							{
								installPath = AssetDatabase.GetAssetPath (KickStarter.settingsManager);
								installPath = installPath.Substring (0, installPath.LastIndexOf ("/"));
								installPath = installPath.Substring (0, installPath.LastIndexOf ("/") + 1);
							}
							installPath = EditorUtility.SaveFolderPanel ("Choose a directory to install the template " + _target.Label + " into.", installPath, "");
							if (!installPath.Contains ("Assets/"))
							{
								return;
							}

							installPath = installPath.Substring (installPath.IndexOf ("Assets/"));
							if (string.IsNullOrEmpty (installPath))
							{
								return;
							}

							AssetDatabase.CreateFolder (installPath, _target.FolderName);
							installPath += "/" + _target.FolderName;
						}

						_target.Apply (installPath);
					}
				}
			}
			EditorGUILayout.EndHorizontal ();
		}


		protected override void OnHeaderGUI () {}

	}

}

#endif