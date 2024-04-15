using UnityEngine;

namespace AC
{

	public class EventSpeechStart : EventBase
	{

		[SerializeField] private SpeakerType speakerType;
		public enum SpeakerType { Character, Narrator };
		[SerializeField] private StartStop startStop;
		public enum StartStop { Start, Stop };
		

		public override string[] EditorNames { get { return new string[] { "Speech/Start", "Speech/Stop" }; } }
		protected override string EventName { get { return startStop == StartStop.Start? "OnStartSpeech" : "OnStopSpeech"; } }
		protected override string ConditionHelp { get { return "Whenever " + ((speakerType == SpeakerType.Character) ? "speech " : "narration ") + startStop.ToString ().ToLower () + "."; } }


		public EventSpeechStart (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, SpeakerType _speakerType, StartStop _startStop)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			speakerType = _speakerType;
			startStop = _startStop;
		}


		public EventSpeechStart () {}


		public override void Register ()
		{
			EventManager.OnStartSpeech_Alt += OnStartSpeech;
			EventManager.OnStopSpeech_Alt += OnStopSpeech;
		}


		public override void Unregister ()
		{
			EventManager.OnStartSpeech_Alt -= OnStartSpeech;
			EventManager.OnStopSpeech_Alt -= OnStopSpeech;
		}


		private void OnStartSpeech (Speech speech)
		{
			if (startStop == StartStop.Start)
			{
				if (speech.speaker && speakerType == SpeakerType.Character)
				{
					Run (new object[] { speech.speaker.gameObject });
				}
				else if (speech.speaker == null && speakerType == SpeakerType.Narrator)
				{
					Run ();
				}
			}
		}


		private void OnStopSpeech (Speech speech)
		{
			if (startStop == StartStop.Stop)
			{
				if (speech.speaker && speakerType == SpeakerType.Narrator)
				{
					Run (new object[] { speech.speaker.gameObject });
				}
				else if (speech.speaker == null && speakerType == SpeakerType.Narrator)
				{
					Run ();
				}
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			if (speakerType == SpeakerType.Character)
			{
				return new ParameterReference[]
				{
					new ParameterReference (ParameterType.GameObject, "Speaker")
				};
			}
			else
			{
				return new ParameterReference[0];
			}
		}


#if UNITY_EDITOR

		public override void AssignVariant (int variantIndex)
		{
			startStop = (StartStop) variantIndex;
		}


		protected override bool HasConditions (bool isAssetFile) { return true; }


		protected override void ShowConditionGUI (bool isAssetFile)
		{
			speakerType = (SpeakerType) CustomGUILayout.EnumPopup ("Speaker type:", speakerType);
		}

#endif

	}

}