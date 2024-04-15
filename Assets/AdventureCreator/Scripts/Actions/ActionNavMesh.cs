/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionNavMesh.cs"
 * 
 *	Changes any of the following scene parameters: NavMesh, Default PlayerStart, Sorting Map, Tint Map, Cutscene On Load, and Cutscene On Start. When the NavMesh is a Polygon Collider, this Action can also be used to add or remove holes from it.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionNavMesh : Action
	{

		public int constantID = 0;
		public int parameterID = -1;

		public int replaceConstantID = 0;
		public int replaceParameterID = -1;

		public NavigationMesh newNavMesh;
		public SortingMap sortingMap;
		public PlayerStart playerStart;
		public Cutscene cutscene;
		public TintMap tintMap;
		public SceneSetting sceneSetting = SceneSetting.DefaultNavMesh;

		public ChangeNavMeshMethod changeNavMeshMethod = ChangeNavMeshMethod.ChangeNavMesh;
		public InvAction holeAction;

		public PolygonCollider2D hole;
		public PolygonCollider2D replaceHole;

		protected SceneSettings sceneSettings;

		protected NavigationMesh runtimeNewNavMesh;
		protected PolygonCollider2D runtimeHole;
		protected PolygonCollider2D runtimeReplaceHole;
		protected PlayerStart runtimePlayerStart;
		protected SortingMap runtimeSortingMap;
		protected TintMap runtimeTintMap;
		protected Cutscene runtimeCutscene;


		public override ActionCategory Category { get { return ActionCategory.Scene; }}
		public override string Title { get { return "Change setting"; }}
		public override string Description { get { return "Changes any of the following scene parameters: NavMesh, Default PlayerStart, Sorting Map, Tint Map, Cutscene On Load, and Cutscene On Start. When the NavMesh is a Polygon Collider, this Action can also be used to add or remove holes from it."; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			if (sceneSettings == null) return;

			switch (sceneSetting)
			{
				case SceneSetting.DefaultNavMesh:
					if ((KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider || KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.AStar2D) && changeNavMeshMethod == ChangeNavMeshMethod.ChangeNumberOfHoles)
					{
						runtimeHole = AssignFile <PolygonCollider2D> (parameters, parameterID, constantID, hole);
						runtimeReplaceHole = AssignFile <PolygonCollider2D> (parameters, replaceParameterID, replaceConstantID, replaceHole);
						runtimeNewNavMesh = null;
					}
					else
					{
						runtimeHole = null;
						runtimeReplaceHole = null;
						runtimeNewNavMesh = AssignFile <NavigationMesh> (parameters, parameterID, constantID, newNavMesh);
					}
					break;

				case SceneSetting.DefaultPlayerStart:
					runtimePlayerStart = AssignFile <PlayerStart> (parameters, parameterID, constantID, playerStart);
					break;

				case SceneSetting.SortingMap:
					runtimeSortingMap = AssignFile <SortingMap> (parameters, parameterID, constantID, sortingMap);
					break;

				case SceneSetting.TintMap:
					runtimeTintMap = AssignFile <TintMap> (parameters, parameterID, constantID, tintMap);
					break;
					
				case SceneSetting.OnLoadCutscene:
				case SceneSetting.OnStartCutscene:
					runtimeCutscene = AssignFile <Cutscene> (parameters, parameterID, constantID, cutscene);
					break;
			}
		}


		public override void AssignParentList (ActionList actionList)
		{
			if (actionList != null)
			{
				sceneSettings = UnityVersionHandler.GetSceneSettingsOfGameObject (actionList.gameObject);
			}
			if (sceneSettings == null)
			{
				sceneSettings = KickStarter.sceneSettings;
			}

			base.AssignParentList (actionList);
		}
		
		
		public override float Run ()
		{
			switch (sceneSetting)
			{
				case SceneSetting.DefaultNavMesh:
					if ((KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider || KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.AStar2D) && changeNavMeshMethod == ChangeNavMeshMethod.ChangeNumberOfHoles)
					{
						if (runtimeHole != null)
						{
							NavigationMesh currentNavMesh = sceneSettings.navMesh;

							switch (holeAction)
							{
								case InvAction.Add:
									currentNavMesh.AddHole (runtimeHole);
									break;

								case InvAction.Remove:
									currentNavMesh.RemoveHole (runtimeHole);
									break;

								case InvAction.Replace:
									currentNavMesh.AddHole (runtimeHole);
									currentNavMesh.RemoveHole (runtimeReplaceHole);
									break;
							}
						}
					}
					else if (runtimeNewNavMesh != null)
					{
						NavigationMesh oldNavMesh = sceneSettings.navMesh;
						oldNavMesh.TurnOff ();
						runtimeNewNavMesh.TurnOn ();
						sceneSettings.navMesh = runtimeNewNavMesh;

						// Bugfix: Need to cycle this otherwise weight caching doesn't always work
						runtimeNewNavMesh.TurnOff ();
						runtimeNewNavMesh.TurnOn ();

						if (runtimeNewNavMesh.GetComponent <ConstantID>() == null)
						{
							LogWarning ("Warning: Changing to new NavMesh with no ConstantID - change will not be recognised by saved games.", runtimeNewNavMesh);
						}
					}

					// Recalculate pathfinding characters
					foreach (Char _character in KickStarter.stateHandler.Characters)
					{
						_character.RecalculateActivePathfind ();
					}
					break;

				case SceneSetting.DefaultPlayerStart:
					if (runtimePlayerStart != null)
					{
						sceneSettings.defaultPlayerStart = runtimePlayerStart;

						if (runtimePlayerStart.GetComponent <ConstantID>() == null)
						{
							LogWarning ("Warning: Changing to new default PlayerStart with no ConstantID - change will not be recognised by saved games.", runtimePlayerStart);
						}
					}
					break;

				case SceneSetting.SortingMap:
					if (runtimeSortingMap != null)
					{
						sceneSettings.SetSortingMap (runtimeSortingMap);

						if (runtimeSortingMap.GetComponent <ConstantID>() == null)
						{
							LogWarning ("Warning: Changing to new SortingMap with no ConstantID - change will not be recognised by saved games.", runtimeSortingMap);
						}
					}
					break;

				case SceneSetting.TintMap:
					if (runtimeTintMap != null)
					{
						sceneSettings.SetTintMap (runtimeTintMap);
						
						if (runtimeTintMap.GetComponent <ConstantID>() == null)
						{
							LogWarning ("Warning: Changing to new TintMap with no ConstantID - change will not be recognised by saved games.", runtimeTintMap);
						}
					}
					break;

				case SceneSetting.OnLoadCutscene:
					if (runtimeCutscene != null)
					{
						sceneSettings.cutsceneOnLoad = runtimeCutscene;

						if (sceneSettings.actionListSource == ActionListSource.AssetFile)
						{
							LogWarning ("Warning: As the Scene Manager relies on asset files for its cutscenes, changes made with the 'Scene: Change setting' Action will not be felt.");
						}
						else if (runtimeCutscene.GetComponent <ConstantID>() == null)
						{
							LogWarning ("Warning: Changing to Cutscene On Load with no ConstantID - change will not be recognised by saved games.", runtimeCutscene);
						}
					}
					break;

				case SceneSetting.OnStartCutscene:
					if (runtimeCutscene != null)
					{
						sceneSettings.cutsceneOnStart = runtimeCutscene;

						if (sceneSettings.actionListSource == ActionListSource.AssetFile)
						{
							LogWarning ("Warning: As the Scene Manager relies on asset files for its cutscenes, changes made with the 'Scene: Change setting' Action will not be felt.");
						}
						else if (runtimeCutscene.GetComponent <ConstantID>() == null)
						{
							LogWarning ("Warning: Changing to Cutscene On Start with no ConstantID - change will not be recognised by saved games.", runtimeCutscene);
						}
					}
					break;

				default:
					break;
			}
			
			return 0f;
		}
		

		#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			if (sceneSettings == null)
			{
				EditorGUILayout.HelpBox ("No 'Scene Settings' component found in the scene. Please add an AC GameEngine object from the Scene Manager.", MessageType.Info);
				return;
			}

			sceneSetting = (SceneSetting) EditorGUILayout.EnumPopup ("Scene setting to change:", sceneSetting);

			if (sceneSetting == SceneSetting.DefaultNavMesh)
			{
				if (sceneSettings.navigationMethod == AC_NavigationMethod.meshCollider || sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider || sceneSettings.navigationMethod == AC_NavigationMethod.AStar2D || sceneSettings.navigationMethod == AC_NavigationMethod.Custom)
				{
					if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider || KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.AStar2D)
					{
						changeNavMeshMethod = (ChangeNavMeshMethod) EditorGUILayout.EnumPopup ("Change NavMesh method:", changeNavMeshMethod);
					}

					if (sceneSettings.navigationMethod == AC_NavigationMethod.meshCollider || changeNavMeshMethod == ChangeNavMeshMethod.ChangeNavMesh)
					{
						ComponentField ("New NavMesh:", ref newNavMesh, ref constantID, parameters, ref parameterID);
					}
					else if (changeNavMeshMethod == ChangeNavMeshMethod.ChangeNumberOfHoles)
					{
						holeAction = (InvAction) EditorGUILayout.EnumPopup ("Add or remove hole:", holeAction);
						string _label = "Hole to add:";
						if (holeAction == InvAction.Remove)
						{
							_label = "Hole to remove:";
						}

						ComponentField (_label, ref hole, ref constantID, parameters, ref parameterID);

						if (holeAction == InvAction.Replace)
						{
							ComponentField ("Hole to remove:", ref replaceHole, ref replaceConstantID, parameters, ref replaceParameterID);
						}
					}
				}
				else
				{
					EditorGUILayout.HelpBox ("This action is not compatible with the Unity Navigation pathfinding method, as set in the Scene Manager.", MessageType.Warning);
				}
			}
			else if (sceneSetting == SceneSetting.DefaultPlayerStart)
			{
				ComponentField ("New default PlayerStart:", ref playerStart, ref constantID, parameters, ref parameterID);
			}
			else if (sceneSetting == SceneSetting.SortingMap)
			{
				ComponentField ("New SortingMap:", ref sortingMap, ref constantID, parameters, ref parameterID);
			}
			else if (sceneSetting == SceneSetting.TintMap)
			{
				ComponentField ("New TintMap:", ref tintMap, ref constantID, parameters, ref parameterID);
			}
			else if (sceneSetting == SceneSetting.OnLoadCutscene)
			{
				ComponentField ("New OnLoad cutscene:", ref cutscene, ref constantID, parameters, ref parameterID);
			}
			else if (sceneSetting == SceneSetting.OnStartCutscene)
			{
				ComponentField ("New OnStart cutscene:", ref cutscene, ref constantID, parameters, ref parameterID);
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (sceneSetting == SceneSetting.DefaultNavMesh)
			{
				if ((KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider || KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.AStar2D) && changeNavMeshMethod == ChangeNavMeshMethod.ChangeNumberOfHoles)
				{
					if (saveScriptsToo)
					{
						if (KickStarter.sceneSettings != null && KickStarter.sceneSettings != null)
						{
							AddSaveScript <RememberNavMesh2D> (KickStarter.sceneSettings.navMesh);
						}
						AddSaveScript <ConstantID> (hole);
						AddSaveScript <ConstantID> (replaceHole);
					}
					constantID = AssignConstantID<PolygonCollider2D> (hole, constantID, parameterID);
					replaceConstantID = AssignConstantID<PolygonCollider2D> (replaceHole, replaceConstantID, replaceParameterID);
				}
				else
				{
					if (saveScriptsToo)
					{
						AddSaveScript <ConstantID> (newNavMesh);
					}
					constantID = AssignConstantID<NavigationMesh> (newNavMesh, constantID, parameterID);
				}
			}
			else if (sceneSetting == SceneSetting.DefaultPlayerStart)
			{
				if (saveScriptsToo)
				{
					AddSaveScript <ConstantID> (playerStart);
				}
				constantID = AssignConstantID<PlayerStart> (playerStart, constantID, parameterID);
			}
			else if (sceneSetting == SceneSetting.SortingMap)
			{
				if (saveScriptsToo)
				{
					AddSaveScript <ConstantID> (sortingMap);
				}
				constantID = AssignConstantID<SortingMap> (sortingMap, constantID, parameterID);
			}
			else if (sceneSetting == SceneSetting.TintMap)
			{
				if (saveScriptsToo)
				{
					AddSaveScript <ConstantID> (tintMap);
				}
				constantID = AssignConstantID<TintMap> (tintMap, constantID, parameterID);
			}
			else if (sceneSetting == SceneSetting.OnLoadCutscene || sceneSetting == SceneSetting.OnStartCutscene)
			{
				constantID = AssignConstantID<Cutscene> (cutscene, constantID, parameterID);
			}
		}
		
		
		public override string SetLabel ()
		{
			return sceneSetting.ToString ();
		}


		public override bool ReferencesObjectOrID (GameObject _gameObject, int id)
		{
			if (sceneSetting == SceneSetting.DefaultNavMesh && holeAction == InvAction.Replace && replaceParameterID < 0)
			{
				if (replaceHole && replaceHole.gameObject == _gameObject) return true;
				if (replaceConstantID == id) return true;
			}
			if (parameterID < 0)
			{
				if (sceneSetting == SceneSetting.DefaultNavMesh)
				{
					if (newNavMesh && newNavMesh.gameObject == _gameObject) return true;
					if (hole && hole.gameObject == _gameObject) return true;
					if (constantID == id) return true;
				}
				if (sceneSetting == SceneSetting.DefaultPlayerStart)
				{
					if (playerStart && playerStart.gameObject == _gameObject) return true;
					if (constantID == id) return true;
				}
				if (sceneSetting == SceneSetting.SortingMap)
				{
					if (sortingMap && sortingMap.gameObject == _gameObject) return true;
					if (constantID == id) return true;
				}
				if (sceneSetting == SceneSetting.TintMap)
				{
					if (tintMap && tintMap.gameObject == _gameObject) return true;
					if (constantID == id) return true;
				}
				if (sceneSetting == SceneSetting.OnLoadCutscene || sceneSetting == SceneSetting.OnStartCutscene)
				{
					if (cutscene && cutscene.gameObject == _gameObject) return true;
					if (constantID == id) return true;
				}
			}
			return base.ReferencesObjectOrID (_gameObject, id);
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Scene: Change setting' Action, set to change the default NavMesh</summary>
		 * <param name = "newNavMesh">The new NavMesh</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionNavMesh CreateNew_ChangeDefaultNavMesh (NavigationMesh newNavMesh)
		{
			ActionNavMesh newAction = CreateNew<ActionNavMesh> ();
			newAction.sceneSetting = SceneSetting.DefaultNavMesh;
			newAction.changeNavMeshMethod = ChangeNavMeshMethod.ChangeNavMesh;
			newAction.newNavMesh = newNavMesh;
			newAction.TryAssignConstantID (newAction.newNavMesh, ref newAction.constantID);
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Scene: Change setting' Action, set to add a new hole to the current NavMesh</summary>
		 * <param name = "holeToAdd">The hole to add</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionNavMesh CreateNew_AddNavMeshHole (PolygonCollider2D holeToAdd)
		{
			ActionNavMesh newAction = CreateNew<ActionNavMesh> ();
			newAction.sceneSetting = SceneSetting.DefaultNavMesh;
			newAction.changeNavMeshMethod = ChangeNavMeshMethod.ChangeNumberOfHoles;
			newAction.holeAction = InvAction.Add;
			newAction.hole = holeToAdd;
			newAction.TryAssignConstantID (newAction.hole, ref newAction.constantID);
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Scene: Change setting' Action, set to remove a hole from the current NavMesh</summary>
		 * <param name = "holeToRemove">The hole to remove</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionNavMesh CreateNew_RemoveNavMeshHole (PolygonCollider2D holeToRemove)
		{
			ActionNavMesh newAction = CreateNew<ActionNavMesh> ();
			newAction.sceneSetting = SceneSetting.DefaultNavMesh;
			newAction.changeNavMeshMethod = ChangeNavMeshMethod.ChangeNumberOfHoles;
			newAction.holeAction = InvAction.Remove;
			newAction.hole = holeToRemove;
			newAction.TryAssignConstantID (newAction.hole, ref newAction.constantID);
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Scene: Change setting' Action, set to change the default PlayerStart</summary>
		 * <param name = "newPlayerStart">The new PlayerStart</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionNavMesh CreateNew_ChangeDefaultPlayerStart (PlayerStart newPlayerStart)
		{
			ActionNavMesh newAction = CreateNew<ActionNavMesh> ();
			newAction.sceneSetting = SceneSetting.DefaultPlayerStart;
			newAction.playerStart = newPlayerStart;
			newAction.TryAssignConstantID (newAction.playerStart, ref newAction.constantID);
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Scene: Change setting' Action, set to change the current SortingMap</summary>
		 * <param name = "newSortingMap">The new SortingMap</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionNavMesh CreateNew_ChangeSortingMap (SortingMap newSortingMap)
		{
			ActionNavMesh newAction = CreateNew<ActionNavMesh> ();
			newAction.sceneSetting = SceneSetting.SortingMap;
			newAction.sortingMap = newSortingMap;
			newAction.TryAssignConstantID (newAction.sortingMap, ref newAction.constantID);
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Scene: Change setting' Action, set to change the current TintMap</summary>
		 * <param name = "newTintMap">The new TintMap</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionNavMesh CreateNew_ChangeTintMap (TintMap newTintMap)
		{
			ActionNavMesh newAction = CreateNew<ActionNavMesh> ();
			newAction.sceneSetting = SceneSetting.TintMap;
			newAction.tintMap = newTintMap;
			newAction.TryAssignConstantID (newAction.tintMap, ref newAction.constantID);
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Scene: Change setting' Action, set to change the OnLoad cutscene</summary>
		 * <param name = "newCutscene">The new OnLoad cutscene</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionNavMesh CreateNew_ChangeCutsceneOnLoad (Cutscene newCutscene)
		{
			ActionNavMesh newAction = CreateNew<ActionNavMesh> ();
			newAction.sceneSetting = SceneSetting.OnLoadCutscene;
			newAction.cutscene = newCutscene;
			newAction.TryAssignConstantID (newAction.cutscene, ref newAction.constantID);
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Scene: Change setting' Action, set to change the OnStart cutscene</summary>
		 * <param name = "newCutscene">The new OnStart cutscene</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionNavMesh CreateNew_ChangeCutsceneOnStart (Cutscene newCutscene)
		{
			ActionNavMesh newAction = CreateNew<ActionNavMesh> ();
			newAction.sceneSetting = SceneSetting.OnStartCutscene;
			newAction.cutscene = newCutscene;
			newAction.TryAssignConstantID (newAction.cutscene, ref newAction.constantID);
			return newAction;
		}
		
	}

}