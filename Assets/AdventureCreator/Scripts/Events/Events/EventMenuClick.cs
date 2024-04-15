using UnityEngine;

namespace AC
{

	public class EventMenuClick : EventBase
	{

		[SerializeField] private string menuName;
		[SerializeField] private string elementName;


		public override string[] EditorNames { get { return new string[] { "Menu/Click" }; } }
		protected override string EventName { get { return "OnMenuElementClick"; } }
		protected override string ConditionHelp { get { return "Whenever " + (!string.IsNullOrEmpty (menuName) ? "menu " + menuName + "'s " : "a menu's ") + (!string.IsNullOrEmpty (menuName) && !string.IsNullOrEmpty (elementName) ? "'" + elementName + "' element is" : "elements are") + " clicked on."; } }


		public EventMenuClick (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, string _menuName, string _elementName)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			menuName = _menuName;
			elementName = _elementName;
		}


		public EventMenuClick () {}


		public override void Register ()
		{
			EventManager.OnMenuElementClick += OnMenuElementClick;
		}


		public override void Unregister ()
		{
			EventManager.OnMenuElementClick -= OnMenuElementClick;
		}


		private void OnMenuElementClick (Menu menu, MenuElement element, int _slot, int buttonPressed)
		{
			if (!string.IsNullOrEmpty (menuName))
			{
				if (menu.title != menuName) return;
				if (!string.IsNullOrEmpty (elementName) && (element == null || element.title != elementName)) return;
			}

			Run (new object[] { menu.title, element.title, _slot });
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.String, "Menu name"),
				new ParameterReference (ParameterType.String, "Element name"),
				new ParameterReference (ParameterType.Integer, "Slot index"),
			};
		}


#if UNITY_EDITOR

		protected override bool HasConditions (bool isAssetFile) { return true; }


		protected override void ShowConditionGUI (bool isAssetFile)
		{
			menuName = CustomGUILayout.TextField ("Menu name:", menuName);
			if (!string.IsNullOrEmpty (menuName))
				elementName = CustomGUILayout.TextField ("Element name:", elementName);
		}

#endif

	}

}