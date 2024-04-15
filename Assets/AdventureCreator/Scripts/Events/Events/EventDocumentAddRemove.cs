using UnityEngine;

namespace AC
{

	public class EventDocumentAddRemove : EventBase
	{

		[SerializeField] private AddRemove addRemove;
		public enum AddRemove { Add, Remove };
		[SerializeField] private int documentID = -1;


		public override string[] EditorNames { get { return new string[] { "Document/Add", "Document/Remove" }; } }
		protected override string EventName { get { return addRemove == AddRemove.Add ? "OnDocumentAdd" : "OnDocumentRemove"; } }
		protected override string ConditionHelp { get { return "Whenever " + ((documentID >= 0) ? GetDocumentName () : "a Document") + " is " + ((addRemove == AddRemove.Add) ? "added." : "removed."); } }


		public EventDocumentAddRemove (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, AddRemove _addRemove, int _documentID)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			addRemove = _addRemove;
			documentID = _documentID;
		}


		public EventDocumentAddRemove () {}


		public override void Register ()
		{
			EventManager.OnDocumentAdd += OnDocumentAdd;
			EventManager.OnDocumentRemove += OnDocumentRemove;
		}


		public override void Unregister ()
		{
			EventManager.OnDocumentAdd -= OnDocumentAdd;
			EventManager.OnDocumentRemove -= OnDocumentRemove;
		}


		private void OnDocumentAdd (DocumentInstance documentInstance)
		{
			if (addRemove == AddRemove.Add && (documentID < 0 || documentID == documentInstance.DocumentID))
			{
				Run (new object[] { documentInstance.DocumentID });
			}
		}


		private void OnDocumentRemove (DocumentInstance documentInstance)
		{
			if (addRemove == AddRemove.Remove && (documentID < 0 || documentID == documentInstance.DocumentID))
			{
				Run (new object[] { documentInstance.DocumentID });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.Document, "Document")
			};
		}


		private string GetDocumentName ()
		{
			if (KickStarter.inventoryManager)
			{
				Document document = KickStarter.inventoryManager.GetDocument (documentID);
				if (document != null) return "document '" + document.title + "'";
			}
			return "document " + documentID;
		}


#if UNITY_EDITOR

		public override void AssignVariant (int variantIndex)
		{
			addRemove = (AddRemove) variantIndex;
		}


		protected override bool HasConditions (bool isAssetFile) { return true; }


		protected override void ShowConditionGUI (bool isAssetFile)
		{
			if (KickStarter.inventoryManager)
			{
				documentID = ActionRunActionList.ShowDocumentSelectorGUI ("Document:", KickStarter.inventoryManager.documents, documentID);
			}
			else
			{
				documentID = CustomGUILayout.IntField ("Document ID:", documentID);
			}
		}

#endif

	}

}