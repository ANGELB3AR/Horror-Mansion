/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionVarProperty.cs"
 * 
 *	This action is used to set the value of a Variable as an Inventory item property.
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
	public class ActionInvProperty : Action
	{
		
		public int varParameterID = -1;
		public int variableID;
		public VariableLocation varLocation;

		public Method method = Method.PropertyToVariable;
		public enum Method { PropertyToVariable, VariableToProperty };

		#if UNITY_EDITOR
		private VariableType varType = VariableType.Boolean;
		private VariableType propertyType = VariableType.Boolean;
		#endif

		protected enum SetVarAsPropertyMethod { SpecificItem, SelectedItem, LastSelectedItem };
		[SerializeField] protected SetVarAsPropertyMethod setVarAsPropertyMethod = SetVarAsPropertyMethod.SpecificItem;

		public bool useLiveValues;
		public enum ItemSource { NoSource, PlayerInventory, Container };
		public ItemSource itemSource = ItemSource.NoSource;

		public Container container;
		public int containerConstantID = 0;
		public int containerParameterID = -1;
		private Container runtimeContainer;

		public bool multiplyByItemCount;
		public int invID;
		public int invParameterID = -1;

		public int propertyID;

		protected LocalVariables localVariables;
		protected InventoryManager inventoryManager;

		public Variables variables;
		public int variablesConstantID = 0;

		protected GVar runtimeVariable;


		public override ActionCategory Category { get { return ActionCategory.Inventory; }}
		public override string Title { get { return "Property to Variable"; }}
		public override string Description { get { return "Sets the value of a Variable as an Inventory item property."; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			invID = AssignInvItemID (parameters, invParameterID, invID);

			runtimeVariable = null;
			switch (varLocation)
			{
				case VariableLocation.Global:
					variableID = AssignVariableID (parameters, varParameterID, variableID);
					runtimeVariable = GlobalVariables.GetVariable (variableID, true);
					break;

				case VariableLocation.Local:
					if (!isAssetFile)
					{
						variableID = AssignVariableID (parameters, varParameterID, variableID);
						runtimeVariable = LocalVariables.GetVariable (variableID, localVariables);
					}
					break;

				case VariableLocation.Component:
					Variables runtimeVariables = AssignFile <Variables> (variablesConstantID, variables);
					if (runtimeVariables != null)
					{
						runtimeVariable = runtimeVariables.GetVariable (variableID);
					}
					runtimeVariable = AssignVariable (parameters, varParameterID, runtimeVariable);
					break;
			}

			if (itemSource == ItemSource.Container)
			{
				runtimeContainer = AssignFile (parameters, containerParameterID, containerConstantID, container);
			}
		}


		public override void Upgrade ()
		{
			if (useLiveValues)
			{
				useLiveValues = false;
				itemSource = ItemSource.PlayerInventory;
			}
			base.Upgrade ();
		}


		public override void AssignParentList (ActionList actionList)
		{
			if (actionList != null)
			{
				localVariables = UnityVersionHandler.GetLocalVariablesOfGameObject (actionList.gameObject);
			}
			if (localVariables == null)
			{
				localVariables = KickStarter.localVariables;
			}

			base.AssignParentList (actionList);
		}


		public override float Run ()
		{
			switch (method)
			{
				case Method.PropertyToVariable:
				default:
					RunPropertyToVariable ();
					break;

				case Method.VariableToProperty:
					RunVariableToProperty ();
					break;
			}
			
			return 0f;
		}


		private void RunPropertyToVariable ()
		{
			InvInstance invInstance = null;

			switch (setVarAsPropertyMethod)
			{
				case SetVarAsPropertyMethod.SelectedItem:
					if (itemSource == ItemSource.PlayerInventory)
					{
						if (InvInstance.IsValid (KickStarter.runtimeInventory.SelectedInstance))
						{
							int selectedItemID = KickStarter.runtimeInventory.SelectedInstance.ItemID;
							invInstance = new InvInstance (KickStarter.inventoryManager.GetItem (selectedItemID));
						}
						else
						{
							LogWarning ("No Inventory item currently selected");
							return;
						}
					}
					else if (itemSource == ItemSource.Container)
					{
						if (runtimeContainer)
						{
							if (InvInstance.IsValid (KickStarter.runtimeInventory.SelectedInstance))
							{
								int selectedItemID = KickStarter.runtimeInventory.SelectedInstance.ItemID;
								invInstance = runtimeContainer.InvCollection.GetFirstInstance (selectedItemID);
							}
							else
							{
								LogWarning ("No Inventory item currently selected");
								return;
							}
						}
						else
						{
							LogWarning ("Container not set");
						}
					}
					else if (itemSource == ItemSource.NoSource)
					{
						invInstance = KickStarter.runtimeInventory.SelectedInstance;
					}
					break;

				case SetVarAsPropertyMethod.LastSelectedItem:
					if (itemSource == ItemSource.PlayerInventory)
					{
						if (InvInstance.IsValid (KickStarter.runtimeInventory.LastSelectedInstance))
						{
							int lastSelectedItemID = KickStarter.runtimeInventory.LastSelectedInstance.ItemID;
							invInstance = new InvInstance (KickStarter.inventoryManager.GetItem (lastSelectedItemID));
						}
						else
						{
							LogWarning ("No Inventory item currently selected");
							return;
						}
					}
					else if (itemSource == ItemSource.Container)
					{
						if (runtimeContainer)
						{
							if (InvInstance.IsValid (KickStarter.runtimeInventory.LastSelectedInstance))
							{
								int lastSelectedItemID = KickStarter.runtimeInventory.LastSelectedInstance.ItemID;
								invInstance = runtimeContainer.InvCollection.GetFirstInstance (lastSelectedItemID);
							}
							else
							{
								LogWarning ("No Inventory item currently selected");
								return;
							}
						}
						else
						{
							LogWarning ("Container not set");
						}
					}
					else if (itemSource == ItemSource.NoSource)
					{
						invInstance = KickStarter.runtimeInventory.LastSelectedInstance;
					}
					break;

				case SetVarAsPropertyMethod.SpecificItem:
				default:
					if (itemSource == ItemSource.PlayerInventory)
					{
						invInstance = KickStarter.runtimeInventory.GetInstance (invID);
					}
					else if (itemSource == ItemSource.Container)
					{
						if (runtimeContainer)
						{
							invInstance = runtimeContainer.InvCollection.GetFirstInstance (invID);
						}
						else
						{
							LogWarning ("Container not set");
						}
					}
					else if (itemSource == ItemSource.NoSource)
					{
						invInstance = new InvInstance (KickStarter.inventoryManager.GetItem (invID));
					}
					break;
			}

			if (!InvInstance.IsValid (invInstance))
			{
				if ((setVarAsPropertyMethod == SetVarAsPropertyMethod.SelectedItem || setVarAsPropertyMethod == SetVarAsPropertyMethod.LastSelectedItem) && itemSource != ItemSource.NoSource)
				{
					LogWarning ("No Inventory item currently selected");
				}
				else if (itemSource == ItemSource.PlayerInventory)
				{
					LogWarning ("Cannot find Inventory item with ID " + invID + " in the Player's inventory");
				}
				else if (itemSource == ItemSource.Container)
				{
					LogWarning ("Cannot find Inventory item with ID " + invID + " in Container " + runtimeContainer, runtimeContainer);
				}
				else
				{
					LogWarning ("Cannot find Inventory item with ID " + invID);
				}
				return;
			}

			InvVar invVar = invInstance.GetProperty (propertyID);

			if (invVar == null)
			{
				LogWarning ("Cannot find property with ID " + propertyID + " on Inventory item " + invInstance.ItemID);
				return;
			}

			if (runtimeVariable.type == VariableType.String)
			{
				if (runtimeVariable.canTranslate && invVar.type == VariableType.String)
				{
					runtimeVariable.SetStringValue (invVar.TextValue, invVar.textValLineID);
				}
				else
				{
					runtimeVariable.TextValue = invVar.GetDisplayValue (Options.GetLanguage ());
				}
			}
			else if (runtimeVariable.type == invVar.type)
			{
				int itemCount = (itemSource != ItemSource.NoSource && multiplyByItemCount) ? invInstance.Count : 1;

				switch (invVar.type)
				{
					case VariableType.Float:
						runtimeVariable.FloatValue = invVar.FloatValue * (float) itemCount;
						break;

					case VariableType.Integer:
						runtimeVariable.IntegerValue = invVar.IntegerValue * itemCount;
						break;

					case VariableType.Vector3:
						runtimeVariable.Vector3Value = invVar.Vector3Value;
						break;

					case VariableType.GameObject:
						runtimeVariable.GameObjectValue = invVar.GameObjectValue;
						break;

					case VariableType.UnityObject:
						runtimeVariable.UnityObjectValue = invVar.UnityObjectValue;
						break;

					default:
						runtimeVariable.IntegerValue = invVar.IntegerValue;
						break;
				}
			}
			else
			{
				LogWarning ("Cannot assign " + varLocation.ToString () + " Variable " + runtimeVariable.label + "'s value from '" + invVar.label + "' property because their types do not match.");
			}
		}


		private void RunVariableToProperty ()
		{
			switch (setVarAsPropertyMethod)
			{
				case SetVarAsPropertyMethod.SelectedItem:
					{
						InvInstance selectedInstance = KickStarter.runtimeInventory.SelectedInstance;

						if (!InvInstance.IsValid (selectedInstance))
						{
							LogWarning ("No Inventory item currently selected");
							return;
						}

						RunVariableToProperty (selectedInstance);
					}
					break;

				case SetVarAsPropertyMethod.LastSelectedItem:
					{
						InvInstance lastSelectedInstance = KickStarter.runtimeInventory.LastSelectedInstance;

						if (!InvInstance.IsValid (lastSelectedInstance))
						{
							LogWarning ("No Inventory item currently selected");
							return;
						}

						RunVariableToProperty (lastSelectedInstance);
					}
					break;

				case SetVarAsPropertyMethod.SpecificItem:
				default:
					{
						InvInstance[] invInstances = KickStarter.runtimeInventory.GetInstances (invID);
						foreach (InvInstance invInstance in invInstances)
						{
							RunVariableToProperty (invInstance);
						}
					}
					break;
			}
		}


		private void RunVariableToProperty (InvInstance invInstance)
		{
			InvVar invVar = invInstance.GetProperty (propertyID);

			if (!InvInstance.IsValid (invInstance))
			{
				return;
			}

			if (invVar == null)
			{
				LogWarning ("Cannot find property with ID " + propertyID + " on Inventory item ID " + invInstance.ItemID);
				return;
			}

			if (runtimeVariable.type == invVar.type)
			{
				switch (invVar.type)
				{
					case VariableType.String:
						if (runtimeVariable.canTranslate && runtimeVariable.type == VariableType.String)
						{
							invVar.SetStringValue (runtimeVariable.TextValue, runtimeVariable.textValLineID);
						}
						else
						{
							invVar.TextValue = runtimeVariable.TextValue;
						}
						break;

					case VariableType.Float:
						invVar.FloatValue = runtimeVariable.FloatValue;
						break;

					case VariableType.Integer:
						invVar.IntegerValue = runtimeVariable.IntegerValue;
						break;

					case VariableType.Vector3:
						invVar.Vector3Value = runtimeVariable.Vector3Value;
						break;

					case VariableType.GameObject:
						invVar.GameObjectValue = runtimeVariable.GameObjectValue;
						break;

					case VariableType.UnityObject:
						invVar.UnityObjectValue = runtimeVariable.UnityObjectValue;
						break;

					default:
						invVar.IntegerValue = runtimeVariable.IntegerValue;
						break;
				}
			}
			else
			{
				LogWarning ("Cannot assign " + varLocation.ToString () + " Variable " + runtimeVariable.label + "'s value from '" + invVar.label + "' property because their types do not match.");
			}
		}


		#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			inventoryManager = KickStarter.inventoryManager;
			method = (Method) EditorGUILayout.EnumPopup ("Method:", method);
			string getSet = (method == Method.PropertyToVariable) ? "Get" : "Set";

			// Select Inventory item

			setVarAsPropertyMethod = (SetVarAsPropertyMethod) EditorGUILayout.EnumPopup (getSet + " property of:", setVarAsPropertyMethod);
			if (setVarAsPropertyMethod == SetVarAsPropertyMethod.SpecificItem)
			{
				ShowInvSelectGUI (parameters);
			}

			// Select item property
			ShowInvPropertyGUI (parameters);

			EditorGUILayout.Space ();

			// Select variable
			varLocation = (VariableLocation) EditorGUILayout.EnumPopup ("Variable source:", varLocation);
			switch (varLocation)
			{
				case VariableLocation.Global:
					if (KickStarter.variablesManager)
					{
						GlobalVariableField ("Global variable:", ref variableID, null, parameters, ref varParameterID);
						foreach (GVar var in KickStarter.variablesManager.vars)
						{
							if (var.id == variableID) varType = var.type;
						}
					}
					break;

				case VariableLocation.Local:
					if (!isAssetFile)
					{
						if (localVariables)
						{
							LocalVariableField ("Global variable:", ref variableID, null, parameters, ref varParameterID);
							foreach (GVar var in localVariables.localVars)
							{
								if (var.id == variableID) varType = var.type;
							}
						}
						else
						{
							EditorGUILayout.HelpBox ("No 'Local Variables' component found in the scene. Please add an AC GameEngine object from the Scene Manager.", MessageType.Info);
						}
					}
					else
					{
						EditorGUILayout.HelpBox ("Local variables cannot be accessed in ActionList assets.", MessageType.Info);
					}
					break;

				case VariableLocation.Component:
					ComponentVariableField ("Component variable:", ref variables, ref variablesConstantID, ref variableID, null, parameters, ref varParameterID);
					varParameterID = Action.ChooseParameterGUI ("Component variable:", parameters, varParameterID, ParameterType.ComponentVariable);
					if (varParameterID < 0 && variables)
					{
						foreach (GVar var in variables.vars)
						{
							if (var.id == variableID) varType = var.type;
						}
					}
					break;
			}

			if (inventoryManager != null && inventoryManager.invVars != null && inventoryManager.invVars.Count > 0)
			{
				if (varType != propertyType && varType != VariableType.String)
				{
					EditorGUILayout.HelpBox ("The chosen Inventory Item Property and Variable must share the same Type, or the Variable must be a String.", MessageType.Warning);
				}
			}
		}


		private void ShowInvPropertyGUI (List<ActionParameter> parameters)
		{
			if (inventoryManager != null)
			{
				// Create a string List of the field's names (for the PopUp box)
				List<string> labelList = new List<string>();

				int i = 0;
				int propertyNumber = 0;

				if (inventoryManager.invVars != null && inventoryManager.invVars.Count > 0)
				{
					foreach (InvVar invVar in inventoryManager.invVars)
					{
						labelList.Add (invVar.label);
						
						// If a property has been removed, make sure selected variable is still valid
						if (invVar.id == propertyID)
						{
							propertyNumber = i;
						}
						
						i++;
					}
					
					if (propertyNumber == -1)
					{
						// Wasn't found (property was possibly deleted), so revert to zero
						if (propertyID > 0) LogWarning ("Previously chosen property no longer exists!");
						
						propertyNumber = 0;
						propertyID = 0;
					}

					propertyNumber = EditorGUILayout.Popup ("Inventory property:", propertyNumber, labelList.ToArray());
					propertyID = inventoryManager.invVars[propertyNumber].id;
					propertyType = inventoryManager.invVars[propertyNumber].type;

					if (method == Method.PropertyToVariable)
					{
						itemSource = (ItemSource) EditorGUILayout.EnumPopup ("Item source:", itemSource);
						if (itemSource != ItemSource.NoSource)
						{
							if (propertyType == VariableType.Integer || propertyType == VariableType.Float)
							{
								multiplyByItemCount = EditorGUILayout.Toggle ("Multiply by item count?", multiplyByItemCount);
							}

							if (itemSource == ItemSource.Container)
							{
								ComponentField ("Container:", ref container, ref containerConstantID, parameters, ref containerParameterID);
							}
						}
					}
				}
				else
				{
					EditorGUILayout.HelpBox ("No inventory properties exist!", MessageType.Info);
					propertyID = -1;
					propertyNumber = -1;
				}
			}
		}


		private void ShowInvSelectGUI (List<ActionParameter> parameters)
		{
			if (inventoryManager != null)
			{
				ItemField (ref invID, parameters, ref invParameterID);
			}
		}


		public override string SetLabel ()
		{
			if (varParameterID < 0)
			{
				switch (varLocation)
				{
					case VariableLocation.Global:
						if (KickStarter.variablesManager)
						{
							return GetLabelString (KickStarter.variablesManager.vars, variableID);
						}
						break;

					case VariableLocation.Local:
						if (!isAssetFile && localVariables != null)
						{
							return GetLabelString (localVariables.localVars, variableID);
						}
						break;

					case VariableLocation.Component:
						if (variables != null)
						{
							return GetLabelString (variables.vars, variableID);
						}
						break;
				}
			}

			return string.Empty;
		}


		private string GetLabelString (List<GVar> vars, int variableID)
		{
			if (vars.Count > 0)
			{
				foreach (GVar _var in vars)
				{
					if (_var.id == variableID)
					{
						return _var.label;
					}
				}
			}
			return string.Empty;
		}


		public override bool ConvertLocalVariableToGlobal (int oldLocalID, int newGlobalID)
		{
			bool wasAmended = base.ConvertLocalVariableToGlobal (oldLocalID, newGlobalID);

			if (varLocation == VariableLocation.Local && variableID == oldLocalID)
			{
				varLocation = VariableLocation.Global;
				variableID = newGlobalID;
				wasAmended = true;
			}

			return wasAmended;
		}


		public override bool ConvertGlobalVariableToLocal (int oldGlobalID, int newLocalID, bool isCorrectScene)
		{
			bool wasAmended = base.ConvertGlobalVariableToLocal (oldGlobalID, newLocalID, isCorrectScene);

			if (varLocation == VariableLocation.Global && variableID == oldGlobalID)
			{
				wasAmended = true;
				if (isCorrectScene)
				{
					varLocation = VariableLocation.Local;
					variableID = newLocalID;
				}
			}

			return wasAmended;
		}


		public override int GetNumVariableReferences (VariableLocation _location, int varID, List<ActionParameter> parameters, Variables _variables = null, int _variablesConstantID = 0)
		{
			int thisCount = 0;

			if (varLocation == _location && variableID == varID && varParameterID < 0)
			{
				if (_location != VariableLocation.Component || (variables != null && variables == _variables) || (variablesConstantID != 0 && _variablesConstantID == variablesConstantID))
				{
					thisCount ++;
				}
			}

			thisCount += base.GetNumVariableReferences (_location, varID, parameters, _variables, _variablesConstantID);
			return thisCount;
		}


		public override int UpdateVariableReferences (VariableLocation _location, int oldVarID, int newVarID, List<ActionParameter> parameters, Variables _variables = null, int _variablesConstantID = 0)
		{
			int thisCount = 0;

			if (varLocation == _location && variableID == oldVarID && varParameterID < 0)
			{
				if (_location != VariableLocation.Component || (variables != null && variables == _variables) || (variablesConstantID != 0 && _variablesConstantID == variablesConstantID))
				{
					variableID = newVarID;
					thisCount++;
				}
			}

			thisCount += base.UpdateVariableReferences (_location, oldVarID, newVarID, parameters, _variables, _variablesConstantID);
			return thisCount;
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (varLocation == VariableLocation.Component)
			{
				variablesConstantID = AssignConstantID<Variables> (variables, variablesConstantID, varParameterID);
			}
		}


		public override bool ReferencesObjectOrID (GameObject _gameObject, int id)
		{
			if (varLocation == VariableLocation.Component && varParameterID < 0)
			{
				if (variables && variables.gameObject == _gameObject) return true;
				if (variablesConstantID == id) return true;
			}
			if (itemSource == ItemSource.Container && containerParameterID < 0)
			{
				if (container && container.gameObject == _gameObject) return true;
				if (containerConstantID == id) return true;
			}
			return base.ReferencesObjectOrID (_gameObject, id);
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Inventory: Property to Variable' Action, set to transfer a property value to a Global variable</summary>
		 * <param name = "variableID">The ID number of the Global variable to update</param>
		 * <param name = "propertyID">The ID number of the inventory property to read</param>
		 * <param name = "itemID">If non-negative, the ID number of the inventory item to read.  Otherwise, the currently-selected item will be read</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionInvProperty CreateNew_ToGlobalVariable (int variableID, int propertyID, int itemID = -1)
		{
			ActionInvProperty newAction = CreateNew<ActionInvProperty> ();
			newAction.setVarAsPropertyMethod = (itemID >= 0) ? SetVarAsPropertyMethod.SpecificItem : SetVarAsPropertyMethod.SelectedItem;
			newAction.propertyID = propertyID;
			newAction.varLocation = VariableLocation.Global;
			newAction.variableID = variableID;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Inventory: Property to Variable' Action, set to transfer a property value to a Local variable</summary>
		 * <param name = "variableID">The ID number of the Local variable to update</param>
		 * <param name = "propertyID">The ID number of the inventory property to read</param>
		 * <param name = "itemID">If non-negative, the ID number of the inventory item to read.  Otherwise, the currently-selected item will be read</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionInvProperty CreateNew_ToLocalVariable (int variableID, int propertyID, int itemID = -1)
		{
			ActionInvProperty newAction = CreateNew<ActionInvProperty> ();
			newAction.setVarAsPropertyMethod = (itemID >= 0) ? SetVarAsPropertyMethod.SpecificItem : SetVarAsPropertyMethod.SelectedItem;
			newAction.propertyID = propertyID;
			newAction.varLocation = VariableLocation.Local;
			newAction.variableID = variableID;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Inventory: Property to Variable' Action, set to transfer a property value to a Component variable</summary>
		 * <param name = "variables">The Variables component that holds the variable</param>
		 * <param name = "variableID">The ID number of the Component variable to update</param>
		 * <param name = "propertyID">The ID number of the inventory property to read</param>
		 * <param name = "itemID">If non-negative, the ID number of the inventory item to read.  Otherwise, the currently-selected item will be read</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionInvProperty CreateNew_ToComponentVariable (Variables variables, int variableID, int propertyID, int itemID = -1)
		{
			ActionInvProperty newAction = CreateNew<ActionInvProperty> ();
			newAction.setVarAsPropertyMethod = (itemID >= 0) ? SetVarAsPropertyMethod.SpecificItem : SetVarAsPropertyMethod.SelectedItem;
			newAction.propertyID = propertyID;
			newAction.varLocation = VariableLocation.Component;
			newAction.variables = variables;
			newAction.variableID = variableID;
			return newAction;
		}

	}

}