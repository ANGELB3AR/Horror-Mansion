using UnityEngine;

namespace AC
{

	public class EventHotspotSelect : EventBase
	{

		[SerializeField] private bool onSelect;
		[SerializeField] private Hotspot hotspot = null;
		

		public override string[] EditorNames { get { return new string[] { "Hotspot/Select", "Hotspot/Deselect" }; } }
		protected override string EventName { get { return onSelect ? "OnHotspotSelect" : "OnHotspotDeselect"; } }
		protected override string ConditionHelp { get { return "Whenever " + (hotspot ? "hotspot '" + hotspot.GetName (0) + "'" : "a Hotspot") + " is " + (onSelect ? "selected." : "deselected."); } }


		public EventHotspotSelect (int _id, string _label, ActionListAsset _actionListAsset, int[] _parameterIDs, bool _onSelect, Hotspot _hotspot)
		{
			id = _id;
			label = _label;
			actionListAsset = _actionListAsset;
			parameterIDs = _parameterIDs;
			onSelect = _onSelect;
			hotspot = _hotspot;
		}


		public EventHotspotSelect () {}


		public override void Register ()
		{
			EventManager.OnHotspotSelect += OnHotspotSelect;
			EventManager.OnHotspotDeselect += OnHotspotDeselect;
		}


		public override void Unregister ()
		{
			EventManager.OnHotspotSelect -= OnHotspotSelect;
			EventManager.OnHotspotDeselect -= OnHotspotDeselect;
		}


		private void OnHotspotDeselect (Hotspot _hotspot)
		{
			if (!onSelect && (hotspot == null || hotspot == _hotspot))
			{
				Run (new object[] { _hotspot });
			}
		}
		

		private void OnHotspotSelect (Hotspot _hotspot)
		{
			if (onSelect && (hotspot == null || hotspot == _hotspot))
			{
				Run (new object[] { _hotspot.gameObject });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.GameObject, "Hotspot")
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
				hotspot = (Hotspot) CustomGUILayout.ObjectField<Hotspot> ("Hotspot:", hotspot, true);
			}
		}


		public override void AssignVariant (int variantIndex)
		{
			onSelect = (variantIndex == 0);
		}

#endif

	}

}