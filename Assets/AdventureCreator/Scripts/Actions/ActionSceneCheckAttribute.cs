/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionSceneCheckAttribute.cs"
 * 
 *	This action checks to see if a scene attribute has been assigned a certain value,
 *	and performs something accordingly.
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
	public class ActionSceneCheckAttribute : ActionCheck
	{

		public int attributeID;
		public int attributeNumber;

		public int intValue;
		public float floatValue;
		public IntCondition intCondition;
		public bool isAdditive = false;
		
		public BoolValue boolValue = BoolValue.True;
		public BoolCondition boolCondition;

		public string stringValue;
		public bool checkCase = true;

		public Vector3 vector3Value;
		public VectorCondition vectorCondition = VectorCondition.EqualTo;

		public GameObject gameObjectValue;
		protected GameObject runtimeGameObjectValue;

		public Object unityObjectValue;
		protected Object runtimeUnityObjectValue;

		public int checkParameterID = -1;
		protected ActionParameter checkParameter;

		protected SceneSettings sceneSettings;

		
		public override ActionCategory Category { get { return ActionCategory.Scene; }}
		public override string Title { get { return "Check attribute"; }}
		public override string Description { get { return "Queries the value of a scene attribute declared in the Scene Manager."; }}


		public override void AssignParentList (ActionList actionList)
		{
			if (sceneSettings == null)
			{
				sceneSettings = KickStarter.sceneSettings;
			}

			base.AssignParentList (actionList);
		}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			boolValue = AssignBoolean (parameters, checkParameterID, boolValue);
			intValue = AssignInteger (parameters, checkParameterID, intValue);
			floatValue = AssignFloat (parameters, checkParameterID, floatValue);
			vector3Value = AssignVector3 (parameters, checkParameterID, vector3Value);
			stringValue = AssignString (parameters, checkParameterID, stringValue);

			runtimeGameObjectValue = AssignFile (parameters, checkParameterID, intValue, gameObjectValue);
			runtimeUnityObjectValue = AssignObject<Object> (parameters, checkParameterID, unityObjectValue);
		}


		public override int GetNextOutputIndex ()
		{
			if (attributeID == -1)
			{
				return -1;
			}

			InvVar attribute = sceneSettings.GetAttribute (attributeID);
			if (attribute != null)
			{
				return CheckCondition (attribute) ? 0 : 1;
			}

			LogWarning ("Cannot find the scene attribute with an ID of " + attributeID);
			return -1;
		
		}
		
		
		protected bool CheckCondition (InvVar attribute)
		{
			if (attribute == null)
			{
				LogWarning ("Cannot check state of attribute since it cannot be found!");
				return false;
			}

			switch (attribute.type)
			{
				case VariableType.Boolean:
				{
					int fieldValue = attribute.IntegerValue;
					int compareValue = (int) boolValue;

					if (boolCondition == BoolCondition.EqualTo)
					{
						return (fieldValue == compareValue);
					}
					else
					{
						return (fieldValue != compareValue);
					}
				}

				case VariableType.Integer:
				case VariableType.PopUp:
				{
					int fieldValue = attribute.IntegerValue;
					int compareValue = intValue;

					if (intCondition == IntCondition.EqualTo)
					{
						return (fieldValue == compareValue);
					}
					else if (intCondition == IntCondition.NotEqualTo)
					{
						return (fieldValue != compareValue);
					}
					else if (intCondition == IntCondition.LessThan)
					{
						return (fieldValue < compareValue);
					}
					else if (intCondition == IntCondition.MoreThan)
					{
						return (fieldValue > compareValue);
					}
					break;
				}

				case VariableType.Float:
				{
					float fieldValue = attribute.FloatValue;
					float compareValue = floatValue;

					if (intCondition == IntCondition.EqualTo)
					{
						return (Mathf.Approximately (fieldValue, compareValue));
					}
					else if (intCondition == IntCondition.NotEqualTo)
					{
						return (!Mathf.Approximately (fieldValue, compareValue));
					}
					else if (intCondition == IntCondition.LessThan)
					{
						return (fieldValue < compareValue);
					}
					else if (intCondition == IntCondition.MoreThan)
					{
						return (fieldValue > compareValue);
					}

					break;
				}

				case VariableType.String:
				{
					string fieldValue = attribute.TextValue;
					string compareValue = AdvGame.ConvertTokens (stringValue);

					if (!checkCase)
					{
						fieldValue = fieldValue.ToLower ();
						compareValue = compareValue.ToLower ();
					}

					if (boolCondition == BoolCondition.EqualTo)
					{
						return (fieldValue == compareValue);
					}
					return (fieldValue != compareValue);
				}

				case VariableType.Vector3:
					if (vectorCondition == VectorCondition.EqualTo)
					{
						return vector3Value == attribute.Vector3Value;
					}
					else if (vectorCondition == VectorCondition.MagnitudeGreaterThan)
					{
						return attribute.Vector3Value.magnitude > floatValue;
					}
					break;

				case VariableType.UnityObject:
					if (boolCondition == BoolCondition.EqualTo)
					{
						return runtimeUnityObjectValue == attribute.UnityObjectValue;
					}
					else
					{
						return runtimeUnityObjectValue != attribute.UnityObjectValue;
					}

				case VariableType.GameObject:
					ConstantID fieldConstantID = attribute.GameObjectValue ? attribute.GameObjectValue.GetComponent<ConstantID> () : null;
					ConstantID runtimeConstantID = runtimeGameObjectValue ? runtimeGameObjectValue.GetComponent<ConstantID> () : null;
					if (boolCondition == BoolCondition.EqualTo)
					{
						if (runtimeGameObjectValue == attribute.GameObjectValue)
						{
							return true;
						}
						else if (fieldConstantID && runtimeConstantID && fieldConstantID.constantID != 0 && fieldConstantID.constantID == runtimeConstantID.constantID)
						{
							return true;
						}
					}
					else
					{
						if (runtimeGameObjectValue != attribute.GameObjectValue)
						{
							return true;
						}
						else if (fieldConstantID && runtimeConstantID && fieldConstantID.constantID != 0 && fieldConstantID.constantID != runtimeConstantID.constantID)
						{
							return true;
						}
					}
					return false;

				default:
					break;
			}
			
			return false;
		}

		
		#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			if (KickStarter.settingsManager)
			{
				SettingsManager settingsManager = KickStarter.settingsManager;

				attributeID = ShowVarGUI (settingsManager.sceneAttributes, attributeID, true, parameters);
			}
			else
			{
				EditorGUILayout.HelpBox ("A Settings Manager is required for this Action's GUI to display.", MessageType.Info);
			}
		}


		private int ShowAttributeSelectorGUI (List<InvVar> attributes, int ID)
		{
			attributeNumber = -1;
			
			List<string> labelList = new List<string>();
			foreach (GVar _var in attributes)
			{
				labelList.Add (_var.label);
			}
			
			attributeNumber = GetVarNumber (attributes, ID);
			
			if (attributeNumber == -1)
			{
				// Wasn't found (variable was deleted?), so revert to zero
				if (ID > 0) LogWarning ("Previously chosen attribute no longer exists!");
				attributeNumber = 0;
				ID = 0;
			}

			attributeNumber = EditorGUILayout.Popup ("Attribute:", attributeNumber, labelList.ToArray());
			ID = attributes[attributeNumber].id;

			return ID;
		}


		private int ShowVarGUI (List<InvVar> attributes, int ID, bool changeID, List<ActionParameter> parameters)
		{
			if (attributes.Count > 0)
			{
				if (changeID)
				{
					ID = ShowAttributeSelectorGUI (attributes, ID);
				}

				attributeNumber = Mathf.Min (attributeNumber, attributes.Count-1);

				switch (attributes[attributeNumber].type)
				{
					case VariableType.Boolean:
						boolCondition = (BoolCondition) EditorGUILayout.EnumPopup ("Condition:", boolCondition);
						EnumBoolField ("Value:", ref boolValue, parameters, ref checkParameterID);
						break;

					case VariableType.Integer:
						intCondition = (IntCondition) EditorGUILayout.EnumPopup ("Condition:", intCondition);
						IntField ("Value:", ref intValue, parameters, ref checkParameterID);
						break;

					case VariableType.PopUp:
						intCondition = (IntCondition) EditorGUILayout.EnumPopup ("Condition:", intCondition);
						PopupField ("Value:", ref intValue, attributes[attributeNumber].popUps, parameters, ref checkParameterID);
						break;

					case VariableType.Float:
						intCondition = (IntCondition) EditorGUILayout.EnumPopup ("Condition:", intCondition);
						FloatField ("Value:", ref floatValue, parameters, ref checkParameterID);
						break;

					case VariableType.String:
						boolCondition = (BoolCondition) EditorGUILayout.EnumPopup ("Condition:", boolCondition);
						TextField ("Value:", ref stringValue, parameters, ref checkParameterID);
						break;

					case VariableType.Vector3:
						vectorCondition = (VectorCondition) EditorGUILayout.EnumPopup ("Condition:", vectorCondition);
						if (vectorCondition == VectorCondition.EqualTo)
						{
							Vector3Field ("Value:", ref vector3Value, parameters, ref checkParameterID);
						}
						else if (vectorCondition == VectorCondition.MagnitudeGreaterThan)
						{
							FloatField ("Value:", ref floatValue, parameters, ref checkParameterID);
						}
						break;

					case VariableType.GameObject:
						boolCondition = (BoolCondition) EditorGUILayout.EnumPopup ("Condition:", boolCondition);
						GameObjectField ("Value:", ref gameObjectValue, ref intValue, parameters, ref checkParameterID);
						break;

					case VariableType.UnityObject:
						boolCondition = (BoolCondition) EditorGUILayout.EnumPopup ("Condition:", boolCondition);
						AssetField ("Value:", ref unityObjectValue, parameters, ref checkParameterID);
						break;
				}

				if (attributes [attributeNumber].type == VariableType.String)
				{
					checkCase = EditorGUILayout.Toggle ("Case-senstive?", checkCase);
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("No variables exist!", MessageType.Info);
				ID = -1;
				attributeNumber = -1;
			}

			return ID;
		}


		public override string SetLabel ()
		{
			if (sceneSettings != null)
			{
				return GetLabelString (sceneSettings.attributes);
			}
			return string.Empty;
		}


		private string GetLabelString (List<InvVar> attributes)
		{
			string labelAdd = string.Empty;

			if (attributes.Count > 0 && attributes.Count > attributeNumber && attributeNumber > -1)
			{
				labelAdd = attributes[attributeNumber].label;
				
				if (attributes [attributeNumber].type == VariableType.Boolean)
				{
					labelAdd += " " + boolCondition.ToString () + " " + boolValue.ToString ();
				}
				else if (attributes [attributeNumber].type == VariableType.Integer)
				{
					labelAdd += " " + intCondition.ToString () + " " + intValue.ToString ();
				}
				else if (attributes [attributeNumber].type == VariableType.Float)
				{
					labelAdd += " " + intCondition.ToString () + " " + floatValue.ToString ();
				}
				else if (attributes [attributeNumber].type == VariableType.String)
				{
					labelAdd += " " + boolCondition.ToString () + " " + stringValue;
				}
				else if (attributes [attributeNumber].type == VariableType.PopUp)
				{
					labelAdd += " " + intCondition.ToString () + " " + attributes[attributeNumber].GetPopUpForIndex (intValue);
				}
			}

			return labelAdd;
		}


		public override bool ReferencesObjectOrID (GameObject gameObject, int id)
		{
			if (gameObjectValue && gameObjectValue == gameObject)
			{
				return true;
			}
			return base.ReferencesObjectOrID (gameObject, id);
		}
		
		#endif


		protected int GetVarNumber (List<InvVar> attributes, int ID)
		{
			int i = 0;
			foreach (InvVar attribute in attributes)
			{
				if (attribute.id == ID)
				{
					return i;
				}
				i++;
			}
			return -1;
		}


		/**
		 * <summary>Creates a new instance of the 'Scene: Check attribute' Action, set to check a Bool attribute</summary>
		 * <param name = "attributeID">The ID number of the Bool attribute</param>
		 * <param name = "value">The attribute value to check for</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionSceneCheckAttribute CreateNew (int attributeID, bool value)
		{
			ActionSceneCheckAttribute newAction = CreateNew<ActionSceneCheckAttribute> ();
			newAction.attributeID = attributeID;
			newAction.boolValue = (value) ? BoolValue.True : BoolValue.False;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Scene: Check attribute' Action, set to check an Integer attribute</summary>
		 * <param name = "attributeID">The ID number of the Integer attribute</param>
		 * <param name = "value">The attribute value to check for</param>
		 * <param name = "condition">The condition to query</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionSceneCheckAttribute CreateNew (int attributeID, int value, IntCondition condition = IntCondition.EqualTo)
		{
			ActionSceneCheckAttribute newAction = CreateNew<ActionSceneCheckAttribute> ();
			newAction.attributeID = attributeID;
			newAction.intValue = value;
			newAction.intCondition = condition;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Scene: Check attribute' Action, set to check a Float attribute</summary>
		 * <param name = "attributeID">The ID number of the Float attribute</param>
		 * <param name = "value">The attribute value to check for</param>
		 * <param name = "condition">The condition to query</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionSceneCheckAttribute CreateNew (int attributeID, float value, IntCondition condition = IntCondition.EqualTo)
		{
			ActionSceneCheckAttribute newAction = CreateNew<ActionSceneCheckAttribute> ();
			newAction.attributeID = attributeID;
			newAction.floatValue = value;
			newAction.intCondition = condition;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Scene: Check attribute' Action, set to check a String attribute</summary>
		 * <param name = "attributeID">The ID number of the String attribute</param>
		 * <param name = "value">The attribute value to check for</param>
		 * <param name = "isCaseSensitive">If True, the query will be case-sensitive</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionSceneCheckAttribute CreateNew (int attributeID, string value, bool isCaseSensitive = false)
		{
			ActionSceneCheckAttribute newAction = CreateNew<ActionSceneCheckAttribute> ();
			newAction.attributeID = attributeID;
			newAction.stringValue = value;
			newAction.checkCase = isCaseSensitive;
			return newAction;
		}

	}

}