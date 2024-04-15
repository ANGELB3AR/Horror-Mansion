/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionDocumentCollection.cs"
 * 
 *	This action adds or removes a Document active from the player's collection.
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
	public class ActionDocumentCollection : Action, IDocumentReferencerAction
	{

		public int documentID;
		public int parameterID = -1;
		public bool addToFront;

		[SerializeField] protected DocumentCollectionMethod documentCollectionMethod = DocumentCollectionMethod.Add;
		public enum DocumentCollectionMethod { Add, Remove, Clear };

		
		public override ActionCategory Category { get { return ActionCategory.Document; }}
		public override string Title { get { return "Add or remove"; }}
		public override string Description { get { return "Adds or removes a document from the player's collection, or removes all of them."; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			documentID = AssignDocumentID (parameters, parameterID, documentID);
		}


		public override float Run ()
		{
			Document document = KickStarter.inventoryManager.GetDocument (documentID);

			if (document != null)
			{
				switch (documentCollectionMethod)
				{
					case DocumentCollectionMethod.Add:
						KickStarter.runtimeDocuments.AddToCollection (document, addToFront);
						break;

					case DocumentCollectionMethod.Remove:
						KickStarter.runtimeDocuments.RemoveFromCollection (document);
						break;

					case DocumentCollectionMethod.Clear:
						KickStarter.runtimeDocuments.ClearCollection ();
						break;

					default:
						break;
				}
			}

			return 0f;
		}
		

		#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			documentCollectionMethod = (DocumentCollectionMethod) EditorGUILayout.EnumPopup ("Method:", documentCollectionMethod);

			DocumentField ("Document:", ref documentID, parameters, ref parameterID, "Document ID:");

			if (documentCollectionMethod == DocumentCollectionMethod.Add)
			{
				addToFront = EditorGUILayout.Toggle ("Add to front?", addToFront);
			}
		}


		public override string SetLabel ()
		{
			return documentCollectionMethod.ToString ();
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
		 * <summary>Creates a new instance of the 'Document: Add or remove' Action</summary>
		 * <param name = "documentID">The ID number of the document to open</param>
		 * <param name = "method">The method to perform</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionDocumentCollection CreateNew (int documentID, DocumentCollectionMethod method)
		{
			ActionDocumentCollection newAction = CreateNew<ActionDocumentCollection> ();
			newAction.documentCollectionMethod = method;
			newAction.documentID = documentID;
			return newAction;
		}
		
	}

}