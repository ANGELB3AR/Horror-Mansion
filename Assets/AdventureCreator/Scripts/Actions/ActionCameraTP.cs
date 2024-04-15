/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionCameraTP.cs"
 * 
 *	This action can rotate a GameCameraThirdPerson to a set rotation.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionCameraTP : Action
	{

		public float newPitchAngle = 0f;
		public float newSpinAngle = 0f;
		public int newRotationParameterID = -1;
		private Vector3 newRotation;

		public GameCameraThirdPerson thirdPersonCamera = null;
		public int thirdPersonCameraConstantID = 0;
		public int thirdPersonCameraParameterID = -1;

		public enum NewTPCamMethod { SetLookAtOverride, ClearLookAtOverride, MoveToRotation, SnapToMainCamera };
		public NewTPCamMethod method = NewTPCamMethod.MoveToRotation;
		public Transform lookAtOverride = null;

		public int lookAtOverrideConstantID = 0;
		public int lookAtOverrideParameterID = -1;

		public float transitionTime = 0f;
		public int transitionTimeParameterID = -1;
		public bool isRelativeToTarget = false;


		public override ActionCategory Category { get { return ActionCategory.Camera; } }
		public override string Title { get { return "Rotate third-person"; } }
		public override string Description { get { return "Manipulates the new third-person camera"; } }


		override public void AssignValues (List<ActionParameter> parameters)
		{
			thirdPersonCamera = AssignFile (parameters, thirdPersonCameraParameterID, thirdPersonCameraConstantID, thirdPersonCamera);
			lookAtOverride = AssignFile (parameters, lookAtOverrideParameterID, lookAtOverrideConstantID, lookAtOverride);
			transitionTime = AssignFloat (parameters, transitionTimeParameterID, transitionTime);

			newRotation = new Vector3 (newSpinAngle, newPitchAngle, 0f);
			newRotation = AssignVector3 (parameters, newRotationParameterID, newRotation);
		}


		override public float Run ()
		{
			if (thirdPersonCamera)
			{
				if (!isRunning)
				{
					switch (method)
					{
						case NewTPCamMethod.SetLookAtOverride:
							thirdPersonCamera.SetLookAtOverride (lookAtOverride, transitionTime);
							break;

						case NewTPCamMethod.ClearLookAtOverride:
							thirdPersonCamera.ClearLookAtOverride (transitionTime);
							break;

						case NewTPCamMethod.MoveToRotation:
							thirdPersonCamera.BeginAutoMove (transitionTime, newRotation, isRelativeToTarget);
							if (transitionTime > 0f && willWait)
							{
								isRunning = true;
								return defaultPauseTime;
							}
							break;

						case NewTPCamMethod.SnapToMainCamera:
							thirdPersonCamera.SnapToDirection (Camera.main.transform.forward, Camera.main.transform.right);
							break;
					}
				}
				else
				{
					if (thirdPersonCamera.IsAutoMoving ())
					{
						return defaultPauseTime;
					}
					isRunning = false;
				}
			}
			return 0f;
		}


		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			ComponentField ("Third-person camera:", ref thirdPersonCamera, ref thirdPersonCameraConstantID, parameters, ref thirdPersonCameraParameterID);

			method = (NewTPCamMethod) EditorGUILayout.EnumPopup ("Method:", method);

			if (method == NewTPCamMethod.MoveToRotation)
			{
				SliderVectorField ("New spin:", ref newSpinAngle, -180f, 180f, parameters, ref newRotationParameterID, "New rotation:");
				if (newRotationParameterID < 0)
				{
					newPitchAngle = EditorGUILayout.Slider ("New pitch:", newPitchAngle, -80f, 80f);
				}

				isRelativeToTarget = EditorGUILayout.Toggle ("Spin relative to target?", isRelativeToTarget);

				SliderField ("Speed:", ref transitionTime, 0f, 10f, parameters, ref transitionTimeParameterID);
				if (transitionTimeParameterID < 0 || transitionTime > 0f)
				{
					willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
				}
			}
			else if (method == NewTPCamMethod.SetLookAtOverride || method == NewTPCamMethod.ClearLookAtOverride)
			{
				if (method == NewTPCamMethod.SetLookAtOverride)
				{
					ComponentField ("Look-at Transform:", ref lookAtOverride, ref lookAtOverrideConstantID, parameters, ref lookAtOverrideParameterID);
				}

				SliderField ("Speed:", ref transitionTime, 0f, 10f, parameters, ref transitionTimeParameterID);
			}
		}


		override public void AssignConstantIDs (bool saveScriptsToo = false, bool fromAssetFile = false)
		{
			lookAtOverrideConstantID = AssignConstantID (lookAtOverride, lookAtOverrideConstantID, lookAtOverrideParameterID);
			thirdPersonCameraConstantID = AssignConstantID<GameCameraThirdPerson> (thirdPersonCamera, thirdPersonCameraConstantID, thirdPersonCameraParameterID);
		}


		override public string SetLabel ()
		{
			return method.ToString ();
		}

		#endif

	}

}