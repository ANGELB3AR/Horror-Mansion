namespace AC
{

	public class EventSaveProfile : EventBase
	{


		public override string[] EditorNames { get { return new string[] { "Save/Switch profile" }; } }
		protected override string EventName { get { return "OnSwitchProfile"; } }
		protected override string ConditionHelp { get { return "Whenever the active profile is set."; } }


		public EventSaveProfile (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
		}


		public EventSaveProfile () {}


		public override void Register ()
		{
			EventManager.OnSwitchProfile += OnSwitchProfile;
		}


		public override void Unregister ()
		{
			EventManager.OnSwitchProfile -= OnSwitchProfile;
		}


		private void OnSwitchProfile (int profileID)
		{
			Run (new object[] { profileID });
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.Integer, "Profile ID"),
			};
		}


#if UNITY_EDITOR

		protected override bool HasConditions (bool isAssetFile) { return false; }

#endif

	}

}