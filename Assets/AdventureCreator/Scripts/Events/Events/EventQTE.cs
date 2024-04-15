using UnityEngine;

namespace AC
{

	public class EventQTE : EventBase
	{

		[SerializeField] private QteCondition qteCondition;
		public enum QteCondition { Begin, Win, Lose };


		public override string[] EditorNames { get { return new string[] { "QTE/Begin", "QTE/Win", "QTE/Lose" }; } }
		
		protected override string EventName { get { return "OnQTE" + qteCondition.ToString (); } }
		protected override string ConditionHelp { get { return "Whenever a QTE " + qteCondition.ToString ().ToLower () + "s."; } }


		public EventQTE (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, QteCondition _qteCondition)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			qteCondition = _qteCondition;
		}


		public EventQTE () {}


		public override void Register ()
		{
			EventManager.OnQTEBegin += OnQTEBegin;
			EventManager.OnQTEWin += OnQTEWin;
			EventManager.OnQTELose += OnQTELose;
		}


		public override void Unregister ()
		{
			EventManager.OnQTEBegin -= OnQTEBegin;
			EventManager.OnQTEWin -= OnQTEWin;
			EventManager.OnQTELose -= OnQTELose;
		}


		private void OnQTEBegin (QTEType qteType, string inputName, float duration)
		{
			if (qteCondition == QteCondition.Begin) Run ();
		}


		private void OnQTEWin (QTEType qteType)
		{
			if (qteCondition == QteCondition.Win) Run ();
		}


		private void OnQTELose (QTEType qteType)
		{
			if (qteCondition == QteCondition.Lose) Run ();
		}


#if UNITY_EDITOR

		protected override bool HasConditions (bool isAssetFile) { return false; }


		public override void AssignVariant (int variantIndex)
		{
			qteCondition = (QteCondition) variantIndex;
		}

#endif

	}

}