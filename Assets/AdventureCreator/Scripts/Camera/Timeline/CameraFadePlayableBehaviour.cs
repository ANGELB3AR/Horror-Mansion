/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"CameraFadePlayableBehaviour.cs"
 * 
 *	A PlayableBehaviour used by CameraFadeMixer.
 * 
 */

using UnityEngine;
using UnityEngine.Playables;
#if TimelineIsPresent
using UnityEngine.Timeline;

namespace AC
{

	/**
	 * A PlayableBehaviour used by CameraFadeMixer.
	 */
	internal sealed class CameraFadePlayableBehaviour : PlayableBehaviour
	{

		#region Variables

		public Texture2D overlayTexture;

		#endregion


		#region GetSet

		public bool IsValid
		{
			get
			{
				return overlayTexture != null;
			}
		}

		#endregion

	}

}
#endif