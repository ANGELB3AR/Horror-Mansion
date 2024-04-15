using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionObjectiveCheckType : Action, IObjectiveReferencerAction
	{

		public int objectiveID;
		public int objectiveParameterID = -1;

		public int playerID;
		public bool setPlayer;
		public int numSockets = 4;

		
		public override ActionCategory Category { get { return ActionCategory.Objective; }}
		public override string Title { get { return "Check state type"; }}
		public override string Description { get { return "Queries the current state type of an objective."; }}
		public override int NumSockets { get { return numSockets; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			objectiveID = AssignObjectiveID (parameters, objectiveParameterID, objectiveID);
		}


		public override int GetNextOutputIndex ()
		{
			Objective objective = KickStarter.inventoryManager.GetObjective (objectiveID);
			if (objective != null)
			{
				int _playerID = (setPlayer && KickStarter.inventoryManager.ObjectiveIsPerPlayer (objectiveID)) ? playerID : -1;

				ObjectiveState currentObjectiveState = KickStarter.runtimeObjectives.GetObjectiveState (objectiveID, _playerID);
				if (currentObjectiveState != null)
				{
					return (int) currentObjectiveState.stateType;
				}
			}
			return 0;
		}

		
		#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			if (KickStarter.inventoryManager == null)
			{
				numSockets = 0;
				EditorGUILayout.HelpBox ("An Inventory Manager must be defined to use this Action", MessageType.Warning);
				return;
			}

			ObjectiveField (ref objectiveID, parameters, ref objectiveParameterID);
			if (objectiveParameterID < 0)
			{
				if (KickStarter.inventoryManager.ObjectiveIsPerPlayer (objectiveID))
				{
					setPlayer = EditorGUILayout.Toggle ("Check specific Player?", setPlayer);
					if (setPlayer)
					{
						playerID = ChoosePlayerGUI (playerID, false);
					}
				}
			}
			
			numSockets = 4;
		}
		

		public override string SetLabel ()
		{
			if (objectiveParameterID < 0)
			{
				Objective objective = KickStarter.inventoryManager.GetObjective (objectiveID);
				if (objective != null)
				{
					return objective.Title;
				}
			}			
			return string.Empty;
		}


		protected override string GetSocketLabel (int i)
		{
			switch (i)
			{
				case 0:
					return "If Inactive:";

				case 1:
					return "If Active:";

				case 2:
					return "If Complete:";

				case 3:
					return "If Failed:";

				default:
					return string.Empty;
			}
		}


		public int GetNumObjectiveReferences (int _objectiveID)
		{
			return (objectiveParameterID < 0 && objectiveID == _objectiveID) ? 1 : 0;
		}


		public int UpdateObjectiveReferences (int oldObjectiveID, int newObjectiveID)
		{
			if (objectiveParameterID < 0 && objectiveID == oldObjectiveID)
			{
				objectiveID = newObjectiveID;
				return 1;
			}
			return 0;
		}

		#endif

	}

}