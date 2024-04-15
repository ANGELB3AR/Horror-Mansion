using UnityEngine;

namespace AC
{

	public class EventMenuHover : EventBase
	{

		[SerializeField] private string menuName;
		[SerializeField] private string elementName;


		public override string[] EditorNames { get { return new string[] { "Menu/Hover" }; } }
		protected override string EventName { get { return "OnMouseOverMenu"; } }
		protected override string ConditionHelp { get { return "Whenever " + (!string.IsNullOrEmpty (menuName) ? "menu '" + menuName + "'" : "a menu") + (!string.IsNullOrEmpty (menuName) && !string.IsNullOrEmpty (elementName) ? " has its '" + elementName + "' element hovered over." : " is hovered over"); } }


		public EventMenuHover (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, string _menuName, string _elementName)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			menuName = _menuName;
			elementName = _elementName;
		}


		public EventMenuHover () {}


		public override void Register ()
		{
			EventManager.OnMouseOverMenu += OnMouseOverMenu;
		}


		public override void Unregister ()
		{
			EventManager.OnMouseOverMenu -= OnMouseOverMenu;
		}


		private void OnMouseOverMenu (Menu menu, MenuElement element, int _slot)
		{
			if (!string.IsNullOrEmpty (menuName))
			{
				if (menu.title != menuName) return;
				if (!string.IsNullOrEmpty (elementName) && (element == null || element.title != elementName)) return;
			}

			Run (new object[] { menu.title, (element != null) ? element.title : string.Empty, _slot });
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