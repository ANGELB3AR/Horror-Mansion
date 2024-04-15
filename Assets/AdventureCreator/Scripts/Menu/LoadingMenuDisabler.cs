/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"LoadingMenuDisabler.cs"
 * 
 *	Disables any non-loading Menu's UI Canvas while enabled.  It is added automatially when inside a loading scene.
 * 
 */

using System.Collections.Generic;
using UnityEngine;

namespace AC
{

	/**
	 * Disables any non-loading Menu's UI Canvas while enabled.  It is added automatially when inside a loading scene.
	 */
	public class LoadingMenuDisabler : MonoBehaviour
	{

		#region Variables

		private readonly HashSet<Menu> menusToReEnable = new HashSet<Menu> ();

		#endregion


		#region UnityStandards

		private void OnEnable ()
		{
			menusToReEnable.Clear ();
			if (KickStarter.playerMenus)
			{
				List<Menu> menus = PlayerMenus.GetMenus (true);
				foreach (Menu menu in menus)
				{
					if (menu.RuntimeCanvas && menu.RuntimeCanvas.gameObject.activeSelf && menu.appearType != AppearType.WhileLoading)
					{
						menusToReEnable.Add (menu);
						menu.RuntimeCanvas.gameObject.SetActive (false);
					}
				}
			}
		}


		private void OnDisable ()
		{
			foreach (Menu menu in menusToReEnable)
			{
				menu.RuntimeCanvas.gameObject.SetActive (true);
			}
		}

		#endregion

	}

}