#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AC
{

	public struct ActionMenuItem
	{

		public readonly string Label;
		public readonly System.Action<List<Action>> Callback;

		public ActionMenuItem (string label, System.Action<List<Action>> callback)
		{
			Label = label;
			Callback = callback;
		}

	}

}

#endif