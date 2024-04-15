#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (Variables))]
	public class VariablesEditor : Editor
	{

		private bool showVariablesList = true;
		private bool showSettings = true;
		private bool showProperties = true;

		private VariableType typeFilter;
		private VarFilter varFilter;

		private GVar selectedVar = null;
		private Variables _target;

		private GVar lastDragOver;
		private int lastSwapIndex;
		private const string DragKey = "AC.ComponentVariables";
		private bool ignoreDrag;


		public override void OnInspectorGUI ()
		{
			_target = (Variables) target;

			CustomGUILayout.UpdateDrag (DragKey, lastDragOver, lastDragOver != null ? lastDragOver.label : string.Empty, ref ignoreDrag, OnCompleteDrag);
			ShowSettings ();

			EditorGUILayout.Space ();
			showVariablesList = CustomGUILayout.ToggleHeader (showVariablesList, "Component variables");
			if (showVariablesList)
			{
				CustomGUILayout.BeginVertical ();
				selectedVar = VariablesManager.ShowVarList (selectedVar, _target.vars, VariableLocation.Component, varFilter, _target.filter, typeFilter, !Application.isPlaying, _target, ref lastDragOver, ref lastSwapIndex, IsDragging ());
				CustomGUILayout.EndVertical ();
			}

			if (selectedVar != null && !_target.vars.Contains (selectedVar))
			{
				selectedVar = null;
			}

			if (selectedVar != null)
			{
				EditorGUILayout.Space ();
				EditorGUILayout.BeginVertical (CustomStyles.thinBox);
				showProperties = CustomGUILayout.ToggleHeader (showProperties, "Variable '" + selectedVar.label + "' properties");
				if (showProperties)
				{
					VariablesManager.ShowVarGUI (selectedVar, VariableLocation.Component, !Application.isPlaying, null, string.Empty, _target);
				}
				CustomGUILayout.EndVertical ();
			}

			UnityVersionHandler.CustomSetDirty (_target);
		}


		private void ShowSettings ()
		{
			showSettings = CustomGUILayout.ToggleHeader (showSettings, "Editor settings");
			if (showSettings)
			{
				CustomGUILayout.BeginVertical ();
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Filter by:", GUILayout.Width (65f));
				varFilter = (VarFilter) EditorGUILayout.EnumPopup (varFilter, GUILayout.MaxWidth (100f));
				if (varFilter == VarFilter.Type)
				{
					typeFilter = (VariableType) EditorGUILayout.EnumPopup (typeFilter);
				}
				else if (varFilter == VarFilter.ID)
				{
					int filterID = -1;
					int.TryParse (_target.filter, out filterID);

					filterID = EditorGUILayout.IntField (filterID);
					_target.filter = filterID.ToString ();
				}
				else
				{
					_target.filter = EditorGUILayout.TextField (_target.filter);
				}
				EditorGUILayout.EndHorizontal ();
				CustomGUILayout.EndVertical ();
			}
		}


		private void OnCompleteDrag (object data)
		{
			GVar variable = (GVar) data;
			if (variable == null) return;

			int dragIndex = _target.vars.IndexOf (variable);
			if (dragIndex >= 0 && lastSwapIndex >= 0)
			{
				GVar tempItem = variable;

				_target.vars.RemoveAt (dragIndex);

				if (lastSwapIndex > dragIndex)
				{
					_target.vars.Insert (lastSwapIndex - 1, tempItem);
				}
				else
				{
					_target.vars.Insert (lastSwapIndex, tempItem);
				}

				Event.current.Use ();
				EditorUtility.SetDirty (_target);
			}

			selectedVar = variable;
		}


		private bool IsDragging ()
		{
			object dragObject = DragAndDrop.GetGenericData (DragKey);
			if (dragObject != null && dragObject is GVar)
			{
				return true;
			}
			return false;
		}

	}

}

#endif