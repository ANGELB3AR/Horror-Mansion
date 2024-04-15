/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"NavMeshSegment.cs"
 * 
 *	This script is used for the NavMeshSegment prefab, which defines
 *	the area to be baked by the Unity Navigation window.
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * Controls a navigation area used by Unity Navigation-based pathfinding method.
	 */
	public class NavMeshSegment : NavMeshBase
	{

		#region UnityStandards

		protected void Awake ()
		{
			BaseAwake ();

			if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.UnityNavigation)
			{
				if (LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer) == -1)
				{
					ACDebug.LogWarning ("No 'NavMesh' layer exists - please define one in the Tags Manager.");
				}
				else
				{
					gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer);
				}
			}
		}

		#endregion

	}

}