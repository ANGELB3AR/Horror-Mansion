/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionRaycast.cs"
 * 
 *	This action performs a Raycast using Unity's physics system.
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
	public class ActionRaycast : ActionCheck
	{

		public DirectionMode directionMode = DirectionMode.FromOriginForward;
		public enum DirectionMode { FromOriginForward, ToSetDestination };
		public Transform destinationTransform;
		public int destinationTransformConstantID = 0;
		public int destinationTransformParameterID = -1;
		private Transform runtimeDestinationTransform;

		public Transform originTransform;
		public int originConstantID = 0;
		public int originParameterID = -1;
		protected Vector3 runtimeOrigin;
		protected Vector3 runtimeDirection;

		public Vector3 direction = new Vector3 (1f, 0f, 0f);
		public int directionParameterID = -1;

		public float distance = 10f;
		public int distanceParameterID = -1;
		private float runtimeDistance;

		public float debugDrawDuration = 1f;

		public float radius = 0f;
		public int radiusParameterID = -1;

		public LayerMask layerMask = new LayerMask ();

		public int detectedGameObjectParameterID = -1;
		public int detectedPositionParameterID = -1;
		protected ActionParameter detectedGameObjectParameter;
		protected ActionParameter detectedPositionParameter;


		public override ActionCategory Category { get { return ActionCategory.Physics; }}
		public override string Title { get { return "Raycast"; }}
		public override string Description { get { return "Performs a physics raycast"; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			ActionParameter originParameter = GetParameterWithID (parameters, originParameterID);
			if (originParameter != null && originParameter.parameterType == ParameterType.Vector3)
			{
				runtimeOrigin = originParameter.vector3Value;
				runtimeDirection = AssignVector3 (parameters, directionParameterID, direction).normalized;
			}
			else
			{
				Transform runtimeOriginTransform = AssignFile (parameters, originParameterID, originConstantID, originTransform);
				runtimeOrigin = runtimeOriginTransform ? runtimeOriginTransform.position : Vector3.zero;
				runtimeDirection = runtimeOriginTransform ? runtimeOriginTransform.forward : Vector3.forward;

				Marker marker = runtimeOriginTransform.GetComponent<Marker> ();
				if (marker && SceneSettings.IsUnity2D ())
				{
					runtimeDirection = marker.transform.up;
				}
			}

			distance = AssignFloat (parameters, distanceParameterID, distance);
			radius = AssignFloat (parameters, radiusParameterID, radius);

			detectedGameObjectParameter = GetParameterWithID (parameters, detectedGameObjectParameterID);
			if (detectedGameObjectParameter != null && detectedGameObjectParameter.parameterType != ParameterType.GameObject)
			{
				detectedGameObjectParameter = null;
			}

			detectedPositionParameter = GetParameterWithID (parameters, detectedPositionParameterID);
			if (detectedPositionParameter != null && detectedPositionParameter.parameterType != ParameterType.Vector3)
			{
				detectedPositionParameter = null;
			}

			if (directionMode == DirectionMode.ToSetDestination)
			{
				runtimeDestinationTransform = AssignFile (parameters, destinationTransformParameterID, destinationTransformConstantID, destinationTransform);
			}
		}
		
		
		public override bool CheckCondition ()
		{
			if (runtimeDestinationTransform)
			{
				runtimeDistance = Vector3.Distance (runtimeOrigin, runtimeDestinationTransform.position);
				runtimeDirection = (runtimeDestinationTransform.position - runtimeOrigin).normalized;
			}
			
			if (debugDrawDuration > 0f)
			{
				Debug.DrawRay (runtimeOrigin, runtimeDirection * runtimeDistance, Color.red, debugDrawDuration);
			}
			
			if (SceneSettings.IsUnity2D ())
			{
				RaycastHit2D hitInfo2D = UnityVersionHandler.Perform2DRaycast (runtimeOrigin, runtimeDirection, runtimeDistance, layerMask);
				if (hitInfo2D.collider)
				{
					if (detectedGameObjectParameter != null)
					{
						detectedGameObjectParameter.SetValue (hitInfo2D.collider.gameObject);
					}

					if (detectedPositionParameter != null)
					{
						detectedPositionParameter.SetValue (hitInfo2D.point);
					}
					return true;
				}
				return false;
			}

			RaycastHit hitInfo;
			if ((radius <= 0f && Physics.Raycast (runtimeOrigin, runtimeDirection, out hitInfo, runtimeDistance, layerMask)) ||
				(radius > 0f && Physics.SphereCast (runtimeOrigin, radius, runtimeDirection, out hitInfo, runtimeDistance, layerMask)))
			{
				if (detectedGameObjectParameter != null)
				{
					detectedGameObjectParameter.SetValue (hitInfo.collider.gameObject);
				}

				if (detectedPositionParameter != null)
				{
					detectedPositionParameter.SetValue (hitInfo.point);
				}
				return true;
			}
			return false;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			ComponentField ("Origin:", ref originTransform, ref originConstantID, parameters, ref originParameterID);

			if (originParameterID >= 0)
			{
				if (GetParameterWithID (parameters, originParameterID) != null && GetParameterWithID (parameters, originParameterID).parameterType == ParameterType.Vector3)
				{
					Vector3Field ("Direction:", ref direction, parameters, ref directionParameterID);
				}
			}

			directionMode = (DirectionMode) EditorGUILayout.EnumPopup ("Direction mode:", directionMode);
			if (directionMode == DirectionMode.FromOriginForward)
			{
				FloatField ("Distance:", ref distance, parameters, ref distanceParameterID);
			}
			else if (directionMode == DirectionMode.ToSetDestination)
			{
				ComponentField ("Destination:", ref destinationTransform, ref destinationTransformConstantID, parameters, ref destinationTransformParameterID);
			}

			if (!SceneSettings.IsUnity2D ())
			{
				FloatField ("Radius:", ref radius, parameters, ref radiusParameterID);
			}

			layerMask = AdvGame.LayerMaskField ("Layer mask:", layerMask);
			detectedGameObjectParameterID = ChooseParameterGUI ("Hit GameObject:", parameters, detectedGameObjectParameterID, ParameterType.GameObject);
			detectedPositionParameterID = ChooseParameterGUI ("Detection point:", parameters, detectedPositionParameterID, ParameterType.Vector3);

			debugDrawDuration = EditorGUILayout.FloatField ("Debug draw time (s):", debugDrawDuration);
		}


		protected override string GetSocketLabel (int index)
		{
			if (index == 0)
			{
				return "If object detected:";
			}
			return "If no object detected:";
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			originConstantID = AssignConstantID (originTransform, originConstantID, originParameterID);
		}
		
		
		public override string SetLabel ()
		{
			if (originTransform)
			{
				return "From " + originTransform.gameObject.name;
			}
			return string.Empty;
		}


		public override bool ReferencesObjectOrID (GameObject gameObject, int id)
		{
			if (originParameterID < 0)
			{
				if (originTransform && originTransform.gameObject == gameObject) return true;
				if (originConstantID == id && id != 0) return true;
			}
			return base.ReferencesObjectOrID (gameObject, id);
		}
		
		#endif

	}

}