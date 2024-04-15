using UnityEngine;

namespace AC
{

	public class EventMenuTurnOnOff : EventBase
	{

		[SerializeField] private AC_OnOff onOff;
		[SerializeField] private string menuName;


		public override string[] EditorNames { get { return new string[] { "Menu/Turn on", "Menu/Turn off" }; } }
		protected override string EventName { get { return onOff == AC_OnOff.On ? "OnMenuTurnOn" : "OnMenuTurnOff"; } }
		protected override string ConditionHelp { get { return "Whenever " + (string.IsNullOrEmpty (menuName) ? "a menu" : "menu '" + menuName + "'") + " is turned " + onOff.ToString ().ToLower () + "."; } }


		public EventMenuTurnOnOff (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, AC_OnOff _onOff, string _menuName)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			onOff = _onOff;
			menuName = _menuName;
		}


		public EventMenuTurnOnOff () {}


		public override void Register ()
		{
			EventManager.OnMenuTurnOn += OnMenuTurnOn;
			EventManager.OnMenuTurnOff += OnMenuTurnOff;
		}

		public override void Unregister ()
		{
			EventManager.OnMenuTurnOn -= OnMenuTurnOn;
			EventManager.OnMenuTurnOff -= OnMenuTurnOff;
		}


		private void OnMenuTurnOn (Menu _menu, bool isInstant)
		{
			if (onOff == AC_OnOff.On && (_menu.title == menuName || string.IsNullOrEmpty (menuName)))
			{
				Run (new object[] { _menu.title });
			}
		}


		private void OnMenuTurnOff (Menu _menu, bool isInstant)
		{
			if (onOff == AC_OnOff.Off && (_menu.title == menuName || string.IsNullOrEmpty (menuName)))
			{
				Run (new object[] { _menu.title });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[] 
			{
				new ParameterReference (ParameterType.String, "Menu name")
			};
		}


#if UNITY_EDITOR

		public override void AssignVariant (int variantIndex)
		{
			onOff = (variantIndex == 0) ? AC_OnOff.On : AC_OnOff.Off;
		}


		protected override bool HasConditions (bool isAssetFile) { return true; }


		protected override void ShowConditionGUI (bool isAssetFile)
		{
			menuName = CustomGUILayout.TextField ("Menu name:", menuName);
		}

#endif

	}

}