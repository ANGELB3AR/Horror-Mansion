/*
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"iActionListAssetReferencer.cs"
 * 
 *	An interface used to aid the location of references to ActionListAsset files
 * 
 */

using System.Collections.Generic;

namespace AC
{

	/** An interface used to aid the location of references to ActionListAsset files */
	public interface iActionListAssetReferencer
	{

		#if UNITY_EDITOR

		bool ReferencesAsset (ActionListAsset actionListAsset);
		List<ActionListAsset> GetReferencedActionListAssets ();

		#endif

	}

}