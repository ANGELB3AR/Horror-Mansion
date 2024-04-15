/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionInputSimulate.cs"
 * 
 *	This action simulates the pressing of an input or axis.
 * 
 */

using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionInputSimulate : Action
	{

		public string inputAxis;
		public int inputAxisParameterID = -1;
		public SimulateInputType simulateInput = SimulateInputType.Button;
		public float simulateValue = 1f;


		public override ActionCategory Category { get { return ActionCategory.Input; } }
		public override string Title { get { return "Simulate"; } }
		public override string Description { get { return "Simulates the pressing of an input button or axis."; } }


		public override void AssignValues (List<ActionParameter> parameters)
		{
			inputAxis = AssignString (parameters, inputAxisParameterID, inputAxis);
		}


		public override float Run ()
		{
			KickStarter.playerInput.SimulateInput (simulateInput, inputAxis, simulateValue);
			return 0f;
		}


#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			simulateInput = (SimulateInputType) EditorGUILayout.EnumPopup ("Simulate:", simulateInput);

			TextField ("Input axis:", ref inputAxis, parameters, ref inputAxisParameterID);

			if (simulateInput == SimulateInputType.Axis)
			{
				simulateValue = EditorGUILayout.FloatField ("Input value:", simulateValue);
			}
		}


		public override string SetLabel ()
		{
			return inputAxis;
		}

#endif

	}

}