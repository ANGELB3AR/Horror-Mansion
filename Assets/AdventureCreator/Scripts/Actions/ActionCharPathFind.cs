/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionCharPathFind.cs"
 * 
 *	This action moves characters by generating a path to a specified point.
 *	If a player is moved, the game will automatically pause.
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
	public class ActionCharPathFind : Action
	{

		public int charToMoveParameterID = -1;
		public int markerParameterID = -1;

		public int charToMoveID = 0;
		public int markerID = 0;
		
		public Marker marker;
		public bool isPlayer;
		public int playerID = -1;
		public int playerParameterID = -1;
		public Char charToMove;
		public PathSpeed speed;
		public bool pathFind = true;
		public bool doFloat = false;

		public Vector3 positionOffset;
		public int positionOffsetParameterID = -1;

		public bool doTimeLimit;
		public int maxTimeParameterID = -1;
		public float maxTime = 10f;
		[SerializeField] protected OnReachTimeLimit onReachTimeLimit = OnReachTimeLimit.TeleportToDestination;
		protected enum OnReachTimeLimit { TeleportToDestination, StopMoving };
		protected float currentTimer;
		protected Char runtimeChar;
		protected Marker runtimeMarker;
		public float minDistance = 0f;
		protected float minSqrDistance;

		public ActionCharMove.MovePathNode movePathNode = ActionCharMove.MovePathNode.First;
		public int nodeIndex;
		public int nodeIndexParameterID = -1;

		public bool faceAfter = false;
		protected bool isFacingAfter;


		public override ActionCategory Category { get { return ActionCategory.Character; }}
		public override string Title { get { return "Move to point"; }}
		public override string Description { get { return "Moves a character to a given Marker object. By default, the character will attempt to pathfind their way to the marker, but can optionally just move in a straight line."; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			if (isPlayer)
			{
				runtimeChar = AssignPlayer (playerID, parameters, playerParameterID);
			}
			else
			{
				runtimeChar = AssignFile <Char> (parameters, charToMoveParameterID, charToMoveID, charToMove);
			}

			Hotspot markerHotspot = AssignFile <Hotspot> (parameters, markerParameterID, markerID, null, false);
			if (markerHotspot != null && markerHotspot.walkToMarker != null)
			{
				runtimeMarker = markerHotspot.walkToMarker;
			}
			else
			{
				runtimeMarker = AssignFile <Marker> (parameters, markerParameterID, markerID, marker);
			}

			nodeIndex = AssignInteger (parameters, nodeIndexParameterID, nodeIndex);

			maxTime = AssignFloat (parameters, maxTimeParameterID, maxTime);
			isFacingAfter = false;
			minSqrDistance = minDistance * minDistance;

			positionOffset = AssignVector3 (parameters, positionOffsetParameterID, positionOffset);
		}
		
		
		public override float Run ()
		{
			if (!isRunning)
			{
				if (runtimeChar && (runtimeMarker || positionOffset.sqrMagnitude > 0f))
				{
					isRunning = true;

					Paths path = runtimeChar.GetComponent <Paths>();
					if (path == null)
					{
						LogWarning ("Cannot move a character with no Paths component", runtimeChar);
					}
					else
					{
						if (!runtimeChar.IsPlayer)
						{
							NPC npcToMove = (NPC) runtimeChar;
							npcToMove.StopFollowing ();
						}

						path.pathType = AC_PathType.ForwardOnly;
						path.pathSpeed = speed;
						path.affectY = true;

						Vector3[] pointArray;
						Vector3 targetPosition = runtimeMarker ? runtimeMarker.Position : Vector3.zero;

						targetPosition = GetPathPosition (targetPosition);

						if (SceneSettings.ActInScreenSpace ())
						{
							targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
						}

						targetPosition += positionOffset;

						float distance = Vector3.Distance (targetPosition, runtimeChar.Transform.position);
						if (distance <= KickStarter.settingsManager.GetDestinationThreshold ())
						{
							if (willWait && faceAfter)
							{
								return defaultPauseTime;
							}

							isRunning = false;
							return 0f;
						}

						if (minDistance > 0f && distance <= minDistance)
						{
							isRunning = false;
							return 0f;
						}

						if (pathFind && KickStarter.navigationManager)
						{
							pointArray = KickStarter.navigationManager.navigationEngine.GetPointsArray (runtimeChar.Transform.position, targetPosition, runtimeChar);
						}
						else
						{
							List<Vector3> pointList = new List<Vector3>();
							pointList.Add (targetPosition);
							pointArray = pointList.ToArray ();
						}

						if (speed == PathSpeed.Walk)
						{
							runtimeChar.MoveAlongPoints (pointArray, false, pathFind);
						}
						else
						{
							runtimeChar.MoveAlongPoints (pointArray, true, pathFind);
						}

						if (runtimeChar.GetPath ())
						{
							if (!pathFind && doFloat)
							{
								runtimeChar.GetPath ().affectY = true;
							}
							else
							{
								runtimeChar.GetPath ().affectY = false;
							}
						}

						if (willWait)
						{
							currentTimer = maxTime;
							return defaultPauseTime;
						}
					}
				}

				return 0f;
			}
			else
			{
				if (runtimeChar.GetPath () == null)
				{
					if (faceAfter)
					{
						if (!isFacingAfter && runtimeMarker)
						{
							isFacingAfter = true;
							runtimeChar.SetLookDirection (runtimeMarker.ForwardDirection, false);
							return defaultPauseTime;
						}
						else
						{
							if (runtimeChar.IsTurning ())
							{
								return defaultPauseTime;
							}
						}
					}

					isRunning = false;
					return 0f;
				}
				else
				{
					if (doTimeLimit)
					{
						currentTimer -= Time.deltaTime;
						if (currentTimer <= 0)
						{
							switch (onReachTimeLimit)
							{
								case OnReachTimeLimit.StopMoving:
									runtimeChar.EndPath ();
									break;

								case OnReachTimeLimit.TeleportToDestination:
									Skip ();
									break;
							}

							isRunning = false;
							return 0f;
						}
					}

					if (minSqrDistance > 0f)
					{
						float sqrDistance = (runtimeChar.Transform.position - runtimeMarker.Transform.position).sqrMagnitude;
						if (sqrDistance <= minSqrDistance)
						{
							runtimeChar.EndPath ();
							isRunning = false;
							return 0f;
						}
					}

					return (defaultPauseTime);
				}
			}
		}


		public override void Skip ()
		{
			if (runtimeChar != null && runtimeMarker != null)
			{
				runtimeChar.EndPath ();

				if (!runtimeChar.IsPlayer)
				{
					NPC npcToMove = (NPC) runtimeChar;
					npcToMove.StopFollowing ();
				}
				
				Vector3[] pointArray;
				Vector3 targetPosition = runtimeMarker.Position;
				
				targetPosition = GetPathPosition (targetPosition);

				if (minDistance > 0f)
				{
					Vector3 relativePosition = runtimeChar.Transform.position - targetPosition;
					if (relativePosition.magnitude < minDistance)
					{
						return;
					}

					targetPosition += relativePosition.normalized * minDistance;
				}

				if (SceneSettings.ActInScreenSpace ())
				{
					targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
				}
				
				if (pathFind && KickStarter.navigationManager)
				{
					pointArray = KickStarter.navigationManager.navigationEngine.GetPointsArray (runtimeChar.Transform.position, targetPosition);
					KickStarter.navigationManager.navigationEngine.ResetHoles (KickStarter.sceneSettings.navMesh);
				}
				else
				{
					List<Vector3> pointList = new List<Vector3>();
					pointList.Add (targetPosition);
					pointArray = pointList.ToArray ();
				}
				
				int i = pointArray.Length-1;

				if (i>0)
				{
					runtimeChar.SetLookDirection (pointArray[i] - pointArray[i-1], true);
				}
				else if (i == 0)
				{
					runtimeChar.SetLookDirection (pointArray[i] - runtimeChar.Transform.position, true);
				}

				if (i >= 0 && i < pointArray.Length)
				{
					runtimeChar.Teleport (pointArray [i]);
				}
				else
				{
					runtimeChar.Teleport (targetPosition);
				}

				if (faceAfter)
				{
					runtimeChar.SetLookDirection (runtimeMarker.ForwardDirection, true);
				}
			}
		}


		private Vector3 GetPathPosition (Vector3 targetPosition)
		{
			Paths markerPath = runtimeMarker.GetComponent<Paths> ();
			if (markerPath)
			{
				switch (movePathNode)
				{
					case ActionCharMove.MovePathNode.First:
					default:
						break;
					
					case ActionCharMove.MovePathNode.Random:
						if (markerPath.nodes.Count > 1)
						{
							int _index = Random.Range (0, markerPath.nodes.Count);
							targetPosition = markerPath.nodes[_index];
						}
						break;
					
					case ActionCharMove.MovePathNode.Specific:
						if (nodeIndex >= 0 && nodeIndex < markerPath.nodes.Count)
						{
							targetPosition = markerPath.nodes[nodeIndex];
						}
						break;
						
					case ActionCharMove.MovePathNode.Closest:
						{
							int _index = markerPath.GetNearestNode (runtimeChar.Transform.position);
							targetPosition = markerPath.nodes[_index];
						}
						break;
				}
			}
			return targetPosition;
		}

		
		#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);

			if (isPlayer)
			{
				PlayerField (ref playerID, parameters, ref playerParameterID);
			}
			else
			{
				ComponentField ("Character to move:", ref charToMove, ref charToMoveID, parameters, ref charToMoveParameterID);
			}

			ComponentField ("Marker to reach:", ref marker, ref markerID, parameters, ref markerParameterID);
			if (markerParameterID >= 0)
			{
				EditorGUILayout.HelpBox ("If a Hotspot is passed to this parameter, that Hotspot's 'Walk-to Marker' will be referred to.", MessageType.Info);
			}

			Vector3Field ("Position offset:", ref positionOffset, parameters, ref positionOffsetParameterID);

			if (marker && marker.GetComponent<Paths> ())
			{
				movePathNode = (ActionCharMove.MovePathNode) EditorGUILayout.EnumPopup ("Path node:", movePathNode);
				if (movePathNode == ActionCharMove.MovePathNode.Specific)
				{
					IntField ("Node index:", ref nodeIndex, parameters, ref nodeIndexParameterID);
				}
			}

			speed = (PathSpeed) EditorGUILayout.EnumPopup ("Move speed:" , speed);
			pathFind = EditorGUILayout.Toggle ("Pathfind?", pathFind);
			if (!pathFind)
			{
				doFloat = EditorGUILayout.Toggle ("Ignore gravity?", doFloat);
			}
			willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);

			if (willWait)
			{
				EditorGUILayout.Space ();
				faceAfter = EditorGUILayout.Toggle ("Copy Marker angle after?", faceAfter);
				minDistance = EditorGUILayout.FloatField ("Minimum distance:", minDistance);
				doTimeLimit = EditorGUILayout.Toggle ("Enforce time limit?", doTimeLimit);
				if (doTimeLimit)
				{
					FloatField ("Time limit (s):", ref maxTime, parameters, ref maxTimeParameterID);
					onReachTimeLimit = (OnReachTimeLimit) EditorGUILayout.EnumPopup ("On reach time limit:", onReachTimeLimit);
				}
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (saveScriptsToo)
			{
				if (!isPlayer && charToMove != null && !charToMove.IsPlayer)
				{
					AddSaveScript <RememberNPC> (charToMove);
				}
			}

			if (!isPlayer)
			{
				charToMoveID = AssignConstantID<Char> (charToMove, charToMoveID, charToMoveParameterID);
			}
			markerID = AssignConstantID<Marker> (marker, markerID, markerParameterID);
		}

		
		public override string SetLabel ()
		{
			if (marker != null)
			{
				if (charToMove != null)
				{
					return charToMove.name + " to " + marker.name;
				}
				else if (isPlayer)
				{
					return "Player to " + marker.name;
				}
			}
			return string.Empty;
		}


		public override bool ReferencesObjectOrID (GameObject _gameObject, int id)
		{
			if (!isPlayer && charToMoveParameterID < 0)
			{
				if (charToMove && charToMove.gameObject == _gameObject) return true;
				if (charToMoveID == id) return true;
			}
			if (isPlayer && _gameObject && _gameObject.GetComponent <Player>()) return true;
			if (markerParameterID < 0)
			{
				if (marker && marker.gameObject == _gameObject) return true;
				if (markerID == id) return true;
			}
			return base.ReferencesObjectOrID (_gameObject, id);
		}


		public override bool ReferencesPlayer (int _playerID = -1)
		{
			if (!isPlayer) return false;
			if (_playerID < 0) return true;
			if (playerID < 0 && playerParameterID < 0) return true;
			return (playerParameterID < 0 && playerID == _playerID);
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Character: Move to point' Action with key variables already set.</summary>
		 * <param name = "charToMove">The character to move</param>
		 * <param name = "marker">The Marker to move the character to</param>
		 * <param name = "pathSpeed">How fast the character moves (Walk, Run)</param>
		 * <param name = "usePathfinding">If True, the character will rely on pathfinding to reach the Marker</param>
		 * <param name = "waitUntilFinish">If True, the Action will wait until the character has reached the Marker</param>
		 * <param name = "turnToFaceAfter">If True, and waitUntilFinish = true, then the character will face the same direction that the Marker is facing after reaching it</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionCharPathFind CreateNew (Char charToMove, Marker marker, PathSpeed pathSpeed = PathSpeed.Walk, bool usePathfinding = true, bool waitUntilFinish = true, bool turnToFaceAfter = false)
		{
			ActionCharPathFind newAction = CreateNew<ActionCharPathFind> ();
			newAction.charToMove = charToMove;
			newAction.TryAssignConstantID (newAction.charToMove, ref newAction.charToMoveID);
			newAction.marker = marker;
			newAction.TryAssignConstantID (newAction.marker, ref newAction.markerID);
			newAction.speed = pathSpeed;
			newAction.pathFind = usePathfinding;
			newAction.willWait = waitUntilFinish;
			newAction.faceAfter = turnToFaceAfter;
			return newAction;
		}
		
	}

}