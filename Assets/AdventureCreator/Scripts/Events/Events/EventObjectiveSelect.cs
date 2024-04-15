using UnityEngine;

namespace AC
{

	public class EventObjectiveSelect : EventBase
	{

		[SerializeField] private int objectiveID = -1;


		public override string[] EditorNames { get { return new string[] { "Objective/Select" }; } }
		protected override string EventName { get { return "OnObjectiveSelect"; } }
		protected override string ConditionHelp { get { return "Whenever " + ((objectiveID >= 0) ? GetObjectiveName () : "an Objective") + " is selected."; } }


		public EventObjectiveSelect (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, int _objectiveID)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			objectiveID = _objectiveID;
		}


		public EventObjectiveSelect () {}


		public override void Register ()
		{
			EventManager.OnObjectiveSelect += OnObjectiveSelect;
		}


		public override void Unregister ()
		{
			EventManager.OnObjectiveSelect -= OnObjectiveSelect;
		}


		private void OnObjectiveSelect (Objective objective, ObjectiveState state)
		{
			if (objectiveID < 0 || objectiveID == objective.ID)
			{
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
		}

#endif

	}

}