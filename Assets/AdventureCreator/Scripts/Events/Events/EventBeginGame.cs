namespace AC
{

	public class EventBeginGame : EventBase
	{

		public override string[] EditorNames { get { return new string[] { "Scene/Begin game" }; } }
		protected override string EventName { get { return "OnBeginGame"; } }
		protected override string ConditionHelp { get { return "Whenever the first scene runs for the first time."; } }


		public EventBeginGame (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
		}


		public EventBeginGame () {}


		public override void Register ()
		{
			EventManager.OnBeginGame += OnBeginGame;
		}


		public override void Unregister ()
		{
			EventManager.OnBeginGame -= OnBeginGame;
		}


		private void OnBeginGame ()
		{
			Run ();
		}


#if UNITY_EDITOR

		protected override bool HasConditions (bool isAssetFile) { return false; }

#endif

	}

}