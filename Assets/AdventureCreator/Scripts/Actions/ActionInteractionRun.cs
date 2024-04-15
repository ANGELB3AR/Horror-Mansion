/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionInteraction.cs"
 * 
 *	This Action can enable and disable
 *	a Hotspot's individual Interactions.
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
	public class ActionInteractionRun : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public Hotspot hotspot;
		protected Hotspot runtimeHotspot;

		public InteractionType interactionType;
		public int index = 0;
		public int indexParameterID = -1;

		public bool ignorePlayerAction;
		public bool requireItemHeld;


		public override ActionCategory Category { get { return ActionCategory.Hotspot; } }
		public override string Title { get { return "Run interaction"; } }
		public override string Description { get { return "Runs individual Interactions on a Hotspot."; } }


		public override void AssignValues (List<ActionParameter> parameters)
		{
			runtimeHotspot = AssignFile<Hotspot> (parameters, parameterID, constantID, hotspot);
			index = AssignInteger (parameters, indexParameterID, index);

			if (interactionType == InteractionType.Inventory && parameterID >= 0 && indexParameterID >= 0 && runtimeHotspot)
			{
				// Special case: parameter was actually inventory item, not integer
				int itemID = AssignInvItemID (parameters, indexParameterID, index);
				for (int i = 0; i < runtimeHotspot.invButtons.Count; i++)
				{
					if (runtimeHotspot.invButtons[i].invID == itemID)
					{
					//	index = i;
					}
				}
			}

		}


		public override float Run ()
		{
			if (runtimeHotspot == null)
			{
				return 0f;
			}

			if (interactionType == InteractionType.Use)
			{
				if (index >= 0 && index < runtimeHotspot.useButtons.Count)
				{
					RunButton (runtimeHotspot.useButtons[index]);
				}
				else
				{
					LogWarning ("Cannot run Hotspot " + runtimeHotspot.gameObject.name + "'s Use interaction " + index.ToString () + " because it doesn't exist!");
				}
			}
			else if (interactionType == InteractionType.Examine)
			{
				RunButton (runtimeHotspot.lookButton);
			}
			else if (interactionType == InteractionType.Inventory)
			{
				if (index >= 0 && index < runtimeHotspot.invButtons.Count)
				{
					if (requireItemHeld)
					{
						int invID = runtimeHotspot.invButtons[index].invID;
						if (KickStarter.runtimeInventory.PlayerInvCollection.GetCount (invID) == 0 &&
							(!InvInstance.IsValid (KickStarter.runtimeInventory.SelectedInstance) || KickStarter.runtimeInventory.SelectedInstance.ItemID != invID))
						{
							return 0f;
						}
					}

					RunButton (runtimeHotspot.invButtons[index]);
				}
				else
				{
					LogWarning ("Cannot run Hotspot " + runtimeHotspot.gameObject.name + "'s Inventory interaction " + index.ToString () + " because it doesn't exist!");
				}
			}

			return 0f;
		}


		private void RunButton (AC.Button button)
		{
			if (button == null)
			{
				return;
			}

			PlayerAction backupPlayerAction = button.playerAction;
			if (ignorePlayerAction)
			{
				button.playerAction = PlayerAction.DoNothing;
			}

			runtimeHotspot.RunInteraction (button);

			if (ignorePlayerAction)
			{
				button.playerAction = backupPlayerAction;
			}
		}


#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			ComponentField ("Hotspot:", ref hotspot, ref constantID, parameters, ref parameterID);

			interactionType = (InteractionType) EditorGUILayout.EnumPopup ("Interaction to run:", interactionType);

			switch (interactionType)
			{
				case InteractionType.Use:
					if (hotspot == null || parameterID >= 0)
					{
						IntField ("Use interaction:", ref index, parameters, ref indexParameterID);
					}
					else if (KickStarter.cursorManager)
					{
						// Multiple use interactions
						if (hotspot.useButtons.Count > 0 && hotspot.provideUseInteraction)
						{
							List<string> labelList = new List<string> ();

							foreach (AC.Button button in hotspot.useButtons)
							{
								labelList.Add (hotspot.useButtons.IndexOf (button) + ": " + KickStarter.cursorManager.GetLabelFromID (button.iconID, 0));
							}

							index = EditorGUILayout.Popup ("Use interaction:", index, labelList.ToArray ());
						}
						else
						{
							EditorGUILayout.HelpBox ("No 'Use' interactions defined!", MessageType.Info);
						}
					}
					else
					{
						EditorGUILayout.HelpBox ("A Cursor Manager is required.", MessageType.Warning);
					}
					break;

				case InteractionType.Examine:
					if (hotspot != null && parameterID < 0 && !hotspot.provideLookInteraction)
					{
						EditorGUILayout.HelpBox ("No 'Examine' interaction defined!", MessageType.Info);
					}
					break;

				case InteractionType.Inventory:
					if (hotspot == null || parameterID >= 0)
					{
						ItemField ("Inventory interaction:", ref index, parameters, ref indexParameterID);
					}
					else if (KickStarter.inventoryManager)
					{
						if (hotspot.invButtons.Count > 0 && hotspot.provideInvInteraction)
						{
							List<string> labelList = new List<string> ();

							foreach (AC.Button button in hotspot.invButtons)
							{
								labelList.Add (hotspot.invButtons.IndexOf (button) + ": " + KickStarter.inventoryManager.GetLabel (button.invID));
							}

							index = EditorGUILayout.Popup ("Inventory interaction:", index, labelList.ToArray ());
						}
						else
						{
							EditorGUILayout.HelpBox ("No 'Inventory' interactions defined!", MessageType.Info);
						}
					}
					else
					{
						EditorGUILayout.HelpBox ("An Inventory Manager is required.", MessageType.Warning);
					}

					requireItemHeld = EditorGUILayout.Toggle ("Require Inventory item?", requireItemHeld);
					break;

				default:
					break;
			}

			ignorePlayerAction = EditorGUILayout.Toggle ("Ignore 'Player action'?", ignorePlayerAction);
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (saveScriptsToo)
			{
				AddSaveScript<RememberHotspot> (hotspot);
			}

			constantID = AssignConstantID<Hotspot> (hotspot, constantID, parameterID);
		}


		public override string SetLabel ()
		{
			if (hotspot != null)
			{
				return hotspot.name + " - " + interactionType;
			}
			return string.Empty;
		}


		public override bool ReferencesObjectOrID (GameObject _gameObject, int id)
		{
			if (parameterID < 0)
			{
				if (hotspot && hotspot.gameObject == _gameObject) return true;
				if (constantID == id) return true;
			}
			return base.ReferencesObjectOrID (_gameObject, id);
		}

#endif


		/**
		 * <summary>Creates a new instance of the 'Hotspot: Run interaction' Action</summary>
		 * <param name = "hotspot">The Hotspot to run</param>
		 * <param name = "interactionType">The type of Hotspot interaction to run</param>
		 * <param name = "index">The index of the interaction to run</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionInteractionRun CreateNew (Hotspot hotspot, InteractionType interactionType, int index)
		{
			ActionInteractionRun newAction = CreateNew<ActionInteractionRun> ();
			newAction.hotspot = hotspot;
			newAction.TryAssignConstantID (newAction.hotspot, ref newAction.constantID);
			newAction.interactionType = interactionType;
			newAction.index = index;

			return newAction;
		}

	}

}