using UnityEngine;

namespace AC
{

	public class EventConversationClick : EventBase
	{

		[SerializeField] private Conversation conversation = null;

		public override string[] EditorNames { get { return new string[] { "Conversation/Click" }; } }
		
		protected override string EventName { get { return "OnClickConversation"; } }
		protected override string ConditionHelp { get { return "Whenever " + (conversation ? "Conversation '" + conversation.name + "' " : "a Converation ") + "is clicked."; } }


		public EventConversationClick (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, Conversation _conversation)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			conversation = _conversation;
		}


		public EventConversationClick () {}


		public override void Register ()
		{
			EventManager.OnClickConversation += OnClickConversation;
		}


		public override void Unregister ()
		{
			EventManager.OnClickConversation -= OnClickConversation;
		}


		private void OnClickConversation (Conversation _conversation, int optionID)
		{
			if (conversation == null || conversation == _conversation)
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