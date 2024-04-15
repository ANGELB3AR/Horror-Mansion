/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionCharMove.cs"
 * 
 *	This action moves characters by assinging them a Paths object.
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
	public class ActionCharMove : Action
	{

		public enum MovePathMethod { MoveOnNewPath, StopMoving, ResumeLastSetPath };
		public MovePathMethod movePathMethod = MovePathMethod.MoveOnNewPath;

		public int charToMoveParameterID = -1;
		public int movePathParameterID = -1;

		public int charToMoveID = 0;
		public int movePathID = 0;

		public bool stopInstantly;
		public Paths movePath;
		protected Paths runtimeMovePath;

		public bool isPlayer;
		public int playerID = -1;
		public int playerParameterID = -1;
		public Char charToMove;

		public bool doTeleport;
		[SerializeField] private bool startRandom = false; // Deprecated

		public enum MovePathNode { First, Random, Specific, Closest };
		public MovePathNode movePathNode = MovePathNode.First;
		public int nodeIndex;
		public int nodeIndexParameterID = -1;

		protected Char runtimeChar;

		
		public override ActionCategory Category { get { return ActionCategory.Character; }}
		public override string Title { get { return "Move along path"; }}
		public override string Description { get { return "Moves the Character along a pre-determined path. Will adhere to the speed setting selected in the relevant Paths object. Can also be used to stop a character from moving, or resume moving along a path if it was previously stopped."; }}


		public override void Upgrade ()
		{
			if (movePath != null && movePath.pathType == AC_PathType.IsRandom && startRandom)
			{
				startRandom = false;
				movePathNode = MovePathNode.Random;
			}
			base.Upgrade ();
		}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			runtimeMovePath = AssignFile <Paths> (parameters, movePathParameterID, movePathID, movePath);
			nodeIndex = AssignInteger (parameters, nodeIndexParameterID, nodeIndex);

			if (isPlayer)
			{
				runtimeChar = AssignPlayer (playerID, parameters, playerParameterID);
			}
			else
			{
				runtimeChar = AssignFile<Char> (parameters, charToMoveParameterID, charToMoveID, charToMove);
			}
		}


		public override float Run ()
		{
			if (runtimeMovePath && runtimeMovePath.GetComponent <Char>())
			{
				LogWarning ("Can't follow a Path attached to a Character!");
				return 0f;
			}

			if (!isRunning)
			{
				isRunning = true;

				if (runtimeChar)
				{
					if (!runtimeChar.IsActivePlayer ())
					{
						NPC npcToMove = (NPC) runtimeChar;
						npcToMove.StopFollowing ();
					}

					switch (movePathMethod)
					{
						case MovePathMethod.StopMoving:
							runtimeChar.EndPath ();
							if (runtimeChar.IsActivePlayer () && KickStarter.playerInteraction.GetHotspotMovingTo () != null)
							{
								KickStarter.playerInteraction.StopMovingToHotspot ();
							}

							if (stopInstantly)
							{
								runtimeChar.Halt ();
							}
							break;

						case MovePathMethod.MoveOnNewPath:
							if (runtimeMovePath)
							{
								int runtimeNodeIndex = nodeIndex;
								if (movePathNode == MovePathNode.First)
								{
									runtimeNodeIndex = 0;
								}
								else if (movePathNode == MovePathNode.Random)
								{
									if (runtimeMovePath.nodes.Count > 1)
									{
										runtimeNodeIndex = Random.Range (0, runtimeMovePath.nodes.Count);
									}
								}
								else if (movePathNode == MovePathNode.Closest)
								{
									runtimeNodeIndex = runtimeMovePath.GetNearestNode (runtimeChar.Transform.position);
								}
								
								if (doTeleport)
								{
									TeleportCharacter (runtimeNodeIndex, runtimeMovePath.pathType != AC_PathType.IsRandom);
								}

								if (willWait && runtimeMovePath.pathType != AC_PathType.ForwardOnly && runtimeMovePath.pathType != AC_PathType.ReverseOnly)
								{
									willWait = false;
									LogWarning ("Cannot pause while character moves along a linear path, as this will create an indefinite cutscene.");
								}

								int prevNodeIndex = runtimeNodeIndex - 1;
								if (runtimeMovePath.pathType == AC_PathType.IsRandom)
								{
									prevNodeIndex = 0;
								}
								else if (runtimeMovePath.pathType == AC_PathType.ReverseOnly)
								{
									prevNodeIndex = runtimeNodeIndex + 1;
								}

								if (prevNodeIndex < 0 || prevNodeIndex >= runtimeMovePath.nodes.Count)
								{
									runtimeChar.SetPath (runtimeMovePath);
								}
								else
								{
									runtimeChar.SetPath (runtimeMovePath, runtimeNodeIndex, prevNodeIndex);
								}

								if (willWait)
								{
									return defaultPauseTime;
								}
							}
							break;

						case MovePathMethod.ResumeLastSetPath:
							if (runtimeChar.GetLastPath ())
							{
								runtimeMovePath = runtimeChar.GetLastPath ();
								runtimeChar.ResumeLastPath ();

								if (willWait && (runtimeMovePath.pathType == AC_PathType.ForwardOnly || runtimeMovePath.pathType == AC_PathType.ReverseOnly))
								{
									return defaultPauseTime;
								}
							}
							break;

						default:
							break;
					}
				}

				return 0f;
			}
			else
			{
				if (runtimeChar.GetPath () != runtimeMovePath)
				{
					isRunning = false;
					return 0f;
				}
				else
				{
					return defaultPauseTime;
				}
			}
		}


		public override void Skip ()
		{
			if (runtimeChar)
			{
				runtimeChar.EndPath (runtimeMovePath);

				if (!runtimeChar.IsActivePlayer ())
				{
					NPC npcToMove = (NPC) runtimeChar;
					npcToMove.StopFollowing ();
				}

				if (movePathMethod == MovePathMethod.StopMoving)
				{
					return;
				}
				else if (movePathMethod == MovePathMethod.ResumeLastSetPath)
				{
					runtimeChar.ResumeLastPath ();
					runtimeMovePath = runtimeChar.GetPath ();
					return;
				}
				
				if (runtimeMovePath != null)
				{
					int runtimeNodeIndex = nodeIndex;
					if (movePathNode == MovePathNode.First)
					{
						if (willWait && runtimeMovePath.nodes.Count > 1)
						{
							runtimeNodeIndex = runtimeMovePath.nodes.Count - 1;
						}
						else
						{
							runtimeNodeIndex = 0;
						}
					}
					else if (movePathNode == MovePathNode.Random)
					{
						if (runtimeMovePath.nodes.Count > 1)
						{
							runtimeNodeIndex = Random.Range (0, runtimeMovePath.nodes.Count);
						}
					}
					else if (movePathNode == MovePathNode.Closest)
					{
						if (willWait && runtimeMovePath.nodes.Count > 1)
						{
							runtimeNodeIndex = runtimeMovePath.nodes.Count - 1;
						}
						else
						{
							runtimeNodeIndex = runtimeMovePath.GetNearestNode (runtimeChar.Transform.position);
						}
					}

					TeleportCharacter (runtimeNodeIndex, runtimeMovePath.pathType != AC_PathType.IsRandom);

					if ((!willWait || runtimeMovePath.pathType == AC_PathType.IsRandom) && !runtimeChar.IsActivePlayer ())
					{
						int prevNodeIndex = runtimeNodeIndex - 1;
						if (runtimeMovePath.pathType == AC_PathType.IsRandom)
						{
							prevNodeIndex = 0;
						}
						else if (runtimeMovePath.pathType == AC_PathType.ReverseOnly)
						{
							prevNodeIndex = runtimeNodeIndex + 1;
						}

						if (prevNodeIndex < 0 || prevNodeIndex >= runtimeMovePath.nodes.Count)
						{
							runtimeChar.SetPath (runtimeMovePath);
						}
						else
						{
							runtimeChar.SetPath (runtimeMovePath, runtimeNodeIndex, prevNodeIndex);
						}
					}
				}
			}
		}


		protected void TeleportCharacter (int startIndex = -1, bool faceDirection = true)
		{
			if (startIndex < 0)
			{
				if (runtimeMovePath.pathType == AC_PathType.ReverseOnly)
				{
					startIndex = runtimeMovePath.nodes.Count - 1;
				}
				else
				{
					startIndex = 0;
				}
			}

			if (startIndex < 0 || startIndex >= runtimeMovePath.nodes.Count)
			{
				return;
			}

			runtimeChar.Teleport (runtimeMovePath.nodes[startIndex]);

			if (faceDirection)
			{
				if (runtimeMovePath.pathType == AC_PathType.ReverseOnly)
				{
					if (startIndex > 0 && runtimeMovePath.nodes.Count >= 2)
					{
						runtimeChar.SetLookDirection (runtimeMovePath.nodes[startIndex-1] - runtimeMovePath.nodes[startIndex], true);
					}
				}
				else
				{
					if ((startIndex + 1) < runtimeMovePath.nodes.Count)
					{
						runtimeChar.SetLookDirection (runtimeMovePath.nodes[startIndex+1] - runtimeMovePath.nodes[startIndex], true);
					}
					else if ((startIndex + 1) == runtimeMovePath.nodes.Count && startIndex > 0)
					{
						runtimeChar.SetLookDirection (runtimeMovePath.nodes[startIndex] - runtimeMovePath.nodes[startIndex-1], true);
					}
				}
			}
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

			movePathMethod = (MovePathMethod) EditorGUILayout.EnumPopup ("Method:", movePathMethod);

			switch (movePathMethod)
			{
				case MovePathMethod.MoveOnNewPath:
				{
					ComponentField ("Path to follow:", ref movePath, ref movePathID, parameters, ref movePathParameterID);

					movePathNode = (MovePathNode) EditorGUILayout.EnumPopup ("Starting node:", movePathNode);
					if (movePathNode == MovePathNode.Specific)
					{
						IntField ("Node index:", ref nodeIndex, parameters, ref nodeIndexParameterID);
					}

					doTeleport = EditorGUILayout.Toggle ("Teleport to start?", doTeleport);
					if (movePath != null && movePath.pathType != AC_PathType.ForwardOnly && movePath.pathType != AC_PathType.ReverseOnly)
					{
						willWait = false;
					}
					else
					{
						willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
					}

					if (movePath != null && movePath.GetComponent <Char>())
					{
						EditorGUILayout.HelpBox ("Can't follow a Path attached to a Character!", MessageType.Warning);
					}
					break;
				}

				case MovePathMethod.StopMoving:
					stopInstantly = EditorGUILayout.Toggle ("Stop instantly?", stopInstantly);
					break;

				case MovePathMethod.ResumeLastSetPath:
					willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
					break;

				default:
					break;
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (saveScriptsToo)
			{
				AddSaveScript <ConstantID> (movePath);
				if (!isPlayer && charToMove != null && !charToMove.IsPlayer)
				{
					AddSaveScript <RememberNPC> (charToMove);
				}
			}

			if (!isPlayer)
			{
				charToMoveID = AssignConstantID<Char> (charToMove, charToMoveID, charToMoveParameterID);
			}
			movePathID = AssignConstantID<Paths> (movePath, movePathID, movePathParameterID);
		}
				
		
		public override string SetLabel ()
		{
			if (movePath != null)
			{
				if (charToMove != null)
				{
					return charToMove.name + " to " + movePath.name;
				}
				else if (isPlayer)
				{
					return "Player to " + movePath.name;
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
			if (movePathMethod == MovePathMethod.MoveOnNewPath && movePathParameterID < 0)
			{
				if (movePath && movePath.gameObject == _gameObject) return true;
				if (movePathID == id) return true;
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
		 * <summary>Creates a new instance of the 'Character: Move along path' Action, set to command a character to move along a new path</summary>
		 * <param name = "characterToMove">The character to affect</param>
		 * <param name = "pathToFollow">The Path that the character should follow</param>
		 * <param name = "teleportToStart">If True, the character will teleport to the first node on the Path</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionCharMove CreateNew_NewPath (AC.Char characterToMove, Paths pathToFollow, bool teleportToStart = false)
		{
			ActionCharMove newAction = CreateNew<ActionCharMove> ();
			newAction.movePathMethod = MovePathMethod.MoveOnNewPath;
			newAction.charToMove = characterToMove;
			newAction.TryAssignConstantID (newAction.charToMove, ref newAction.charToMoveID);
			newAction.movePath = pathToFollow;
			newAction.TryAssignConstantID (newAction.movePath, ref newAction.movePathID);
			newAction.doTeleport = teleportToStart;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Character: Move along path' Action, set to command a character to resume moving along their last-assigned path</summary>
		 * <param name = "characterToMove">The character to affect</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionCharMove CreateNew_ResumeLastPath (AC.Char characterToMove)
		{
			ActionCharMove newAction = CreateNew<ActionCharMove> ();
			newAction.movePathMethod = MovePathMethod.ResumeLastSetPath;
			newAction.charToMove = characterToMove;
			newAction.TryAssignConstantID (newAction.charToMove, ref newAction.charToMoveID);
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Character: Move along path' Action, set to command a character to stop moving</summary>
		 * <param name = "characterToStop">The character to affect</param>
		 * <param name = "stopInstantly">If True, the character will stop in one frame, as opposed to more naturally through deceleration</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionCharMove CreateNew_StopMoving (AC.Char characterToStop, bool stopInstantly = false)
		{
			ActionCharMove newAction = CreateNew<ActionCharMove> ();
			newAction.movePathMethod = MovePathMethod.StopMoving;
			newAction.charToMove = characterToStop;
			newAction.TryAssignConstantID (newAction.charToMove, ref newAction.charToMoveID);
			newAction.stopInstantly = stopInstantly;
			return newAction;
		}
		
	}

}