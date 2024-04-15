/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"RememberConversation.cs"
 * 
 *	This script is attached to conversation objects in the scene
 *	with DialogOption states we wish to save.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/** Attach this script to Conversation objects in the scene with DialogOption states you wish to save. */
	[AddComponentMenu("Adventure Creator/Save system/Remember Conversation")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_conversation.html")]
	public class RememberConversation : Remember
	{

		#region Variables

		[SerializeField] private Conversation conversationToSave = null;

		#endregion


		#region PublicFunctions

		public override string SaveData ()
		{
			if (Conversation == null) return string.Empty;

			ConversationData conversationData = new ConversationData();
			conversationData.objectID = constantID;
			conversationData.savePrevented = savePrevented;

			List<bool> optionStates = new List<bool>();
			List<bool> optionLocks = new List<bool>();
			List<bool> optionChosens = new List<bool>();
			List<string> optionLabels = new List<string>();
			List<int> optionLineIDs = new List<int>();

			foreach (ButtonDialog _option in Conversation.options)
			{
				optionStates.Add (_option.isOn);
				optionLocks.Add (_option.isLocked);
				optionChosens.Add (_option.hasBeenChosen);
				optionLabels.Add (_option.label);
				optionLineIDs.Add (_option.lineID);
			}

			conversationData._optionStates = ArrayToString <bool> (optionStates.ToArray ());
			conversationData._optionLocks = ArrayToString <bool> (optionLocks.ToArray ());
			conversationData._optionChosens = ArrayToString <bool> (optionChosens.ToArray ());
			conversationData._optionLabels = ArrayToString <string> (optionLabels.ToArray ());
			conversationData._optionLineIDs = ArrayToString <int> (optionLineIDs.ToArray ());

			conversationData.lastOption = Conversation.lastOption;

			return Serializer.SaveScriptData <ConversationData> (conversationData);
		}


		public override void LoadData (string stringData)
		{
			if (Conversation == null) return;

			ConversationData data = Serializer.LoadScriptData <ConversationData> (stringData);
			if (data == null) return;
			SavePrevented = data.savePrevented; if (savePrevented) return;

			bool[] optionStates = StringToBoolArray (data._optionStates);
			bool[] optionLocks = StringToBoolArray (data._optionLocks);
			bool[] optionChosens = StringToBoolArray (data._optionChosens);
			string[] optionLabels = StringToStringArray (data._optionLabels);
			int[] optionLineIDs = StringToIntArray (data._optionLineIDs);

			for (int i=0; i< Conversation.options.Count; i++)
			{
				if (optionStates != null && optionStates.Length > i)
				{
					Conversation.options[i].isOn = optionStates[i];
				}

				if (optionLocks != null && optionLocks.Length > i)
				{
					Conversation.options[i].isLocked = optionLocks[i];
				}

				if (optionChosens != null && optionChosens.Length > i)
				{
					Conversation.options[i].hasBeenChosen = optionChosens[i];
				}

				if (optionLabels != null && optionLabels.Length > i)
				{
					Conversation.options[i].label = optionLabels[i];
				}

				if (optionLineIDs != null && optionLineIDs.Length > i)
				{
					Conversation.options[i].lineID = optionLineIDs[i];
				}
			}

			Conversation.lastOption = data.lastOption;
		}


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			if (conversationToSave == null) conversationToSave = GetComponent<Conversation> ();

			CustomGUILayout.Header ("Conversation");
			CustomGUILayout.BeginVertical ();
			conversationToSave = (Conversation) CustomGUILayout.ObjectField<Conversation> ("Conversation to save:", conversationToSave, true);
			CustomGUILayout.EndVertical ();
		}

		#endif

		#endregion


		#region GetSet

		private Conversation Conversation
		{
			get
			{
				if (conversationToSave == null)
				{
					conversationToSave = GetComponent <Conversation>();
				}
				return conversationToSave;
			}
		}

		#endregion

	}


	/**
	 * A data container used by the RememberConversation script.
	 */
	[System.Serializable]
	public class ConversationData : RememberData
	{

		/** The enabled state of each DialogOption */
		public string _optionStates;
		/** The locked state of each DialogOption */
		public string _optionLocks;
		/** The 'already chosen' state of each DialogOption */
		public string _optionChosens;
		/** The index of the last-chosen option */
		public int lastOption;
		/** The labels of each DialogOption */
		public string _optionLabels;
		/** The line IDs of each DialogOption */
		public string _optionLineIDs;

		/**
		 * The default Constructor.
		 */
		public ConversationData () { }
	}

}