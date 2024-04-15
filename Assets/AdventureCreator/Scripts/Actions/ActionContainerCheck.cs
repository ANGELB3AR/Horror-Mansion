/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionContainerCheck.cs"
 * 
 *	This action checks to see if a particular inventory item
 *	is inside a container, and performs something accordingly.
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
	public class ActionContainerCheck : ActionCheck
	{

		public int invParameterID = -1;
		public int invID;

		public bool useActive = false;
		public int parameterID = -1;
		public int constantID = 0;
		public Container container;
		protected Container runtimeContainer;

		public bool doCount;
		public int intValue = 1;
		public enum IntCondition { EqualTo, NotEqualTo, LessThan, MoreThan };
		public IntCondition intCondition;

		#if UNITY_EDITOR
		protected InventoryManager inventoryManager;
		#endif

		
		public override ActionCategory Category { get { return ActionCategory.Container; }}
		public override string Title { get { return "Check"; }}
		public override string Description { get { return "Queries the contents of a Container for a stored Item, and reacts accordingly."; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			runtimeContainer = AssignFile <Container> (parameters, parameterID, constantID, container);
			invID = AssignInvItemID (parameters, invParameterID, invID);

			if (useActive)
			{
				runtimeContainer = KickStarter.playerInput.activeContainer;
			}
		}

		
		public override bool CheckCondition ()
		{
			if (runtimeContainer == null)
			{
				return false;
			}

			int count = runtimeContainer.GetCount (invID);
			
			if (doCount)
			{
				switch (intCondition)
				{
					case IntCondition.EqualTo:
						return (count == intValue);

					case IntCondition.NotEqualTo:
						return (count != intValue);

					case IntCondition.LessThan:
						return (count < intValue);

					case IntCondition.MoreThan:
						return (count > intValue);

					default:
						return false;
				}
			}
			
			return (count > 0);
		}
		

		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			inventoryManager = KickStarter.inventoryManager;
			
			if (inventoryManager)
			{
				// Create a string List of the field's names (for the PopUp box)
				if (inventoryManager.items.Count > 0)
				{
					useActive = EditorGUILayout.Toggle ("Affect active container?", useActive);
					if (!useActive)
					{
						ComponentField ("Container:", ref container, ref constantID, parameters, ref parameterID);
					}

					ItemField ("Item to check:", ref invID, parameters, ref invParameterID, "Item to check ID:");

					if (inventoryManager.GetItem (invID) != null && inventoryManager.GetItem (invID).canCarryMultiple)
					{
						doCount = EditorGUILayout.Toggle ("Query count?", doCount);
					
						if (doCount)
						{
							EditorGUILayout.BeginHorizontal ();
							EditorGUILayout.LabelField ("Count is:", GUILayout.MaxWidth (70));
							intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
							intValue = EditorGUILayout.IntField (intValue);
						
							if (intValue < 1)
							{
								intValue = 1;
							}
							EditorGUILayout.EndHorizontal ();
						}
					}
					else
					{
						doCount = false;
					}
				}

				else
				{
					EditorGUILayout.LabelField ("No inventory items exist!");
					invID = -1;
				}
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			constantID = AssignConstantID<Container> (container, constantID, parameterID);
		}

		
		public override string SetLabel ()
		{
			if (inventoryManager == null)
			{
				inventoryManager = KickStarter.inventoryManager;
			}

			if (inventoryManager != null)
			{
				if (inventoryManager.GetItem (invID) != null && inventoryManager.GetItem (invID) != null)
				{
					return inventoryManager.GetItem (invID).label;
				}
			}
			
			return string.Empty;
		}


		public override bool ReferencesObjectOrID (GameObject _gameObject, int id)
		{
			if (!useActive && parameterID < 0)
			{
				if (container && container.gameObject == _gameObject) return true;
				if (constantID == id) return true;
			}
			return base.ReferencesObjectOrID (_gameObject, id);
		}

		#endif


		/**
		* <summary>Creates a new instance of the 'Containter: Check' Action</summary>
		* <param name = "container">The Container to search</param>
		* <param name = "itemID">The ID of the inventory item to check the presence of</param>
		* <returns>The generated Action</returns>
		*/
		public static ActionContainerCheck CreateNew (Container container, int itemID)
		{
			ActionContainerCheck newAction = CreateNew<ActionContainerCheck> ();
			newAction.container = container;
			newAction.TryAssignConstantID (newAction.container, ref newAction.constantID);
			newAction.invID = itemID;
			return newAction;
		}
		
	}

}