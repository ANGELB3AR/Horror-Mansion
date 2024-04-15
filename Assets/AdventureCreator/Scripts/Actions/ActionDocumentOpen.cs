/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionDocumentOpen.cs"
 * 
 *	This action makes a Document active for display in a Menu.
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
	public class ActionDocumentOpen : Action, IDocumentReferencerAction
	{

		public int documentID;
		public int parameterID = -1;
		public bool addToCollection = false;

		public bool setElement;
		public string menuName;
		public string documentElementName;

		public int menuParameterID = -1;
		public int elementParameterID = -1;

		protected Document runtimeDocument;
		protected LocalVariables localVariables;
		protected MenuJournal runtimeJournal;
		
		
		public override ActionCategory Category { get { return ActionCategory.Document; }}
		public override string Title { get { return "Open"; }}
		public override string Description { get { return "Opens a document, causing any Menu of 'Appear type: On View Document' to open."; }}


		public override void AssignParentList (ActionList actionList)
		{
			if (localVariables == null)
			{
				localVariables = KickStarter.localVariables;
			}

			base.AssignParentList (actionList);
		}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			int runtimeDocumentID = AssignDocumentID (parameters, parameterID, documentID);
			runtimeDocument = KickStarter.inventoryManager.GetDocument (runtimeDocumentID);

			if (setElement)
			{
				string runtimeMenuName = AssignString (parameters, menuParameterID, menuName);
				string runtimeDocumentElementName = AssignString (parameters, elementParameterID, documentElementName);
				
				runtimeMenuName = AdvGame.ConvertTokens (runtimeMenuName, Options.GetLanguage (), localVariables, parameters);
				runtimeDocumentElementName = AdvGame.ConvertTokens (runtimeDocumentElementName, Options.GetLanguage (), localVariables, parameters);
				
				MenuElement element = PlayerMenus.GetElementWithName (runtimeMenuName, runtimeDocumentElementName);
				if (element != null)
				{
					runtimeJournal = element as MenuJournal;
				}
			}
		}


		public override float Run ()
		{
			if (runtimeDocument == null)
			{
				return 0f;
			}

			if (!isRunning)
			{
				if (addToCollection)
				{
					KickStarter.runtimeDocuments.AddToCollection (runtimeDocument);
				}

				if (setElement)
				{
					if (runtimeJournal != null)
					{
						DocumentInstance documentInstance = KickStarter.runtimeDocuments.GetCollectedDocumentInstance (runtimeDocument);
						if (!DocumentInstance.IsValid (documentInstance))
						{
							documentInstance = new DocumentInstance (runtimeDocument);
						}

						runtimeJournal.OverrideDocument = documentInstance;
					}
					else
					{
						LogWarning ("Could not find Journal to assign Document");
					}
					return 0f;
				}
				KickStarter.runtimeDocuments.OpenDocument (runtimeDocument);

				if (willWait)
				{
					isRunning = true;
					return defaultPauseTime;
				}
			}
			else
			{
				if (KickStarter.runtimeDocuments.ActiveDocument == runtimeDocument)
				{
					return defaultPauseTime;
				}
			}

			isRunning = false;
			return 0f;
		}


		public override void Skip ()
		{
			if (runtimeDocument == null)
			{
				return;
			}

			if (addToCollection)
			{
				KickStarter.runtimeDocuments.AddToCollection (runtimeDocument);
			}
			if (willWait)
			{
				if (KickStarter.runtimeDocuments.ActiveDocument == runtimeDocument)
				{
					KickStarter.runtimeDocuments.CloseDocument ();
				}
			}
			else
			{
				KickStarter.runtimeDocuments.OpenDocument (runtimeDocument);
			}
		}
		

		#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			DocumentField ("Document:", ref documentID, parameters, ref parameterID, "Document ID:");
			addToCollection = EditorGUILayout.Toggle ("Add to collection?", addToCollection);

			setElement = EditorGUILayout.Toggle ("Open in set element?", setElement);
			if (setElement)
			{
				TextField ("Menu name:", ref menuName, parameters, ref menuParameterID);
				TextField ("Journal name:", ref documentElementName, parameters, ref elementParameterID);
			}
			else
			{
				willWait = EditorGUILayout.Toggle ("Wait until close?", willWait);
			}
		}


		public override string SetLabel ()
		{
			Document document = KickStarter.inventoryManager.GetDocument (documentID);
			if (document != null)
			{
				return document.Title;
			}
			return string.Empty;
		}


		public int GetNumDocumentReferences (int _docID, List<ActionParameter> parameters)
		{
			if (parameterID < 0 && documentID == _docID)
			{
				return 1;
			}
			return 0;
		}


		public int UpdateDocumentReferences (int oldDocumentID, int newDocumentID, List<ActionParameter> actionParameters)
		{
			if (parameterID < 0 && documentID == oldDocumentID)
			{
				documentID = newDocumentID;
				return 1;
			}
			return 0;
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Document: Open' Action</summary>
		 * <param name = "documentID">The ID number of the document to open</param>
		 * <param name = "addToCollection">If True, the document will be added to the player's collection if not there already</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionDocumentOpen CreateNew (int documentID, bool addToCollection)
		{
			ActionDocumentOpen newAction = CreateNew<ActionDocumentOpen> ();
			newAction.documentID = documentID;
			newAction.addToCollection = addToCollection;
			return newAction;
		}
		
	}

}