/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionCharDirection.cs"
 * 
 *	This action is used to make characters turn to face fixed directions relative to the camera.
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
	public class ActionCharFaceDirection : Action
	{
		
		public int charToMoveParameterID = -1;
		public int charToMoveID = 0;

		public bool isInstant;
		public Direction direction;
		public enum Direction { Up, Down, Left, Right, UpLeft, DownLeft, UpRight, DownRight, SetDirection, SetPosition };
		public int directionParameterID = -1;

		public Char charToMove;
		protected Char runtimeCharToMove;

		public bool isPlayer;
		public int playerID = -1;

		public Vector3 vector;
		public int vectorParameterID = -1;
		protected Vector3 runtimeVector;
		
		[SerializeField] protected RelativeTo relativeTo = RelativeTo.Camera;
		public enum RelativeTo { Camera, Character };


		public override ActionCategory Category { get { return ActionCategory.Character; }}
		public override string Title { get { return "Face direction"; }}
		public override string Description { get { return "Makes a Character turn, either instantly or over time, to face a direction relative to the camera – i.e. up, down, left or right."; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			if (isPlayer)
			{
				runtimeCharToMove = AssignPlayer (playerID, parameters, charToMoveParameterID);
			}
			else
			{
				runtimeCharToMove = AssignFile <Char> (parameters, charToMoveParameterID, charToMoveID, charToMove);
			}

			if (directionParameterID >= 0)
			{
				int _directionInt = AssignInteger (parameters, directionParameterID, 0);
				direction = (Direction) _directionInt;
			}
		
			runtimeVector = AssignVector3 (parameters, vectorParameterID, vector);

			if (SceneSettings.IsUnity2D () && (direction == Direction.SetPosition || direction == Direction.SetDirection))
			{
				runtimeVector = new Vector3 (vector.x, 0f, vector.y);
			}
		}


		public override float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
				
				if (runtimeCharToMove)
				{
					if (!isInstant && runtimeCharToMove.IsMovingAlongPath ())
					{
						runtimeCharToMove.EndPath ();
					}

					Vector3 lookVector;
					if (direction == Direction.SetDirection)
					{
						lookVector = runtimeVector.normalized;
					}
					else if (direction == Direction.SetPosition)
					{
						lookVector = (runtimeVector - runtimeCharToMove.transform.position).normalized;
					}
					else
					{
						lookVector = AdvGame.GetCharLookVector ((CharDirection) direction, (relativeTo == RelativeTo.Character) ? runtimeCharToMove : null);
					}

					runtimeCharToMove.SetLookDirection (lookVector, isInstant);

					if (!isInstant)
					{
						if (willWait)
						{
							return (defaultPauseTime);
						}
					}
				}
				
				return 0f;
			}
			else
			{
				if (runtimeCharToMove.IsTurning ())
				{
					return defaultPauseTime;
				}
				else
				{
					isRunning = false;
					return 0f;
				}
			}
		}
		
		
		public override void Skip ()
		{
			if (runtimeCharToMove)
			{
				Vector3 lookVector;
				if (direction == Direction.SetDirection)
				{
					lookVector = runtimeVector.normalized;
				}
				else if (direction == Direction.SetPosition)
				{
					lookVector = (runtimeVector - runtimeCharToMove.transform.position).normalized;
				}
				else
				{
					lookVector = AdvGame.GetCharLookVector ((CharDirection) direction, (relativeTo == RelativeTo.Character) ? runtimeCharToMove : null);
				}
				
				runtimeCharToMove.SetLookDirection (lookVector, true);
			}
		}


		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Affect Player?", isPlayer);
			if (isPlayer)
			{
				PlayerField (ref playerID, parameters, ref charToMoveParameterID);
			}
			else
			{
				ComponentField ("Character to turn:", ref charToMove, ref charToMoveID, parameters, ref charToMoveParameterID);
			}

			direction = EnumPopupField<Direction> ("Direction to face:", direction, parameters, ref directionParameterID);

			if (directionParameterID < 0 && direction == Direction.SetDirection)
			{
				Vector3Field ("Direction:", ref vector, parameters, ref vectorParameterID);
			}
			else if (directionParameterID < 0 && direction == Direction.SetPosition)
			{
				Vector3Field ("Position:", ref vector, parameters, ref vectorParameterID);
			}
			else
			{
				relativeTo = (RelativeTo) EditorGUILayout.EnumPopup ("Direction is relative to:", relativeTo);
			}

			isInstant = EditorGUILayout.Toggle ("Is instant?", isInstant);
			if (!isInstant)
			{
				willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (!isPlayer)
			{
				if (saveScriptsToo && charToMove != null && !charToMove.IsPlayer)
				{
					AddSaveScript <RememberNPC> (charToMove);
				}

				charToMoveID = AssignConstantID<Char> (charToMove, charToMoveID, charToMoveParameterID);
			}
		}

		
		public override string SetLabel ()
		{
			if (charToMove != null)
			{
				return charToMove.name + " - " + direction;
			}
			return string.Empty;
		}


		public override bool ReferencesObjectOrID (GameObject _gameObject, int id)
		{
			if (!isPlayer && charToMoveParameterID < 0)
			{
				if (charToMove != null && charToMove.gameObject == _gameObject) return true;
				if (charToMoveID == id) return true;
			}
			if (isPlayer && _gameObject && _gameObject.GetComponent <Player>() != null) return true;
			return base.ReferencesObjectOrID (_gameObject, id);
		}


		public override bool ReferencesPlayer (int _playerID = -1)
		{
			if (!isPlayer) return false;
			if (_playerID < 0) return true;
			if (playerID < 0 && charToMoveParameterID < 0) return true;
			return (charToMoveParameterID < 0 && playerID == _playerID);
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Character: Face direction' Action</summary>
		 * <param name = "characterToTurn">The character to affect</param>
		 * <param name = "directionToFace">The direction to face</param>
		 * <param name = "relativeTo">What the supplied direction is relative to</param>
		 * <param name = "isInstant">If True, the character will stop turning their head instantly</param>
		 * <param name = "waitUntilFinish">If True, then the Action will wait until the transition is complete</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionCharFaceDirection CreateNew (AC.Char characterToTurn, CharDirection directionToFace, RelativeTo relativeTo = RelativeTo.Camera, bool isInstant = false, bool waitUntilFinish = false)
		{
			ActionCharFaceDirection newAction = CreateNew<ActionCharFaceDirection> ();
			newAction.charToMove = characterToTurn;
			newAction.TryAssignConstantID (newAction.charToMove, ref newAction.charToMoveID);
			newAction.direction = (Direction) directionToFace;
			newAction.relativeTo = relativeTo;
			newAction.isInstant = isInstant;
			newAction.willWait = waitUntilFinish;
			return newAction;
		}
		
	}

}