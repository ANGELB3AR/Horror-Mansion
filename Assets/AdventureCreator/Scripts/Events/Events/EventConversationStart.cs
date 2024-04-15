using UnityEngine;

namespace AC
{

	public class EventConversationStart : EventBase
	{

		[SerializeField] private Conversation conversation = null;
		[SerializeField] private StartEnd startEnd;
		public enum StartEnd { Start, End };


		public override string[] EditorNames { get { return new string[] { "Conversation/Begin", "Conversation/End" }; } }
		
		protected override string EventName { get { return startEnd == StartEnd.Start ? "OnStartConversation" : "OnEndConversation"; } }
		protected override string ConditionHelp { get { return "Whenever " + (conversation ? "Conversation '" + conversation.name + "' " : "a Converation ") + startEnd.ToString ().ToLower () + "s."; } }


		public EventConversationStart (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, Conversation _conversation, StartEnd _startEnd)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			conversation = _conversation;
			startEnd = _startEnd;
		}


		public EventConversationStart () {}


		public override void Register ()
		{
			EventManager.OnStartConversation += OnStartConversation;
			EventManager.OnEndConversation += OnEndConversation;
		}


		public override void Unregister ()
		{
			EventManager.OnStartConversation -= OnStartConversation;
			EventManager.OnEndConversation -= OnEndConversation;
		}


		private void OnStartConversation (Conversation _conversation)
		{
			if (startEnd == StartEnd.Start && (conversation == null || conversation == _conversation))
			{
				Run (new object[] { _conversation.gameObject });
			}
		}


		private void OnEndConversation (Conversation _conversation)
		{
			if (startEnd == StartEnd.End && (conversation == null || conversation == _conversation))
			{
				Run (new object[] { _conversation.gameObject });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.GameObject, "Conversation")
			};
		}


#if UNITY_EDITOR

		public override void AssignVariant (int variantIndex)
		{
			startEnd = (StartEnd) variantIndex;
		}


		protected override bool HasConditions (bool isAssetFile)
		{
			return !isAssetFile;
		}


		protected override void ShowConditionGUI (bool isAssetFile)
		{
			if (!isAssetFile)
			{
				conversation = (Conversation) CustomGUILayout.ObjectField<Conversation> ("Conversation:", conversation, true);
			}
		}

#endif

	}

}