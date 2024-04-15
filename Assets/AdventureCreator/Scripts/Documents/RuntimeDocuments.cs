/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"RuntimeDocuments.cs"
 * 
 *	This script stores information about the currently-open Document, as well as any runtime-made changes to all Documents.
 * 
 */

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AC
{

	/** This script stores information about the currently-open Document, as well as any runtime-made changes to all Documents. */
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_runtime_documents.html")]
	public class RuntimeDocuments : MonoBehaviour
	{

		#region Variables

		protected DocumentInstance activeDocumentInstance;
		protected readonly List<DocumentInstance> collectedDocuments = new List<DocumentInstance> (); 
		
		#endregion


		#region UnityStandards

		private void OnEnable ()
		{
			EventManager.OnInitialiseScene += OnInitialiseScene;
		}


		private void OnDisable ()
		{
			EventManager.OnInitialiseScene -= OnInitialiseScene;
		}

		#endregion


		#region PublicFunctions

		/** This is called when the game begins, and sets up the initial state. */
		public void OnInitPersistentEngine ()
		{
			activeDocumentInstance = null;
			collectedDocuments.Clear ();

			GetDocumentsOnStart ();
		}


		/**
		 * <summary>Opens a Document.  To view it, a Menu with an Appear Type of OnViewDocument must be present in the Menu Manager.<summary>
		 * <param name = "document">The Document to open</param>
		 */
		public void OpenDocument (Document document)
		{
			if (document != null && (!DocumentInstance.IsValid (activeDocumentInstance) || activeDocumentInstance.Document != document))
			{
				CloseDocument ();

				DocumentInstance collectedDocument = GetCollectedDocumentInstance (document);
				if (collectedDocument != null)
				{
					activeDocumentInstance = collectedDocument;
				}
				else
				{
					activeDocumentInstance = new DocumentInstance (document);
				}
				activeDocumentInstance.hasBeenViewed = true;
				KickStarter.eventManager.Call_OnHandleDocument (activeDocumentInstance, true);
			}
		}


		/**
		 * <summary>Opens a Document.  To view it, a Menu with an Appear Type of OnViewDocument must be present in the Menu Manager.<summary>
		 * <param name = "documentID">The ID number of the Document to open</param>
		 */
		public void OpenDocument (int documentID)
		{
			if (documentID >= 0)
			{
				Document document = KickStarter.inventoryManager.GetDocument (documentID);
				OpenDocument (document);
			}
		}


		/** Closes the currently-viewed Document, if there is one */
		public void CloseDocument ()
		{
			if (activeDocumentInstance != null)
			{
				KickStarter.eventManager.Call_OnHandleDocument (activeDocumentInstance, false);
				activeDocumentInstance = null;
			}
		}


		/**
		 * <summary>Checks if a particular Document is in the Player's collection</summary>
		 * <param name = "documentID">The ID number of the Document to check for</param>
		 * <returns>True if the Document is in the Player's collection</returns>
		 */
		public bool DocumentIsInCollection (int documentID)
		{
			return GetCollectedDocumentInstance (documentID) != null;
		}


		/**
		 * <summary>Checks if a given Document has been read by the Player</summary>
		 * <param name = "document">The Document to check for</param>
		 * <returns>True if the Document is held by the Player and has been read</returns>
		 */
		public bool HasBeenRead (Document document)
		{
			if (document == null) return false;

			DocumentInstance documentInstance = GetCollectedDocumentInstance (document);
			if (documentInstance != null)
			{
				return documentInstance.hasBeenViewed;
			}
			return false;
		}


		/**
		 * <summary>Checks if a given Document has been read by the Player</summary>
		 * <param name = "documentID">The ID of theDocument to check for</param>
		 * <returns>True if the Document is held by the Player and has been read</returns>
		 */
		public bool HasBeenRead (int documentID)
		{
			if (documentID >= 0)
			{
				Document document = KickStarter.inventoryManager.GetDocument (documentID);
				return HasBeenRead (document);
			}
			return false;
		}


		/**
		 * <summary>Gets the DocumentInstance class for a Document present in the Player's collection</summary>
		 * <param name = "document">The original Document, as defined in the Inventory Manager</param>
		 * <returns>The DocumentInstance class, if present, or null otherwise</returns>
		 */
		public DocumentInstance GetCollectedDocumentInstance (Document document)
		{
			if (document != null)
			{
				return GetCollectedDocumentInstance (document.ID);
			}
			return null;
		}

		
		/**
		 * <summary>Gets the DocumentInstance class for a Document present in the Player's collection</summary>
		 * <param name = "documentID">The ID of the Document</param>
		 * <returns>The DocumentInstance class, if present, or null otherwise</returns>
		 */
		public DocumentInstance GetCollectedDocumentInstance (int documentID)
		{
			foreach (DocumentInstance collectedDocument in collectedDocuments)
			{
				if (collectedDocument.DocumentID == documentID)
				{
					return collectedDocument;
				}
			}
			return null;
		}


		/**
		 * <summary>Adds a Document to the Player's own collection</summary>
		 * <param name = "document">The Document to add</param>
		 * <param name = "addToFront">If True, the Document will be added to the front of the collection</param>
		 */
		public void AddToCollection (Document document, bool addToFront = false)
		{
			if (document == null || DocumentIsInCollection (document.ID)) return;

			DocumentInstance documentInstance = new DocumentInstance (document);
			if (addToFront)
			{
				collectedDocuments.Insert (0, documentInstance);
			}
			else
			{
				collectedDocuments.Add (documentInstance);
			}
			PlayerMenus.ResetInventoryBoxes ();

			KickStarter.eventManager.Call_OnAddRemoveDocument (documentInstance, true);
		}


		/**
		 * <summary>Adds a Document to the Player's own collection</summary>
		 * <param name = "documentID">The ID of the Document to add</param>
		 */
		public void AddToCollection (int documentID)
		{
			Document document = KickStarter.inventoryManager.GetDocument (documentID);
			AddToCollection (document);
		}


		/**
		 * <summary>Removes a Document from the Player's own collection</summary>
		 * <param name = "document">The Document to remove</param>
		 */
		public void RemoveFromCollection (Document document)
		{
			if (document == null) return;

			DocumentInstance documentInstance = GetCollectedDocumentInstance (document);
			if (documentInstance == null) return;

			collectedDocuments.Remove (documentInstance);
			PlayerMenus.ResetInventoryBoxes ();

			KickStarter.eventManager.Call_OnAddRemoveDocument (documentInstance, true);
		}


		/**
		 * <summary>Removes a Document from the Player's own collection</summary>
		 * <param name = "documentID">The ID of the Document to remove</param>
		 */
		public void RemoveFromCollection (int documentID)
		{
			Document document = KickStarter.inventoryManager.GetDocument (documentID);
			RemoveFromCollection (document);
		}


		/** Removes all Documents from the Player's own collection */
		public void ClearCollection ()
		{
			collectedDocuments.Clear ();
			PlayerMenus.ResetInventoryBoxes ();
		}


		/**
		 * <summary>Gets the page number to return to when opening a previously-read Document</summary>
		 * <param name = "document">The Document in question</param>
		 * <returns>The page number to return to when opening a previously-read Document</returns>
		 */
		public int GetLastOpenPage (DocumentInstance documentInstance)
		{
			if (DocumentInstance.IsValid (documentInstance) && documentInstance.Document.rememberLastOpenPage)
			{
				return documentInstance.lastOpenPage;
			}
			return 1;
		}


		/**
		 * <summary>Sets the page number to return to when a given Document is next opened</summary>
		 * <param name = "document">The Document in question</param>
		 * <param name = "page">The page number to return to next time</param>
		 */
		public void SetLastOpenPage (DocumentInstance documentInstance, int page)
		{
			if (DocumentInstance.IsValid (documentInstance) && documentInstance.Document.rememberLastOpenPage)
			{
				documentInstance.lastOpenPage = page;
			}
		}


		/**
		 * <summary>Updates a PlayerData class with its own variables that need saving.</summary>
		 * <param name = "playerData">The original PlayerData class</param>
		 * <returns>The updated PlayerData class</returns>
		 */
		public PlayerData SavePlayerDocuments (PlayerData playerData)
		{
			playerData.activeDocumentID = DocumentInstance.IsValid (activeDocumentInstance) ? activeDocumentInstance.DocumentID : -1;

			System.Text.StringBuilder collectedDocumentsData = new System.Text.StringBuilder ();
			foreach (DocumentInstance documentInstance in collectedDocuments)
			{
				collectedDocumentsData.Append (documentInstance.DocumentID.ToString ());
				collectedDocumentsData.Append (SaveSystem.colon);
				collectedDocumentsData.Append (documentInstance.lastOpenPage);
				collectedDocumentsData.Append (SaveSystem.colon);
				collectedDocumentsData.Append (documentInstance.hasBeenViewed ? 1 : 0);
				collectedDocumentsData.Append (SaveSystem.pipe);
			}
			if (collectedDocuments.Count > 0)
			{
				collectedDocumentsData.Remove (collectedDocumentsData.Length-1, 1);
			}
			playerData.collectedDocumentData = collectedDocumentsData.ToString ();
			playerData.lastOpenDocumentPagesData = string.Empty;
			
			return playerData;
		}


		/**
		 * <summary>Restores saved data from a PlayerData class</summary>
		 * <param name = "playerData">The PlayerData class to load from</param>
		 */
		public void AssignPlayerDocuments (PlayerData playerData)
		{
			collectedDocuments.Clear ();
			if (!string.IsNullOrEmpty (playerData.collectedDocumentData))
			{
				string[] collectedDocumentArray = playerData.collectedDocumentData.Split (SaveSystem.pipe[0]);
				
				foreach (string chunkBlock in collectedDocumentArray)
				{
					string[] chunkArray = chunkBlock.Split (SaveSystem.colon[0]);
					if (chunkArray == null) continue;

					if (chunkArray.Length >= 1)
					{
						int _id = -1;
						if (int.TryParse (chunkArray[0], out _id))
						{
							if (_id >= 0)
							{
								Document document = KickStarter.inventoryManager.GetDocument (_id);
								if (document != null)
								{
									DocumentInstance documentInstance = new DocumentInstance (document);

									if (chunkArray.Length >= 2)
									{
										if (document.rememberLastOpenPage)
										{
											int _lastOpenPage = 1;
											if (int.TryParse (chunkArray[1], out _lastOpenPage))
											{
												documentInstance.lastOpenPage = _lastOpenPage;
											}
										}

										if (chunkArray.Length >= 3)
										{
											int _hasBeenViewed = 0;
											if (int.TryParse (chunkArray[2], out _hasBeenViewed))
											{
												documentInstance.hasBeenViewed = _hasBeenViewed == 1;
											}
										}
									}

									collectedDocuments.Add (documentInstance);
								}
							}
						}
					}
				}
			}

			OpenDocument (playerData.activeDocumentID);
		}


		/**
		 * <summary>Gets an array of ID numbers that each represent a Document held by the Player</summary>
		 * <param name = "limitToCategoryIDs">If non-negative, ID numbers of inventory categories to limit results to</param>
		 * <returns>An array of ID numbers that each represent a Document held by the Player</returns>
		 */
		public int[] GetCollectedDocumentIDs (int[] limitToCategoryIDs = null)
		{
			List<int> limitedDocuments = new List<int> ();
			foreach (DocumentInstance documentInstance in collectedDocuments)
			{
				bool canAdd = false;
				if (limitToCategoryIDs != null)
				{
					foreach (int limitToCategoryID in limitToCategoryIDs)
					{
						if (documentInstance.Document.binID == limitToCategoryID)
						{
							canAdd = true;
						}
					}
				}
				else
				{
					canAdd = true;
				}

				if (canAdd)
				{
					limitedDocuments.Add (documentInstance.DocumentID);
				}
			}
			return limitedDocuments.ToArray ();
		}

		#endregion


		#region ProtectedFunctions

		protected void GetDocumentsOnStart ()
		{
			if (KickStarter.inventoryManager)
			{
				foreach (Document document in KickStarter.inventoryManager.documents)
				{
					if (document.carryOnStart)
					{
						collectedDocuments.Add (new DocumentInstance (document));
					}
				}
			}
			else
			{
				ACDebug.LogError ("No Inventory Manager found - please use the Adventure Creator window to create one.");
			}
		}

		#endregion


		#region CustomEvents

		private void OnInitialiseScene ()
		{
			activeDocumentInstance = null;
		}

		#endregion


		#region GetSet

		/** The currently-active Document Instance */
		public DocumentInstance ActiveDocumentInstance { get { return activeDocumentInstance; } }

		/** The currently-active Document */
		public Document ActiveDocument { get { return DocumentInstance.IsValid (activeDocumentInstance) ? activeDocumentInstance.Document : null; } }

		#endregion

	}

}