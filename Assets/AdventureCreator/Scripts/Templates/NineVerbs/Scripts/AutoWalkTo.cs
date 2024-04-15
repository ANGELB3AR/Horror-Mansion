using UnityEngine;

namespace AC.Templates.NineVerbs
{

	public class AutoWalkTo : MonoBehaviour
	{

		#region Variables

		public int walkToIconID = 9;

		#endregion


		#region UnityStandards

		private void OnEnable ()
		{
			EventManager.OnHotspotSelect += OnHotspotSelect;
			EventManager.OnHotspotDeselect += OnHotspotDeselect;
			EventManager.OnHotspotInteract += OnHotspotInteract;
		}


		private void OnDisable ()
		{
			EventManager.OnHotspotSelect -= OnHotspotSelect;
			EventManager.OnHotspotDeselect -= OnHotspotDeselect;
			EventManager.OnHotspotInteract -= OnHotspotInteract;
		}

		#endregion


		#region CustomEvents

		private void OnHotspotSelect (Hotspot hotspot)
		{
			if (HotspotHasWalkToInteraction (hotspot))
			{
				if (KickStarter.playerCursor.GetSelectedCursorID () == -1 && KickStarter.runtimeInventory.SelectedItem == null)
				{
					KickStarter.playerCursor.SetCursorFromID (walkToIconID);
				}
			}
		}


		private void OnHotspotDeselect (Hotspot hotspot)
		{
			if (HotspotHasWalkToInteraction (hotspot))
			{
				if (KickStarter.playerCursor.GetSelectedCursorID () == walkToIconID && KickStarter.runtimeInventory.SelectedItem == null)
				{
					KickStarter.playerCursor.ResetSelectedCursor ();
				}
			}
		}


		private void OnHotspotInteract (Hotspot hotspot, Button button)
		{
			if (button != null && button.iconID == walkToIconID)
			{
				OnHotspotSelect (hotspot);
			}
		}

		#endregion


		#region PrivateFunctions

		private bool HotspotHasWalkToInteraction (Hotspot hotspot)
		{
			return (hotspot.GetUseButton (walkToIconID) != null);
		}

		#endregion
		
	}

}