using UnityEngine;

namespace AC
{

	public class EventDocumentOpenClose : EventBase
	{

		[SerializeField] private OpenClose openClose;
		public enum OpenClose { Open, Close };
		[SerializeField] private int documentID = -1;


		public override string[] EditorNames { get { return new string[] { "Document/Open", "Document/Close" }; } }
		protected override string EventName { get { return openClose == OpenClose.Open ? "OnDocumentOpen" : "OnDocumentClose"; } }
		protected override string ConditionHelp { get { return "Whenever " + ((documentID >= 0) ? GetDocumentName () : "a Document") + " is " + ((openClose == OpenClose.Open) ? "opened." : "closed."); } }


		public EventDocumentOpenClose (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, OpenClose _openClose, int _documentID)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			openClose = _openClose;
			documentID = _documentID;
		}


		public EventDocumentOpenClose () {}


		public override void Register ()
		{
			EventManager.OnDocumentOpen += OnDocumentOpen;
			EventManager.OnDocumentClose += OnDocumentClose;
		}


		public override void Unregister ()
		{
			EventManager.OnDocumentOpen -= OnDocumentOpen;
			EventManager.OnDocumentClose -= OnDocumentClose;
		}


		private void OnDocumentOpen (DocumentInstance documentInstance)
		{
			if (openClose == OpenClose.Open && (documentID < 0 || documentID == documentInstance.DocumentID))
			{
				Run (new object[] { documentInstance.DocumentID });
			}
		}


		private void OnDocumentClose (DocumentInstance documentInstance)
		{
			if (openClose == OpenClose.Close && (documentID < 0 || documentID == documentInstance.DocumentID))
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
			openClose = (OpenClose) variantIndex;
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