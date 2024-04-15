#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AC.Templates.MobileJoystick
{

	public class MobileJoystickTemplate : Template
	{

		#region Variables

		[SerializeField] private JoystickUI joystickUIPrefab = null;
		private const string EventName = "Spawn Mobile Joystick";

		#endregion


		#region PublicFunctions

		public override bool CanInstall (ref string errorText)
		{
			if (Resource.References.settingsManager == null)
			{
				errorText = "No Settings Manager assigned";
				return false;
			}

			if (joystickUIPrefab == null)
			{
				errorText = "No JoystickUI prefab assigned";
				return false;
			}

			return true;
		}


		public override bool CanSuggest (NGWData data)
		{
			return data.InputMethod == InputMethod.TouchScreen && (data.MovementMethod == MovementMethod.Direct || data.MovementMethod == MovementMethod.FirstPerson);
		}

		#endregion


		#region ProtectedFunctions

		protected override void MakeChanges (string installPath, bool canDeleteOldAssets, System.Action onComplete, System.Action<string> onFail)
		{
			Undo.RecordObjects (new UnityEngine.Object[] { Resource.References.settingsManager }, "");

			Resource.References.settingsManager.directTouchScreen = DirectTouchScreen.CustomInput;
			Resource.References.settingsManager.firstPersonTouchScreen = FirstPersonTouchScreen.CustomInput;
			Resource.References.settingsManager.drawDragLine = false;
			Resource.References.settingsManager.magnitudeAffectsDirect = true;

			JoystickUI newJoystickPrefab = CopyAsset<JoystickUI> (installPath, joystickUIPrefab, ".prefab");
			if (newJoystickPrefab == null)
			{
				onFail.Invoke ("Prefab copy failed.");
				return;
			}
			ActionListAsset spawnMobileJoystickActionList = CreateActionList ("Spawn JoystickUI", installPath, newJoystickPrefab);

			EventSceneSwitch newEvent = new EventSceneSwitch (Resource.References.settingsManager. GetNextAvailableEventID (), EventName, spawnMobileJoystickActionList, new int[] { 0 }, EventSceneSwitch.BeforeAfter.After, EventSceneSwitch.DueToLoadingSave.Either);
			Resource.References.settingsManager.events.Add (newEvent);

			EditorUtility.SetDirty (Resource.References.settingsManager);

			onComplete.Invoke ();
		}


		#endregion


		#region PrivateFunctions
		
		private ActionListAsset CreateActionList (string assetName, string installPath, JoystickUI joystickUI)
		{
			List<Action> actions = new List<Action>
			{
				ActionInstantiate.CreateNew_Add (joystickUI.gameObject),
			};

			ActionListAsset newAsset = ActionListAsset.CreateFromActions (assetName, installPath, actions, ActionListType.RunInBackground);
			return newAsset;
		}

		#endregion


		#region GetSet

		public override string Label { get { return "Mobile joystick"; }}
		public override string PreviewText { get { return "Adds an on-screen joystick that can be used to control the Player and camera on mobile devices."; }}
		public override Type[] AffectedManagerTypes { get { return new Type[] { typeof (SettingsManager) }; }}
		public override bool RequiresInstallPath { get { return true; }}
		public override string FolderName { get { return "MobileJoystick"; }}
		public override TemplateCategory Category { get { return TemplateCategory.Misc; }}

		#endregion

	}


	[CustomEditor (typeof (MobileJoystickTemplate))]
	public class MobileJoystickTemplateEditor : TemplateEditor
	{}

}

#endif