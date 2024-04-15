/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"HotspotLabelData.cs"
 * 
 *	This script stores data related to the Hotspot label - both what it is and where it's coming from
 * 
 */


using UnityEngine;

namespace AC
{

	public class HotspotLabelData
	{

		#region Variables

		/** The label text to display */
		public string HotspotLabel { get; private set; }
		/** If True, the label text is set */
		public bool HasLabel { get; private set; }
		/** The Hotspot that the label is sourced from */
		public Hotspot Hotspot { get; private set; }
		private MenuElement menuElement;

		private int elementSlot;
		private Hotspot backupHotspot;
		public InvInstance InvInstance { get; private set; }

		#endregion


		#region PublicFunctions

		public void SetData (MenuElement _menuElement, int slot, string label)
		{
			menuElement = _menuElement;
			elementSlot = slot;
			Hotspot = null;
			InvInstance = null;
			HotspotLabel = label;
			HasLabel = !string.IsNullOrEmpty (HotspotLabel);
			backupHotspot = null;
		}


		public void SetData (MenuElement _menuElement, int slot, Hotspot _hotspot, string label)
		{
			menuElement = _menuElement;
			elementSlot = slot;
			Hotspot = _hotspot;
			InvInstance = null;
			HotspotLabel = label;
			HasLabel = !string.IsNullOrEmpty (HotspotLabel);
			backupHotspot = null;
		}


		public void SetData (MenuElement _menuElement, int slot, InvInstance _invInstance, string label)
		{
			menuElement = _menuElement;
			elementSlot = slot;
			Hotspot = null;
			InvInstance = _invInstance;
			HotspotLabel = label;
			HasLabel = !string.IsNullOrEmpty (HotspotLabel);
			backupHotspot = null;
		}


		public void SetData (Hotspot _hotspot, string label)
		{
			Hotspot = _hotspot;
			HotspotLabel = label;
			InvInstance = null;
			menuElement = null;
			HasLabel = !string.IsNullOrEmpty (HotspotLabel);
			backupHotspot = null;
		}


		public void SetData (Hotspot _hotspot, InvInstance _invInstance, string label)
		{
			Hotspot = _hotspot;
			HotspotLabel = label;
			InvInstance = _invInstance;
			menuElement = null;
			HasLabel = !string.IsNullOrEmpty (HotspotLabel);
			backupHotspot = null;
		}


		public void SetData (InvInstance _invInstance, string label)
		{
			InvInstance = _invInstance;
			HotspotLabel = label;
			Hotspot = null;
			menuElement = null;
			HasLabel = !string.IsNullOrEmpty (HotspotLabel);
			backupHotspot = null;
		}


		public void SetData (string label)
		{
			backupHotspot = Hotspot;
			HotspotLabel = label;
			InvInstance = null;
			Hotspot = null;
			menuElement = null;
			HasLabel = !string.IsNullOrEmpty (HotspotLabel);
		}


		public void ClearString ()
		{
			HotspotLabel = string.Empty;
			HasLabel = false;
			SetData (string.Empty);
		}


		public bool HasData
		{
			get
			{
				return menuElement != null || Hotspot || InvInstance.IsValid (InvInstance);
			}
		}


		public bool SourceMatches (HotspotLabelData data)
		{
			return menuElement == data.menuElement && elementSlot == data.elementSlot && Hotspot == data.Hotspot && InvInstance == data.InvInstance;
		}


		public void Copy (HotspotLabelData data)
		{
			if (!string.IsNullOrEmpty (data.HotspotLabel))
				backupHotspot = data.backupHotspot;

			menuElement = data.menuElement;
			elementSlot = data.elementSlot;
			Hotspot = data.Hotspot;
			InvInstance = data.InvInstance;
			HotspotLabel = data.HotspotLabel;
			HasLabel = !string.IsNullOrEmpty (HotspotLabel);
		}


		/*public void ShowGUI (string title)
		{
			GUILayout.BeginVertical (CustomStyles.thinBox, GUILayout.ExpandWidth (true), GUILayout.ExpandHeight (true));
			GUILayout.Label (" = " + title + " = ");
			if (Hotspot) GUILayout.Label ("Hotspot: " + Hotspot);
			if (backupHotspot) GUILayout.Label ("backupHotspot: " + backupHotspot);
			if (InvInstance.IsValid (InvInstance)) GUILayout.Label ("Inv item: " + InvInstance.ItemLabel);
			if (menuElement != null) GUILayout.Label ("Element:" + menuElement.title + ", Slot: " + elementSlot);
			if (HasLabel) GUILayout.Label ("Label: " + HotspotLabel);
			GUILayout.EndVertical ();
		}*/


		public void UpdateAutoPosition (Menu menu)
		{
			if (Hotspot) backupHotspot = Hotspot;

			Vector2 autoPosition = Vector2.zero;
			Hotspot hotspot = Hotspot ? Hotspot : backupHotspot;

			if (GetPosition (hotspot, ref autoPosition))
			{
				if (menu.IsUnityUI ())
				{
					if (menu.RuntimeCanvas == null)
					{
						ACDebug.LogWarning ("Cannot move UI menu " + menu.title + " as no Canvas is assigned!");
					}
					else if (menu.RuntimeCanvas.renderMode == RenderMode.WorldSpace && hotspot)
					{
						menu.SetCentre3D (hotspot.GetIconPosition ());
					}
					else
					{
						menu.SetCentre (new Vector2 (autoPosition.x * ACScreen.width,
												(1f - autoPosition.y) * ACScreen.height));
					}
				}
				else
				{
					menu.SetCentre (new Vector2 (autoPosition.x + (menu.manualPosition.x / 100f) - 0.5f,
											autoPosition.y + (menu.manualPosition.y / 100f) - 0.5f));
				}
			}
		}

		#endregion


		#region PrivateFunctions

		private bool GetPosition (Hotspot hotspot, ref Vector2 position)
		{
			if (menuElement != null)
			{
				Vector2 elementCentre = menuElement.ParentMenu.GetSlotCentre (menuElement, elementSlot);

				position = new Vector2 (elementCentre.x / ACScreen.width, elementCentre.y / ACScreen.height);
				return true;
			}

			if (hotspot)
			{
				Vector2 screenPos = hotspot.GetIconScreenPosition ();

				position = new Vector2 (screenPos.x / ACScreen.width, 1f - (screenPos.y / ACScreen.height));
				return true;
			}

			return false;
		}

		#endregion

	}

}