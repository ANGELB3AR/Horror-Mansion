/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"UISlotClickRight.cs"
 * 
 *	A subclass of UISlotClick that also listens for right-clicks.
 * 
 */

using UnityEngine;
using UnityEngine.EventSystems;

namespace AC
{

	/** A subclass of UISlotClick that also listens for right-clicks. */
	public class UISlotClickRight : UISlotClick, IPointerClickHandler
	{

		#region UnityStandards

		private void Update ()
		{
			if (menuElement)
			{
				if (KickStarter.playerInput && KickStarter.playerInput.InputGetButtonDown ("InteractionB"))
				{
					if (KickStarter.playerMenus.IsEventSystemSelectingObject (gameObject))
					{
						menuElement.ProcessClick (menu, slot, MouseState.RightClick);
						return;
					}
				}

				if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && KickStarter.playerInput && KickStarter.playerInput.InputTouchCount () == 2 && KickStarter.playerInput.InputTouchPhase (1) == TouchPhase.Began)
				{
					if (KickStarter.playerMenus.IsEventSystemSelectingObject (gameObject))
					{
						menuElement.ProcessClick (menu, slot, MouseState.RightClick);
					}
				}
			}
		}

		
		/** Implementation of IPointerClickHandler */
		public void OnPointerClick (PointerEventData eventData)
		{
			if (menuElement)
			{
				if (KickStarter.settingsManager.defaultMouseClicks && eventData.button == PointerEventData.InputButton.Right)
				{
					menuElement.ProcessClick (menu, slot, MouseState.RightClick);
				}
			}
		}

		#endregion

	}

}