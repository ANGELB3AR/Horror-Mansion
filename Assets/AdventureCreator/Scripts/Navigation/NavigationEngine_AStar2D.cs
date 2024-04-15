using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using AC.AStar2D;

namespace AC
{

	public class NavigationEngine_AStar2D : NavigationEngine
	{

		#region Variables

		private Dictionary<Collider2D, Grid2D> gridDict = new Dictionary<Collider2D, Grid2D> ();
		private readonly AC.AStar2D.Pathfinding pathfinding = new AC.AStar2D.Pathfinding ();

		#endregion


		#region PublicFunctions

		public override void OnReset (NavigationMesh navMesh)
		{
			if (navMesh)
			{
				float cellWidth = navMesh.accuracy * 0.1f;
				float cellHeight = cellWidth;

				gridDict.Clear ();
				Collider2D[] colliders = navMesh.Collider2Ds;
				foreach (Collider2D collider in colliders)
				{
					Grid2D grid = new Grid2D (collider, cellWidth, cellHeight);
					gridDict.Add (collider, grid);
				}

				if (Application.isPlaying)
				{
					ResetHoles (navMesh);
				}
			}
			base.OnReset (navMesh);
		}


		public Grid2D GetGrid (Collider2D collider)
		{
			if (gridDict.ContainsKey (collider))
			{
				return gridDict[collider];
			}
			return null;
		}


		public override void TurnOn (NavigationMesh navMesh)
		{
			if (navMesh == null || KickStarter.settingsManager == null) return;
			
			if (LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer) == -1)
			{
				ACDebug.LogError ("Can't find layer " + KickStarter.settingsManager.navMeshLayer + " - please define it in Unity's Tags Manager (Edit -> Project settings -> Tags and Layers).");
			}
			else if (!string.IsNullOrEmpty (KickStarter.settingsManager.navMeshLayer))
			{
				navMesh.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer);
			}

			if (navMesh.GetComponent<Collider2D> () == null)
			{
				ACDebug.LogWarning ("A 2D Collider component must be attached to " + navMesh.gameObject.name + " for pathfinding to work - please attach one.");
			}

