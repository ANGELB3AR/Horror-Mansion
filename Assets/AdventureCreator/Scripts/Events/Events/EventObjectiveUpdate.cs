using UnityEngine;

namespace AC
{

	public class EventObjectiveUpdate : EventBase
	{

		[SerializeField] private int objectiveID = -1;
		[SerializeField] private ObjectiveStateCondition objectiveStateCondition = ObjectiveStateCondition.Any;
		public enum ObjectiveStateCondition { Any, Started, Updated, Completed, Failed };


		public override string[] EditorNames { get { return new string[] { "Objective/Update" }; } }
		protected override string EventName { get { return "OnObjectiveUpdate"; } }
		protected override string ConditionHelp { get { return "Whenever " + ((objectiveID >= 0) ? GetObjectiveName () : "an Objective") + " is updated."; } }


		public EventObjectiveUpdate (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, int _objectiveID, ObjectiveStateCondition _objectiveStateCondition = ObjectiveStateCondition.Any)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			objectiveID = _objectiveID;
			objectiveStateCondition = _objectiveStateCondition;
		}


		public EventObjectiveUpdate () {}


		public override void Register ()
		{
			EventManager.OnObjectiveUpdate += OnObjectiveUpdate;
		}


		public override void Unregister ()
		{
			EventManager.OnObjectiveUpdate -= OnObjectiveUpdate;
		}


		private void OnObjectiveUpdate (Objective objective, ObjectiveState state)
		{
			if (objectiveID < 0 || objectiveID == objective.ID)
			{
				switch (objectiveStateCondition)
				{
					case ObjectiveStateCondition.Started:
						if (state.stateType == ObjectiveStateType.Active)
						{
							ObjectiveInstance objectiveInstance = KickStarter.runtimeObjectives.GetObjective (objective.ID);
							if (objectiveInstance != null && objectiveInstance.PreviousStateID >= 0)
							{
								return;
							}
						}
						break;

					case ObjectiveStateCondition.Updated:
						if (state.stateType == ObjectiveStateType.Active)
						{
							ObjectiveInstance objectiveInstance = KickStarter.runtimeObjectives.GetObjective (objective.ID);
							if (objectiveInstance != null && objectiveInstance.PreviousStateID < 0)
							{
								return;
							}
						}
						break;

					case ObjectiveStateCondition.Completed:
						if (state.stateType != ObjectiveStateType.Complete)
						{
							return;
						}
						break;

					case ObjectiveStateCondition.Failed:
						if (state.stateType != ObjectiveStateType.Fail)
						{
							return;
						}
						break;

					default:
						break;
				}

				Run (new object[] { objective.ID });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.Objective, "Objective")
			};
		}


		private string GetObjectiveName ()
		{
			if (KickStarter.inventoryManager)
			{
				Objective objective = KickStarter.inventoryManager.GetObjective (objectiveID);
				if (objective != null) return "objective '" + objective.Title + "'";
			}
			return "objective " + objectiveID;
		}


#if UNITY_EDITOR

		protected override bool HasConditions (bool isAssetFile) { return true; }


		protected override void ShowConditionGUI (bool isAssetFile)
		{
			if (KickStarter.inventoryManager)
			{
				objectiveID = ActionRunActionList.ShowObjectiveSelectorGUI ("Objective:", KickStarter.inventoryManager.objectives, objectiveID);
			}
			else
			{
				objectiveID = CustomGUILayout.IntField ("Objective ID:", objectiveID);
			}
			objectiveStateCondition = (ObjectiveStateCondition) CustomGUILayout.EnumPopup ("State condition:", objectiveStateCondition);
		}

#endif

	}

}