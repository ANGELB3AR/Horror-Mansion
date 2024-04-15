/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"HeadTurnPlayableBehaviour.cs"
 * 
 *	A PlayableBehaviour used by HeadTurnMixer.
 * 
 */

#if TimelineIsPresent

using UnityEngine;
using UnityEngine.Playables;

namespace AC
{

	/**
	 * A PlayableBehaviour used by HeadTurnMixer.
	 */
	internal sealed class HeadTurnPlayableBehaviour : PlayableBehaviour
	{

		#region Variables

		public Transform headTurnTarget;
		public Vector3 headTurnOffset;

		#endregion


		#region GetSet

		public bool IsValid
		{
			get
			{
				return headTurnTarget != null;
			}
		}

		#endregion

	}

}
#endif