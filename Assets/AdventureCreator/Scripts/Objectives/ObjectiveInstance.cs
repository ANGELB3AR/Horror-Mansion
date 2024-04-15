/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ObjectiveInstance.cs"
 * 
 *	A runtime instance of an active Objective
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{

	/** A runtime instance of an active Objective */
	public class ObjectiveInstance
	{

		#region Variables

		protected Objective linkedObjective;
		protected int currentStateID;
		protected int previousStateID;
		protected long updateTime;
		protected bool isMarked;

		#endregion


		#region Constructors

		public ObjectiveInstance (int objectiveID)
		{
			if (KickStarter.inventoryManager)
			{
				linkedObjective = KickStarter.inventoryManager.GetObjective (objectiveID);
				currentStateID = 0;
				previousStateID = -1;
				updateTime = System.DateTime.Now.Ticks;
			}
		}


		public ObjectiveInstance (int objectiveID, int startingStateID)
		{
			if (KickStarter.inventoryManager)
			{
				linkedObjective = KickStarter.inventoryManager.GetObjective (objectiveID);;
				currentStateID = startingStateID;
				previousStateID = -1;
				updateTime = System.DateTime.Now.Ticks;
			}
		}


		public ObjectiveInstance (string saveData)
		{
			if (KickStarter.inventoryManager)
			{
				string[] chunkData = saveData.Split (SaveSystem.colon[0]);
				if (chunkData.Length > 1)
				{
					int objectiveID = -1;
					if (int.TryParse (chunkData[0], out objectiveID))
					{
						linkedObjective = KickStarter.inventoryManager.GetObjective (objectiveID);
					}

					int.TryParse (chunkData[1], out currentStateID);

					if (chunkData.Length > 2)
					{
						long.TryParse (chunkData[2], out updateTime);
					}

					if (chunkData.Length > 3)
					{
						bool.TryParse (chunkData[3], out isMarked);
					}

					if (chunkData.Length > 4)
					{
						int.TryParse (chunkData[4], out previousStateID);
					}
				}
			}
		}

		#endregion


		#region PublicFunctions

		/**
		 * <summary> Gets all Objective instances in the category marked as the 'sub-Objective category' for the Objective's current state</summary>
		 * <returns>All Objective instances that match the given criteria</returns>
		 */
		public ObjectiveInstance[] GetSubObjectives ()
		{
			int subCategoryID = CurrentState.LinkedCategoryID;
			if (subCategoryID >= 0)
			{
				List<int> subCategoryIDList = new List<int> ();
				subCategoryIDList.Add (subCategoryID);
				return KickStarter.runtimeObjectives.GetObjectives (subCategoryIDList);
			}

			return new ObjectiveInstance[0];
		}


		/**
		 * <summary> Gets all Objective instances in the category marked as the 'sub-objective category' for the Objective's current state</summary>
		 * <param name = "objectiveStateType">A filter for returned sub-Objectives based on their own current state<param>
		 * <returns>All Objective instances that match the given criteria</returns>
		 */
		public ObjectiveInstance[] GetSubObjectives (ObjectiveStateType objectiveStateType)
		{
			int subCategoryID = CurrentState.LinkedCategoryID;
			if (subCategoryID >= 0)
			{
				List<int> subCategoryIDList = new List<int> ();
				subCategoryIDList.Add (subCategoryID);
				return KickStarter.runtimeObjectives.GetObjectives (objectiveStateType, subCategoryIDList);
			}

			return new ObjectiveInstance[0];
		}

		#endregion


		#region GetSet

		/** The Objective this instance is linked to */
		public Objective Objective
		{
			get
			{
				return linkedObjective;
			}
		}


		/** The ID number of the instance's current objective state */
		public int CurrentStateID
		{
			get
			{
				return currentStateID;
			}
			set
			{
				if (CurrentState.stateType == ObjectiveStateType.Complete && linkedObjective.lockStateWhenComplete)
				{
					if (currentStateID != value)
					{
						ACDebug.Log ("Cannot update the state of completed Objective " + linkedObjective.Title + " as it is locked.");
					}
					return;
				}
				if (CurrentState.stateType == ObjectiveStateType.Fail && linkedObjective.lockStateWhenFail)
				{
					if (currentStateID != value)
					{
						ACDebug.Log ("Cannot update the state of failed Objective " + linkedObjective.Title + " as it is locked.");
					}
					return;
				}

				ObjectiveState newState = linkedObjective.GetState (value);
				if (newState != null)
				{
					int oldStateID = currentStateID;
					currentStateID = value;

					if (oldStateID != currentStateID)
					{
						previousStateID = oldStateID;
						updateTime = System.DateTime.Now.Ticks;

						if (newState.actionListOnEnter)
						{
							newState.actionListOnEnter.Interact ();
						}

						KickStarter.eventManager.Call_OnObjectiveUpdate (this);
					}
				}
				else
				{
					ACDebug.LogWarning ("Cannot set the state of objective " + linkedObjective.ID + " to " + value + " because it does not exist!");
				}
			}
		}


		/** The instance's current objective state */
		public ObjectiveState CurrentState
		{
			get
			{
				return linkedObjective.GetState (currentStateID);
			}
		}


		/** The ID of the instance's previous objective state, or -1 if it has only been one state */
		public int PreviousStateID
		{
			get
			{
				return previousStateID;
			}
		}


		/** A data string containing all saveable data */
		public string SaveData
		{
			get
			{
				return linkedObjective.ID.ToString ()
						+ SaveSystem.colon
						+ currentStateID.ToString ()
						+ SaveSystem.colon
						+ updateTime.ToString ()
						+ SaveSystem.colon
						+ isMarked
						+ SaveSystem.colon
						+ previousStateID;
			}
		}


		/** The ID of the Objective */
		public int ObjectiveID
		{
			get
			{
				if (linkedObjective == null)
				{
					return -1;
				}
				return linkedObjective.ID;
			}
		}


		/** The time the objective was last updated, represented by the number of Ticks in DateTime */
		public long UpdateTime
		{
			get
			{
				return updateTime;
			}
		}


		/** A flag that represents if the objecive is 'marked' or not.  This has no use internally, but can be set/read via custom script for use in extensions.  The set value is stored within save-game files */
		public bool IsMarked
		{
			get
			{
				return isMarked;
			}
			set
			{
				isMarked = value;
			}
		}

		#endregion

	}

}