#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace AC.Templates.FirstPersonPlayer
{

	public class SamplePlayerFPTemplate : Template
	{

		#region Variables

		[SerializeField] private Player playerPrefab = null;
		[SerializeField] private AnimatorController cameraAnimator = null;

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

			if (cameraAnimator == null)
			{
				errorText = "No camera Animator assigned";
				return false;
			}

			return true;
		}


		public override bool CanSuggest (NGWData data)
		{
			return data.CameraPerspective == CameraPerspective.ThreeD && data.MovementMethod == MovementMethod.FirstPerson;
		}

		#endregion


		#region ProtectedFunctions

		protected override void MakeChanges (string installPath, bool canDeleteOldAssets, System.Action onComplete, System.Action<string> onFail)
		{
			// Animator
			AnimatorController newAnimator = CopyAsset<AnimatorController> (installPath, cameraAnimator, ".controller");
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

			newPlayerPrefab.GetComponentInChildren<Animator> ().runtimeAnimatorController = newAnimator;

			// Settings
			Resource.References.settingsManager.lockCursorOnStart = true;
			Resource.References.settingsManager.SetDefaultPlayer (newPlayerPrefab);
			Resource.References.settingsManager.freeAimSmoothSpeed = 15f;
			Resource.References.settingsManager.dragWalkThreshold = 10f;
			Resource.References.settingsManager.firstPersonMovementSmoothing = true;

			EditorUtility.SetDirty (Resource.References.settingsManager);

			// Inputs
			ACInstaller.InputAxis cursorHorizontalAxis = new ACInstaller.InputAxis ()
			{
				name = "CursorHorizontal",
				type = ACInstaller.AxisType.MouseMovement,
				axis = 1,
				gravity = 1f,
				dead = 0f,
				sensitivity = 0.1f,
				snap = false,
			};

			ACInstaller.InputAxis cursorVerticalAxis = new ACInstaller.InputAxis ()
			{
				name = "CursorVertical",
				type = ACInstaller.AxisType.MouseMovement,
				axis = 2,
				gravity = 1f,
				dead = 0f,
				sensitivity = 0.1f,
				snap = false,
			};

			ACInstaller.InputAxis crouchInputAxis = new ACInstaller.InputAxis ()
			{
				name = "Crouch",
				positiveButton = "c",
			};

			ACInstaller.AddAxis (cursorHorizontalAxis);
			ACInstaller.AddAxis (cursorVerticalAxis);
			ACInstaller.AddAxis (crouchInputAxis);

			onComplete.Invoke ();
		}

		#endregion


		#region GetSet

		public override string Label { get { return "First-person Player"; }}
		public override string PreviewText { get { return "A ready-made First-person player character, featuring smooth movement, animation and crouching."; }}
		public override Type[] AffectedManagerTypes { get { return new Type[] { typeof (SettingsManager) }; }}
		public override string FolderName { get { return "Sample Player FP"; }}
		public override bool RequiresInstallPath { get { return true; }}
		public override bool SelectedByDefault { get { return true; }}
		public override TemplateCategory Category { get { return TemplateCategory.SamplePlayer; }}
		public override bool IsExclusiveToCategory { get { return true; }}
		
		#endregion

	}

}

#endif