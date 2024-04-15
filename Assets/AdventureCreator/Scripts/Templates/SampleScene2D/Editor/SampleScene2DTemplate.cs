#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace AC.Templates.SampleScene2D
{

	public class SampleScene2DTemplate : Template
	{

		#region PublicFunctions

		public override bool CanInstall (ref string errorText)
		{
			return true;
		}


		public override bool CanSuggest (NGWData data)
		{
			return data.CameraPerspective == CameraPerspective.TwoD;
		}

		#endregion


		#region ProtectedFunctions

		protected override void MakeChanges (string installPath, bool canDeleteOldAssets, System.Action onComplete, System.Action<string> onFail)
		{
			onComplete?.Invoke ();
		}

		#endregion


		#region GetSet

		public override string Label { get { return "2D sample scene"; }}
		public override string PreviewText { get { return "A sample scene for 2D games, demonstrating Camera, Hotspots and Conversations."; }}
		public override Type[] AffectedManagerTypes { get { return new Type[0]; }}
		public override string FolderName { get { return "SampleScene2D"; }}
		public override bool SelectedByDefault { get { return true; }}
		public override TemplateCategory Category { get { return TemplateCategory.SampleScene; }}
		public override bool IsExclusiveToCategory { get { return true; }}
		
		#endregion

	}


	[CustomEditor (typeof (SampleScene2DTemplate))]
	public class SampleScene2DTemplateEditor : TemplateEditor
	{}

}

#endif