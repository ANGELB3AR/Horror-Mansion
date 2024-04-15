using UnityEngine;

namespace AC
{

	public class EventVariableChange : EventBase
	{

		[SerializeField] private Variables variables = null;
		[SerializeField] private VariableLocation variableLocation;
		[SerializeField] private int varID = -1;

		public override string[] EditorNames { get { return new string[] { "Variable/Change Global", "Variable/Change Local", "Variable/Change Component" }; } }

		protected override string EventName { get { return "OnVariableChange"; } }
		protected override string ConditionHelp { get { return "Whenever " + (varID >= 0 ? "" : "a ") + variableLocation.ToString () + (varID >= 0 ? " variable " + varID + "'s" : " variable's") + " value is changed."; } }


		public EventVariableChange (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, VariableLocation _variableLocation, Variables _variables, int _varID)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			variables = _variables;
			variableLocation = _variableLocation;
			varID = _varID;
		}


		public EventVariableChange () {}


		public override void Register ()
		{
			EventManager.OnVariableChange += OnVariableChange;
		}


		public override void Unregister ()
		{
			EventManager.OnVariableChange -= OnVariableChange;
		}


		private void OnVariableChange (GVar gVar)
		{
			switch (variableLocation)
			{
				case VariableLocation.Global:
					if (gVar.IsGlobalVariable () && (varID == -1 || varID == gVar.id))
					{
						Run (new object[] { gVar.id });
					}
					break;

				case VariableLocation.Local:
					if (KickStarter.localVariables.localVars.Contains (gVar) && (varID == -1 || varID == gVar.id))
					{
						Run (new object[] { gVar.id });
					}
					break;

				case VariableLocation.Component:
					if (!KickStarter.runtimeVariables.globalVars.Contains (gVar) && !KickStarter.localVariables.localVars.Contains (gVar) && (varID == -1 || varID == gVar.id))
					{
						if (variables == null || variables.vars.Contains (gVar))
						{
							Run (new object[] { gVar });
						}
					}
					break;

				default:
					break;
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			switch (variableLocation)
			{
				case VariableLocation.Global:
					return new ParameterReference[] { new ParameterReference (ParameterType.GlobalVariable, "Global variable") };

				case VariableLocation.Local:
					return new ParameterReference[] { new ParameterReference (ParameterType.LocalVariable, "Local variable") };

				case VariableLocation.Component:
					return new ParameterReference[] { new ParameterReference (ParameterType.ComponentVariable, "Component variable") };

				default:
					return new ParameterReference[0];
			}
		}


#if UNITY_EDITOR

		public override void AssignVariant (int variantIndex)
		{
			variableLocation = (VariableLocation) variantIndex;
		}


		protected override bool HasConditions (bool isAssetFile) { return true; }


		protected override void ShowConditionGUI (bool isAssetFile)
		{
			switch (variableLocation)
			{
				case VariableLocation.Global:
					{
						if (KickStarter.variablesManager)
						{
							varID = ActionRunActionList.ShowVarSelectorGUI ("Global variable:", KickStarter.variablesManager.vars, varID);
						}
						else
						{
							varID = CustomGUILayout.IntField ("Global variable ID:", varID);
						}
					}
					break;

				case VariableLocation.Local:
					if (!isAssetFile)
					{
						if (KickStarter.variablesManager)
						{
							varID = ActionRunActionList.ShowVarSelectorGUI ("Local variable:", KickStarter.localVariables.localVars, varID);
						}
						else
						{
							varID = CustomGUILayout.IntField ("Local variable ID:", varID);
						}
					}
					break;

				case VariableLocation.Component:
					if (!isAssetFile)
					{
						variables = (Variables) CustomGUILayout.ObjectField<Variables> ("Variables:", variables, true);
						if (variables)
						{
							varID = ActionRunActionList.ShowVarSelectorGUI ("Variable:", variables.vars, varID);
						}
					}
					break;

				default:
					break;
			}
		}


#endif

	}

}