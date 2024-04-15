/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"InvBin.cs"
 * 
 *	This script is a container class for inventory item categories.
 * 
 */
using UnityEngine;

namespace AC
{

	/** A data container for an inventory item category. */
	[System.Serializable]
	public class InvBin
	{

		#region Variables

		/** The category's editor name */
		public string label;
		/** A unique identifier */
		public int id;
		[SerializeField] private bool notForItems;
		/** If True, the category is avaiable for Objectives to use */
		public bool forObjectives;
		/** If True, the category is avaiable for Documents to use */
		public bool forDocuments;

		#endregion


		#region Constructors

		/**
		 * <summary>The default Constructor.</summary>
		 * <param name = "idArray">An array of already-used ID numbers, so that a unique one can be generated</param>
		 */
		public InvBin (int[] idArray)
		{
			id = 0;

			foreach (int _id in idArray)
			{
				if (id == _id)
				{
					id ++;
				}
			}

			label = "Category " + (id + 1).ToString ();
			forItems = true;
			forObjectives = false;
			forDocuments = false;
		}

		#endregion


		#if UNITY_EDITOR

		public string EditorLabel
		{
			get
			{
				return id.ToString () + ": " + (string.IsNullOrEmpty (label) ? "(Unnamed)" : label);
			}
		}

		#endif


		#region GetSet

		/** If True, the category is avaiable for Inventory items to use */
		public bool forItems
		{
			get
			{
				return !notForItems;
			}
			set
			{
				notForItems = !value;
			}
		}

		#endregion

	}

}