#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (GameCamera2D))]
	public class GameCamera2DEditor : Editor
	{

		public override void OnInspectorGUI ()
		{
			GameCamera2D _target = (GameCamera2D) target;

			_target.ShowCursorInfluenceGUI ();

			CustomGUILayout.Header ("Horizontal movement");
			CustomGUILayout.BeginVertical ();
		
			_target.lockHorizontal = CustomGUILayout.Toggle ("Lock?", _target.lockHorizontal, string.Empty, "If True, then horizontal panning is prevented");
			if (!_target.GetComponent <Camera>().orthographic || !_target.lockHorizontal)
			{
				_target.afterOffset.x = CustomGUILayout.FloatField ("Offset:", _target.afterOffset.x, string.Empty, "The horizontal panning offset");
			}
		
			if (!_target.lockHorizontal)
			{
				_target.freedom.x = CustomGUILayout.FloatField ("Track freedom:",_target.freedom.x, string.Empty, "The amount of freedom when tracking a target. Higher values will result in looser tracking");
				_target.directionInfluence.x = CustomGUILayout.FloatField ("Target direction factor:", _target.directionInfluence.x, string.Empty, "The influence that the target's facing direction has on the tracking position");
				_target.limitHorizontal = CustomGUILayout.Toggle ("Constrain?", _target.limitHorizontal, string.Empty, "If True, then horizontal panning will be limited to minimum and maximum values");

				if (_target.limitHorizontal)
				{
					if (_target.GetComponent<Camera> ().orthographic)
					{
						_target.backgroundConstraint = (Renderer) CustomGUILayout.ObjectField<Renderer> ("Background constraint:", _target.backgroundConstraint, true, string.Empty, "If set, this sprite's boundary will be used to set the constraint limits");
						if (_target.backgroundConstraint)
						{
							_target.autoScaleToFitBackgroundConstraint = CustomGUILayout.Toggle ("Auto-set Orthographic size to fit?", _target.autoScaleToFitBackgroundConstraint, string.Empty, "If True, then the Camera's Orthographic Size value will be reduced if the background is not large enough to fill the screen.");
						}
					}

					if (!_target.GetComponent<Camera>().orthographic || _target.backgroundConstraint == null)
					{
						CustomGUILayout.BeginVertical ();
						_target.constrainHorizontal[0] = CustomGUILayout.FloatField ("Minimum constraint:", _target.constrainHorizontal[0], string.Empty, "The lower horizontal panning limit");
						_target.constrainHorizontal[1] = CustomGUILayout.FloatField ("Maximum constraint:", _target.constrainHorizontal[1], string.Empty, "The upper horizontal panning limit");
						CustomGUILayout.EndVertical ();
					}
				}
			}
			CustomGUILayout.EndVertical ();
			
			CustomGUILayout.Header ("Vertical movement");
			CustomGUILayout.BeginVertical ();
		
			_target.lockVertical = CustomGUILayout.Toggle ("Lock?", _target.lockVertical, "", "If True, then vertical panning is prevented");
			if (!_target.GetComponent <Camera>().orthographic || !_target.lockVertical)
			{
				_target.afterOffset.y = CustomGUILayout.FloatField ("Offset:", _target.afterOffset.y, "", "The vertical panning offset");
			}

			if (!_target.lockVertical)
			{
				_target.freedom.y = CustomGUILayout.FloatField ("Track freedom:",_target.freedom.y, "", "The amount of freedom when tracking a target. Higher values will result in looser tracking");
				_target.directionInfluence.y = CustomGUILayout.FloatField ("Target direction factor:", _target.directionInfluence.y, "", "The influence that the target's facing direction has on the tracking position");
				_target.limitVertical = CustomGUILayout.Toggle ("Constrain?", _target.limitVertical, "", "If True, then vertical panning will be limited to minimum and maximum values");

				if (_target.limitVertical)
				{
					if (_target.GetComponent<Camera> ().orthographic)
					{
						_target.backgroundConstraint = (Renderer) CustomGUILayout.ObjectField<Renderer> ("Background constraint:", _target.backgroundConstraint, true, string.Empty, "If set, this sprite's boundary will be used to set the constraint limits");
						if (_target.backgroundConstraint)
						{
							_target.autoScaleToFitBackgroundConstraint = CustomGUILayout.Toggle ("Auto-set Orthographic size to fit?", _target.autoScaleToFitBackgroundConstraint, string.Empty, "If True, then the Camera's Orthographic Size value will be reduced if the background is not large enough to fill the screen.");
						}
					}

					if (!_target.GetComponent<Camera> ().orthographic || _target.backgroundConstraint == null)
					{
						CustomGUILayout.BeginVertical ();
						_target.constrainVertical[0] = CustomGUILayout.FloatField ("Minimum constraint:", _target.constrainVertical[0], "", "The lower vertical panning limit");
						_target.constrainVertical[1] = CustomGUILayout.FloatField ("Maximum constraint:", _target.constrainVertical[1], "", "The upper vertical panning limit");
						CustomGUILayout.EndVertical ();
					}
				}
			}
			CustomGUILayout.EndVertical ();
			
			if (!_target.lockHorizontal || !_target.lockVertical)
			{
				CustomGUILayout.BeginVertical ();
				EditorGUILayout.LabelField ("Target object to control camera movement", EditorStyles.boldLabel);
				
				_target.targetIsPlayer = CustomGUILayout.Toggle ("Target is Player?", _target.targetIsPlayer, "", "If True, the camera will follow the active Player");
				
				if (!_target.targetIsPlayer)
				{
					_target.target = (Transform) CustomGUILayout.ObjectField <Transform> ("Target:", _target.target, true, "", "The object for the camera to follow");
				}
				
				_target.dampSpeed = CustomGUILayout.Slider ("Follow speed", _target.dampSpeed, 0.01f, 1f, "", "The follow speed when tracking a target");

				_target.doSnapping = CustomGUILayout.Toggle ("Snap to grid?", _target.doSnapping, "", "If True, the camera will only move in steps, as if snapping to a grid");
				if (_target.doSnapping)
				{
					_target.unitSnap = CustomGUILayout.FloatField ("Snap unit size:", _target.unitSnap, "", "The step size when snapping");
				}

				_target.updateWhilePaused = CustomGUILayout.Toggle ("Update while paused?", _target.updateWhilePaused, "", "If True, the camera will not be frozen while the game is paused");

				CustomGUILayout.EndVertical ();
			}
			
			if (!_target.IsCorrectRotation ())
			{
				if (GUILayout.Button ("Set correct rotation"))
				{
					Undo.RecordObject (_target, "Clear " + _target.name + " rotation");
					_target.SetCorrectRotation ();
				}
			}

			if (!Application.isPlaying)
			{
				_target.GetComponent <Camera>().ResetProjectionMatrix ();
				if (!_target.GetComponent <Camera>().orthographic)
				{
					_target.SnapToOffset ();
				}
			}

			UnityVersionHandler.CustomSetDirty (_target);
		}
	}

}

#endif