			OnReset (navMesh);
		}


		public override Vector3[] GetPointsArray (Vector3 startPosition, Vector3 targetPosition, Char _char = null)
		{
			Collider2D[] colliders = KickStarter.sceneSettings.navMesh.Collider2Ds;
			if (colliders == null || colliders.Length == 0)
			{
				return base.GetPointsArray (startPosition, targetPosition, _char);
			}

			// Get correct polygon
			float minDist = Mathf.Infinity;
			int minDistIndex = -1;
			for (int i = 0; i < colliders.Length; i++)
			{
				if (colliders[i].OverlapPoint (startPosition))
				{
					Grid2D grid = gridDict[colliders[i]];
					AddCharHoles (grid, _char, KickStarter.sceneSettings.navMesh);

					return pathfinding.FindPath (startPosition, targetPosition, grid);
				}

#if UNITY_2019_4_OR_NEWER
				float dist = (colliders[i].ClosestPoint (startPosition) - (Vector2) startPosition).sqrMagnitude;
#else
				float dist = (colliders[i].bounds.center - startPosition).sqrMagnitude;
#endif
				if (dist < minDist)
				{
					minDist = dist;
					minDistIndex = i;
				}
			}

			if (minDistIndex >= 0)
			{
				Grid2D grid = gridDict[colliders[minDistIndex]];
				AddCharHoles (grid, _char, KickStarter.sceneSettings.navMesh);

				return pathfinding.FindPath (startPosition, targetPosition, grid);
			}

			return base.GetPointsArray (startPosition, targetPosition, _char);
		}


		public override string GetPrefabName ()
		{
			return ("NavMesh2D");
		}
		
		
		public override void SceneSettingsGUI ()
		{
			#if UNITY_EDITOR
			EditorGUILayout.BeginHorizontal ();
			KickStarter.sceneSettings.navMesh = (NavigationMesh) EditorGUILayout.ObjectField ("Default NavMesh:", KickStarter.sceneSettings.navMesh, typeof (NavigationMesh), true);
			if (!SceneSettings.IsUnity2D ())
			{
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.HelpBox ("This pathfinding method is only compatible with 'Unity 2D' mode.", MessageType.Warning);
				EditorGUILayout.BeginHorizontal ();
			}
			else if (KickStarter.sceneSettings.navMesh == null)
			{
				if (CustomGUILayout.ClickedCreateButton ())
				{
					NavigationMesh newNavMesh = null;
					newNavMesh = SceneManager.AddPrefab ("Navigation", "NavMesh2D", true, false, true).GetComponent <NavigationMesh>();

					newNavMesh.gameObject.name = "Default NavMesh";
					KickStarter.sceneSettings.navMesh = newNavMesh;
					EditorGUIUtility.PingObject (newNavMesh.gameObject);
				}
			}
			EditorGUILayout.EndHorizontal ();
			#endif
		}


		public override void ResetHoles (NavigationMesh navMesh)
		{
			if (navMesh == null) return;
			
			PolygonCollider2D[] polys = navMesh.PolygonCollider2Ds;
			if (polys == null || polys.Length == 0) return;

			for (int p = 0; p < polys.Length; p++)
			{
				polys[p].pathCount = navMesh.OriginalPathCount;

				Vector2 scaleFac = new Vector2 (1f / navMesh.transform.lossyScale.x, 1f / navMesh.transform.lossyScale.y);
				foreach (PolygonCollider2D hole in navMesh.polygonColliderHoles)
				{
					if (hole)
					{
						polys[p].pathCount++;

						List<Vector2> newPoints = new List<Vector2> ();
						foreach (Vector2 holePoint in hole.points)
						{
							Vector2 relativePosition = hole.transform.TransformPoint (holePoint) - navMesh.transform.position;
							newPoints.Add (new Vector2 (relativePosition.x * scaleFac.x, relativePosition.y * scaleFac.y));
						}

						polys[p].SetPath (polys[p].pathCount - 1, newPoints.ToArray ());
						hole.gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
						hole.isTrigger = true;
					}
				}

				if (gridDict.ContainsKey (polys[p]))
				{
					Grid2D grid = gridDict[polys[p]];
					grid.Rebuild ();
				}
			}
		}

		#endregion


		#if UNITY_EDITOR

		public override NavigationMesh NavigationMeshGUI (NavigationMesh _target)
		{
			_target = base.NavigationMeshGUI (_target);

			_target.characterEvasion = (CharacterEvasion) CustomGUILayout.EnumPopup ("Character evasion:", _target.characterEvasion, "", "The condition for which dynamic 2D pathfinding can occur by generating holes around characters");
			if (_target.characterEvasion != CharacterEvasion.None)
			{
				_target.characterEvasionYScale = CustomGUILayout.Slider ("Evasion y-scale:", _target.characterEvasionYScale, 0.1f, 1f, "", "The scale of generated character evasion 'holes' in the NavMesh in the y-axis, relative to the x-axis");

				EditorGUILayout.HelpBox ("Note: Characters can only be avoided if they have a Circle Collider 2D (no Trigger) component on their base.\n\n" +
					"For best results, set a non-zero 'Pathfinding update time' in the Settings Manager.", MessageType.Info);

				if (_target.transform.lossyScale != Vector3.one)
				{
					EditorGUILayout.HelpBox ("For character evasion to work, the NavMesh must have a unit scale (1,1,1).", MessageType.Warning);
				}

				#if UNITY_ANDROID || UNITY_IOS
				EditorGUILayout.HelpBox ("This is an expensive calculation - consider setting this to 'None' for mobile platforms.", MessageType.Warning);
				#endif
			}

			float cellSize = _target.accuracy * 0.1f;
			cellSize = CustomGUILayout.Slider ("Cell size:", cellSize, 0.01f, 1f);
			_target.accuracy = cellSize * 10f;
			_target.gizmoColour = CustomGUILayout.ColorField ("Gizmo colour:", _target.gizmoColour, "", "The colour of its Gizmo when used for 2D polygons");

			EditorGUILayout.Separator ();
			GUILayout.Box (string.Empty, GUILayout.ExpandWidth (true), GUILayout.Height(1));
			EditorGUILayout.LabelField ("NavMesh holes", EditorStyles.boldLabel);

			for (int i=0; i<_target.polygonColliderHoles.Count; i++)
			{
				EditorGUILayout.BeginHorizontal ();
				_target.polygonColliderHoles [i] = (PolygonCollider2D) CustomGUILayout.ObjectField <PolygonCollider2D> ("Hole #" + i.ToString () + ":", _target.polygonColliderHoles [i], true, "", "A shape within the boundary of this PolygonCollider2D to create a hole from");

				if (GUILayout.Button ("-", GUILayout.MaxWidth (20f)))
				{
					_target.polygonColliderHoles.RemoveAt (i);
					i=-1;
					continue;
				}

				EditorGUILayout.EndHorizontal ();

				if (_target.polygonColliderHoles[i] != null && _target.polygonColliderHoles[i].GetComponent <NavMeshBase>())
				{
					EditorGUILayout.HelpBox ("A NavMesh cannot use its own Polygon Collider component as a hole!", MessageType.Warning);
				}
			}

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Create new hole"))
			{
				_target.polygonColliderHoles.Add (null);
			}
			GUI.enabled = !Application.isPlaying;
			if (_target.polygonColliderHoles.Count > 0 && GUILayout.Button ("Bake"))
			{
				BakeHoles (_target);
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal ();

			if (_target.GetComponent <PolygonCollider2D>())
			{
				int numPolys = _target.GetComponents <PolygonCollider2D>().Length;
				if (numPolys > 1)
				{
					if (_target.polygonColliderHoles.Count > 0)
					{
						EditorGUILayout.HelpBox ("Holes will only work if they are within the boundaries of " + _target.gameObject.name + "'s FIRST PolygonCollider component.", MessageType.Warning);
					}
					if (_target.characterEvasion != CharacterEvasion.None)
					{
						EditorGUILayout.HelpBox ("Character-evasion will only work within the boundaries of " + _target.gameObject.name + "'s FIRST PolygonCollider component.", MessageType.Warning);
					}
				}
			}

			return _target;
		}


		public override void DrawGizmos (GameObject navMeshOb)
		{
			if (navMeshOb)
			{
				NavigationMesh navMesh = navMeshOb.GetComponent<NavigationMesh> ();
				if (navMesh == null)
				{
					return;
				}

				PolygonCollider2D[] polys = navMeshOb.GetComponents <PolygonCollider2D>();
				if (polys != null)
				{
					for (int i=0; i<polys.Length; i++)
					{
						AdvGame.DrawPolygonCollider (navMeshOb.transform, polys[i], navMesh.gizmoColour);
					}
				}

				if (gridDict.Count > 0 && Application.isPlaying)
				{
					foreach (Grid2D grid in gridDict.Values)
					{
						grid.DrawGizmos ();
					}
					return;
				}

				#if UNITY_EDITOR
				if (Selection.activeGameObject != navMeshOb) return;
				#endif
				
				Gizmos.color = navMesh.gizmoColour;
				
				Collider2D[] colliders = navMesh.GetComponents<Collider2D> ();
				foreach (Collider2D _collider in colliders)
				{
					float cellWidth = navMesh.accuracy * 0.1f;
					float cellHeight = cellWidth;

					int gridSizeX = Mathf.RoundToInt (_collider.bounds.size.x / cellWidth);
					int gridSizeY = Mathf.RoundToInt (_collider.bounds.size.y / cellHeight);
					Vector2 bottomLeft = (Vector2) _collider.bounds.center - (Vector2.right * _collider.bounds.size.x / 2f) - (Vector2.up * _collider.bounds.size.y / 2f);

					for (int x = 0; x < gridSizeX; x++)
					{
						for (int y = 0; y < gridSizeY; y++)
						{
							Vector2 position = bottomLeft + Vector2.right * (x * cellWidth + cellWidth * 0.5f) + Vector2.up * (y * cellHeight + cellHeight * 0.5f);
							Gizmos.DrawWireCube (position, new Vector2 (cellWidth, cellHeight));
						}
					}
				}
			}
		}

		
		private void BakeHoles (NavigationMesh navMesh)
		{
			PolygonCollider2D[] polys = navMesh.GetComponents<PolygonCollider2D> ();
			if (polys == null || polys.Length == 0) return;

			if (polys[0].pathCount > 1)
			{
				bool addSubPaths = EditorUtility.DisplayDialog ("Reset sub-paths?", "The NavMesh already has additional path data baked into it.  Should the new holes be added to them, or replace them?", "Add", "Replace");
				if (!addSubPaths)
				{
					polys[0].pathCount = 1;
				}
			}

			List<Object> undoObs = new List<Object> ();
			undoObs.Add (polys[0]);
			undoObs.Add (navMesh);
			for (int i = 0; i < navMesh.polygonColliderHoles.Count; i++)
			{
				PolygonCollider2D hole = navMesh.polygonColliderHoles[i];
				if (hole && !undoObs.Contains (hole))
				{
					undoObs.Add (hole);
				}
			}

			Undo.RecordObjects (undoObs.ToArray (), "Bake NavMesh holes");

			Vector2 scaleFac = new Vector2 (1f / navMesh.transform.lossyScale.x, 1f / navMesh.transform.lossyScale.y);
			for (int i = 0; i < navMesh.polygonColliderHoles.Count; i++)
			{
				PolygonCollider2D hole = navMesh.polygonColliderHoles[i];

				if (hole)
				{
					polys[0].pathCount++;

					List<Vector2> newPoints = new List<Vector2> ();
					foreach (Vector2 holePoint in hole.points)
					{
						Vector2 relativePosition = hole.transform.TransformPoint (holePoint) - navMesh.transform.position;
						newPoints.Add (new Vector2 (relativePosition.x * scaleFac.x, relativePosition.y * scaleFac.y));
					}

					polys[0].SetPath (polys[0].pathCount - 1, newPoints.ToArray ());
					hole.enabled = false;
				}
			}

			navMesh.polygonColliderHoles.Clear ();
		}

		#endif


		#region PrivateFunctions
		
		private void AddCharHoles (Grid2D grid, AC.Char charToExclude, NavigationMesh navigationMesh)
		{
			if (navigationMesh.characterEvasion == CharacterEvasion.None)
			{
				return;
			}

			grid.ClearCharHoles ();

			foreach (AC.Char character in KickStarter.stateHandler.Characters)
			{
				// Discard if not inside
				if (!grid.IsPointInside (character.transform.position)) continue;

				CircleCollider2D circleCollider2D = character.GetComponent<CircleCollider2D> ();
				if (circleCollider2D != null &&
					(character.charState == CharState.Idle || navigationMesh.characterEvasion == CharacterEvasion.AllCharacters) &&
					(charToExclude == null || character != charToExclude))
				{
					if (character.IsPlayer && KickStarter.settingsManager.movementMethod == MovementMethod.Direct)
					{
						// In this particular case, do not set Is Trigger
					}
					else
					{
						circleCollider2D.isTrigger = true;
					}

					grid.AddCharHole (character.transform.position, circleCollider2D.radius, navigationMesh.characterEvasionYScale);
				}
			}
		}

		#endregion


		#region GetSet

		public override bool RequiresNavMeshGameObject
		{
			get
			{
				return true;
			}
		}

		#endregion

	}

}