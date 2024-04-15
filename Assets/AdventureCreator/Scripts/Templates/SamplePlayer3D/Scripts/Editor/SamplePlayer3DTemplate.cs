﻿#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace AC.Templates.SamplePlayer3D
{

	public class SamplePlayer3DTemplate : Template
	{

		#region Variables

		[SerializeField] private Player playerPrefab = null;
		[SerializeField] private AnimatorController animator = null;

		#endregion


		#region PublicFunctions

		public override bool CanInstall (ref string errorText)
		{
			if (Resource.References.settingsManager == null)
			{
				errorText = "No Settings Manager assigned";
				return false;
			}

			if (playerPrefab == null)
			{
				errorText = "No Player prefab assigned";
				return false;
			}

			return true;
		}


		public override bool CanSuggest (NGWData data)
		{
			return data.CameraPerspective != CameraPerspective.TwoD && (data.MovementMethod == MovementMethod.PointAndClick || data.MovementMethod == MovementMethod.Direct);
		}

		#endregion


		#region ProtectedFunctions

		protected override void MakeChanges (string installPath, bool canDeleteOldAssets, System.Action onComplete, System.Action<string> onFail)
		{
			// Animator
			AnimatorController newAnimator = CopyAsset<AnimatorController> (installPath, animator, ".controller");
			if (newAnimator == null)
			{
				onFail.Invoke ("Controller copy failed.");
				return;
			}

			// Player
			Player newPlayerPrefab = CopyAsset<Player> (installPath, playerPrefab, ".prefab");
			if (newPlayerPrefab == null)
			{
				onFail.Invoke ("Prefab copy failed.");
				return;
			}
			newPlayerPrefab.GetComponent<Animator> ().runtimeAnimatorController = newAnimator;

			// Settings
			Undo.RecordObjects (new UnityEngine.Object[] { Resource.References.settingsManager }, "");
			Resource.References.settingsManager.SetDefaultPlayer (newPlayerPrefab);
			EditorUtility.SetDirty (Resource.References.settingsManager);
			
			onComplete?.Invoke ();
		}

		#endregion


		#region GetSet

		public override string Label { get { return "3D sample Player"; }}
		public override string PreviewText { get { return "A sample Player character for 3D games."; }}
		public override Type[] AffectedManagerTypes { get { return new Type[] { typeof (SettingsManager) }; }}
		public override string FolderName { get { return "SamplePlayer3D"; }}
		public override bool RequiresInstallPath { get { return true; }}
		public override bool SelectedByDefault { get { return true; }}
		public override TemplateCategory Category { get { return TemplateCategory.SamplePlayer; }}
		public override bool IsExclusiveToCategory { get { return true; }}
		
		#endregion

	}


	[CustomEditor (typeof (SamplePlayer3DTemplate))]
	public class SamplePlayer3DTemplateEditor : TemplateEditor
	{}

}

#endif