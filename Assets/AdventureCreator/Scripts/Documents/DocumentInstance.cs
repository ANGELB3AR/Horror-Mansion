/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"DocumentInstance.cs"
 * 
 *	This script stores data related to a Document at runtime, allowing it to be modified wihout affecting the original asset.
 * 
 */

using System.Collections.Generic;
using UnityEngine;

namespace AC
{

	/** Stores data related to a Document at runtime */
	public class DocumentInstance
	{

		#region Variables

		/** The linked Document */
		public readonly Document Document;
		/** The index number of the page last shown */
		public int lastOpenPage = 1;
		/** If True, the Document has been read by the Player */
		public bool hasBeenViewed;
		private readonly Dictionary<int, PageTextureOverride> textureOverrideDict;

		#endregion


		#region Constructors

		/** A Constructore that assigns the Document directly */
		public DocumentInstance (Document document)
		{
			Document = document;
			lastOpenPage = 1;
			hasBeenViewed = false;
			textureOverrideDict = new Dictionary<int, PageTextureOverride> ();

			if (Document == null) ACDebug.LogWarning ("Invalid Document");
			else if (!KickStarter.inventoryManager.IsInDocumentsCategory (Document.binID)) Document.binID = KickStarter.inventoryManager.GetFirstDocumentsCategoryID ();
		}


		/** A Constructore that assigns the Document based on its ID in the Inventory Manager */
		public DocumentInstance (int id)
		{
			Document = KickStarter.inventoryManager ? KickStarter.inventoryManager.GetDocument (id) : null;
			textureOverrideDict = new Dictionary<int, PageTextureOverride> ();
			lastOpenPage = 1;
			hasBeenViewed = false;
			 
			if (Document == null) ACDebug.LogWarning ("Invalid Document with ID " + id	);
			else if (!KickStarter.inventoryManager.IsInDocumentsCategory (Document.binID)) Document.binID = KickStarter.inventoryManager.GetFirstDocumentsCategoryID ();
		}

		#endregion


		#region PublicFunctions

		/** 
		 * <summary>Gets the texture associated with a given page.  This can be overridden with SetPageTexture.</summary> 
		 * <param name = "pageNumber">The number of the page, starting from 1</param>
		 * <returns>The page's texture</returns>
		 */
		public Texture2D GetPageTexture (int pageNumber)
		{
			PageTextureOverride pageTextureOverride = null;
			if (textureOverrideDict.TryGetValue (pageNumber, out pageTextureOverride))
			{
				return pageTextureOverride.texture;
			}

			return Document.pages[pageNumber].texture;
		}


		/** 
		 * <summary>Sets the texture associated with a given page.  This will override the default.</summary> 
		 * <param name = "pageNumber">The number of the page, starting from 1</param>
		 * <param name = "texture">The page's new texture.  If null, it will revert to its default texture</param>
		 */
		public void SetPageTexture (int pageNumber, Texture2D texture)
		{
			PageTextureOverride pageTextureOverride = null;
			if (!textureOverrideDict.TryGetValue (pageNumber, out pageTextureOverride))
			{
				pageTextureOverride = new PageTextureOverride (pageNumber);
				textureOverrideDict.Add (pageNumber, pageTextureOverride);
			}

			if (texture != null)
			{
				pageTextureOverride.texture = texture;
			}
			else
			{
				textureOverrideDict.Remove (pageNumber);
			}
		}

		#endregion


		#region StaticFunctions

		public static bool IsValid (DocumentInstance documentInstance)
		{
			if (documentInstance != null && documentInstance.Document != null)
			{
				return true;
			}
			return false;
		}

		#endregion


		#region GetSet

		/** The linked Document's ID, or -1 if invalid */
		public int DocumentID { get { return Document != null ? Document.ID : -1; } }
		/** The linked Document's category ID, or -1 if invalid */
		public int CategoryID { get { return Document != null ? Document.binID : -1; } }

		#endregion


		#region PrivateFunctions

		private class PageTextureOverride
		{

			public readonly int Number;
			public Texture2D texture;

			public PageTextureOverride (int number)
			{
				Number = number;
			}

		}

		#endregion

	}

}