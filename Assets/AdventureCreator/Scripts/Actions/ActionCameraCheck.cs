/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionCameraCheck.cs"
 * 
 *	This action checks the which GameCamera is currently active.
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
	public class ActionCameraCheck : ActionCheck
	{
		
		public int parameterID = -1;
		public int constantID = 0;
		public _Camera cameraToCheck;
		protected _Camera runtimeCameraToCheck;

		public int detectedCameraParameterID = -1;
		protected ActionParameter detectedCameraParameter;


		public override ActionCategory Category { get { return ActionCategory.Camera; }}
		public override string Title { get { return "Check active"; }}
		public override string Description { get { return "Checks the active GameCamera."; }}
		
		
		public override void AssignValues (List<ActionParameter> parameters)
		{
			runtimeCameraToCheck = AssignFile<_Camera> (parameters, parameterID, constantID, cameraToCheck);

			detectedCameraParameter = GetParameterWithID (parameters, detectedCameraParameterID);
			if (detectedCameraParameter != null && detectedCameraParameter.parameterType != ParameterType.GameObject)
			{
				detectedCameraParameter = null;
			}
		}


		public override bool CheckCondition ()
		{
			if (runtimeCameraToCheck && KickStarter.mainCamera)
			{
				if (detectedCameraParameter != null && KickStarter.mainCamera.attachedCamera)
				{
					detectedCameraParameter.SetValue (KickStarter.mainCamera.attachedCamera.gameObject);
				}

				return KickStarter.mainCamera.attachedCamera == runtimeCameraToCheck;
			}
			return false;
		}
		
			
		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			ComponentField ("Camera to check:", ref cameraToCheck, ref constantID, parameters, ref parameterID);
			detectedCameraParameterID = ChooseParameterGUI ("Active camera assignment:", parameters, detectedCameraParameterID, ParameterType.GameObject);
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			constantID = AssignConstantID (cameraToCheck, constantID, parameterID);
		}

		
		public override string SetLabel ()
		{
			if (cameraToCheck)
			{
				return cameraToCheck.gameObject.name;
			}
			return string.Empty;
		}


		public override bool ReferencesObjectOrID (GameObject gameObject, int id)
		{
			if (parameterID < 0)
			{
				if (cameraToCheck && cameraToCheck.gameObject == gameObject) return true;
				return (constantID == id && id != 0);
			}
			return base.ReferencesObjectOrID (gameObject, id);
		}
		
		#endif


		/**
		 * <summary>Creates a new instance of the 'Camera: Check active' Action</summary>
		 * <param name = "checkToAffect">The camera to check</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionCameraCheck CreateNew (_Camera cameraToCheck)
		{
			ActionCameraCheck newAction = CreateNew<ActionCameraCheck> ();
			newAction.cameraToCheck = cameraToCheck;
			newAction.TryAssignConstantID (newAction.cameraToCheck, ref newAction.constantID);
			return newAction;
		}

	}
	
}