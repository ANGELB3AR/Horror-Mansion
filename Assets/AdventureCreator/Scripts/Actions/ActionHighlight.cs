﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionHighlight.cs"
 * 
 *	This action manually highlights objects and Inventory items
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
	public class ActionHighlight : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public enum WhatToHighlight { SceneObject, InventoryItem };
		public WhatToHighlight whatToHighlight = WhatToHighlight.SceneObject;
		public HighlightType highlightType = HighlightType.Enable;
		public bool isInstant = false;

		public Highlight highlightObject;
		protected Highlight runtimeHighlightObject;

		public int invID;
		
		protected InventoryManager inventoryManager;


		public override ActionCategory Category { get { return ActionCategory.Object; }}
		public override string Title { get { return "Highlight"; }}
		public override string Description { get { return "Gives a glow effect to any mesh object with the Highlight script component attached to it. Can also be used to make Inventory items glow, making it useful for tutorial sections."; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			if (whatToHighlight == WhatToHighlight.SceneObject)
			{
				runtimeHighlightObject = AssignFile <Highlight> (parameters, parameterID, constantID, highlightObject);
			}
			else
			{
				invID = AssignInvItemID (parameters, parameterID, invID);
			}
		}
		
		
		public override float Run ()
		{
			if (whatToHighlight == WhatToHighlight.SceneObject && runtimeHighlightObject == null)
			{
				return 0f;
			}

			if (whatToHighlight == WhatToHighlight.SceneObject)
			{
				switch (highlightType)
				{
					case HighlightType.Enable:
						if (isInstant)
						{
							runtimeHighlightObject.HighlightOnInstant();
						}
						else
						{
							runtimeHighlightObject.HighlightOn();
						}
						break;

					case HighlightType.Disable:
						if (isInstant)
						{
							runtimeHighlightObject.HighlightOffInstant();
						}
						else
						{
							runtimeHighlightObject.HighlightOff();
						}
						break;

					case HighlightType.PulseOnce:
						runtimeHighlightObject.Flash();
						break;

					case HighlightType.PulseContinually:
						runtimeHighlightObject.Pulse();
						break;

					default:
						break;
				}
			}

			else
			{
				if (KickStarter.runtimeInventory)
				{
					if (highlightType == HighlightType.Enable && isInstant)
					{
						KickStarter.runtimeInventory.HighlightItemOnInstant (invID);
						return 0f;
					}
					else if (highlightType == HighlightType.Disable && isInstant)
					{
						KickStarter.runtimeInventory.HighlightItemOffInstant ();
						return 0f;
					}
					KickStarter.runtimeInventory.HighlightItem (invID, highlightType);
				}
			}

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			whatToHighlight = (WhatToHighlight) EditorGUILayout.EnumPopup ("What to highlight:", whatToHighlight);

			if (whatToHighlight == WhatToHighlight.SceneObject)
			{
				ComponentField ("Object to highlight:", ref highlightObject, ref constantID, parameters, ref parameterID);
			}
			else if (whatToHighlight == WhatToHighlight.InventoryItem)
			{
				ItemField ("Inventory item:", ref invID, parameters, ref parameterID, "Inventory item ID:");
			}

			highlightType = (HighlightType) EditorGUILayout.EnumPopup ("Highlight type:", highlightType);
			if (highlightType == HighlightType.Enable || highlightType == HighlightType.Disable)
			{
				isInstant = EditorGUILayout.Toggle ("Is instant?", isInstant);
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (whatToHighlight == WhatToHighlight.SceneObject)
			{
				constantID = AssignConstantID<Highlight> (highlightObject, constantID, parameterID);
			}
		}
		
		
		public override string SetLabel ()
		{
			if (highlightObject != null)
			{
				if (whatToHighlight == WhatToHighlight.SceneObject)
				{
					return highlightType.ToString () + " " + highlightObject.gameObject.name;
				}
				return highlightType.ToString () + " Inventory item";
			}

			return string.Empty;
		}


		public override bool ReferencesObjectOrID (GameObject _gameObject, int id)
		{
			if (parameterID < 0 && whatToHighlight == WhatToHighlight.SceneObject)
			{
				if (highlightObject && highlightObject.gameObject == _gameObject) return true;
				if (constantID == id) return true;
			}
			return base.ReferencesObjectOrID (_gameObject, id);
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Object: Highlight' Action, set to highlight an object in the scene</summary>
		 * <param name = "objectToAffect">The Highlight component to affect</param>
		 * <param name = "highlightType">What type of highlighting effect to perform</param>
		 * <param name = "isInstant">If True, then the effect will be performed instantly</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionHighlight CreateNew_SceneObject (Highlight objectToAffect, HighlightType highlightType, bool isInstant = false)
		{
			ActionHighlight newAction = CreateNew<ActionHighlight> ();
			newAction.whatToHighlight = WhatToHighlight.SceneObject;
			newAction.highlightObject = objectToAffect;
			newAction.TryAssignConstantID (newAction.highlightObject, ref newAction.constantID);
			newAction.highlightType = highlightType;
			newAction.isInstant = isInstant;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Object: Highlight' Action, set to highlight an inventory item</summary>
		 * <param name = "itemIDToAffect">The ID number of the inventory item held by the player</param>
		 * <param name = "highlightType">What type of highlighting effect to perform</param>
		 * <param name = "isInstant">If True, then the effect will be performed instantly</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionHighlight CreateNew_InventoryItem (int itemIDToAffect, HighlightType highlightType, bool isInstant = false)
		{
			ActionHighlight newAction = CreateNew<ActionHighlight> ();
			newAction.whatToHighlight = WhatToHighlight.InventoryItem;
			newAction.invID = itemIDToAffect;
			newAction.highlightType = highlightType;
			newAction.isInstant = isInstant;
			return newAction;
		}
		
	}

}