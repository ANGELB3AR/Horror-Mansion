/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionContainerOpen.cs"
 * 
 *	This action makes a Container active for display in an
 *	InventoryBox. To de-activate it, close a Menu with AppearType
 *	set to OnContainer.
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
	public class ActionContainerOpen : Action
	{

		public bool useActive = false;
		public int parameterID = -1;
		public int constantID = 0;
		public Container container;
		protected Container runtimeContainer;

		public bool setElement;
		public string menuName;
		public string containerElementName;

		public int menuParameterID = -1;
		public int elementParameterID = -1;

		protected LocalVariables localVariables;
		protected MenuInventoryBox runtimeInventoryBox;
		

		public override ActionCategory Category { get { return ActionCategory.Container; }}
		public override string Title { get { return "Open"; }}
		public override string Description { get { return "Opens a chosen Container, causing any Menu of Appear type: On Container to open. To close the Container, simply close the Menu."; }}
		public override int NumSockets { get { return (!useActive && setElement) ? 1 : 0; }}


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


		public override void AssignValues (List<ActionParameter> parameters)
		{
			if (useActive)
			{
				runtimeContainer = KickStarter.playerInput.activeContainer;
			}
			else
			{
				runtimeContainer = AssignFile <Container> (parameters, parameterID, constantID, container);
			}

			if (!useActive && setElement)
			{
				string runtimeMenuName = AssignString (parameters, menuParameterID, menuName);
				string runtimeContainerElementName = AssignString (parameters, elementParameterID, containerElementName);
				
				runtimeMenuName = AdvGame.ConvertTokens (runtimeMenuName, Options.GetLanguage (), localVariables, parameters);
				runtimeContainerElementName = AdvGame.ConvertTokens (runtimeContainerElementName, Options.GetLanguage (), localVariables, parameters);
				
				MenuElement element = PlayerMenus.GetElementWithName (runtimeMenuName, runtimeContainerElementName);
				if (element != null)
				{
					runtimeInventoryBox = element as MenuInventoryBox;
				}
			}
		}

		
		public override float Run ()
		{
			if (runtimeContainer && runtimeContainer.enabled && runtimeContainer.gameObject.activeInHierarchy)
			{
				if (!useActive && setElement)
				{
					if (runtimeInventoryBox != null)
					{
						runtimeInventoryBox.OverrideContainer = runtimeContainer;
					}
					else
					{
						LogWarning ("Could not find InventoryBox to assign Container");
					}
					return 0f;
				}
				else
				{
					runtimeContainer.Interact ();
				}
			}

			return 0f;
		}
		

		#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			useActive = EditorGUILayout.Toggle ("Affect active container?", useActive);
			if (!useActive)
			{
				ComponentField ("Container:", ref container, ref constantID, parameters, ref parameterID);

				setElement = EditorGUILayout.Toggle ("Open in set element?", setElement);
				if (setElement)
				{
					TextField ("Menu name:", ref menuName, parameters, ref menuParameterID);
					TextField ("InventoryBox name:", ref containerElementName, parameters, ref elementParameterID);
				}
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (saveScriptsToo)
			{
				AddSaveScript <RememberContainer> (container);
			}
			constantID = AssignConstantID<Container> (container, constantID, parameterID);
		}

		
		public override string SetLabel ()
		{
			if (container != null)
			{
				return container.name;
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
		* <summary>Creates a new instance of the 'Containter: Open' Action</summary>
		* <param name = "containerToOpen">The Container to open</param>
		* <returns>The generated Action</returns>
		*/
		public static ActionContainerOpen CreateNew (Container containerToOpen)
		{
			ActionContainerOpen newAction = CreateNew<ActionContainerOpen> ();
			newAction.container = containerToOpen;
			if (newAction.container)
			{
				newAction.TryAssignConstantID (newAction.container, ref newAction.constantID);
			}
			return newAction;
		}
		
	}

}