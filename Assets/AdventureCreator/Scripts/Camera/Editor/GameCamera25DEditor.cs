#if UNITY_EDITOR

#if UNITY_2018_2_OR_NEWER
#define ALLOW_PHYSICAL_CAMERA
#endif

using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (GameCamera25D))]
	public class GameCamera25DEditor : Editor
	{

		private GameCamera25D _target;

		
		private BackgroundImage OnAutoCreateBackgroundImage ()
		{
			Undo.RecordObject (_target, "Create Background Image");
			BackgroundImage newBackgroundImage = SceneManager.AddPrefab ("SetGeometry", "BackgroundImage", true, false, true).GetComponent <BackgroundImage>();
			
			string cameraName = _target.gameObject.name;

			newBackgroundImage.gameObject.name = AdvGame.UniqueName (cameraName + ": Background");
			return newBackgroundImage;
		}


		public override void OnInspectorGUI ()
		{
			_target = (GameCamera25D) target;
			
			CustomGUILayout.Header ("Background image");
			CustomGUILayout.BeginVertical ();

			_target.backgroundImage = (BackgroundImage) CustomGUILayout.AutoCreateField ("Background:", _target.backgroundImage, OnAutoCreateBackgroundImage, "", "The BackgroundImage to display underneath all scene objects");

			if (_target.backgroundImage)
			{
				if (!Application.isPlaying && GUILayout.Button ("Set as active"))
				{
					Undo.RecordObject (_target, "Set active background");
					
					_target.SetActiveBackground ();
				}
			}

			if (MainCamera.AllowProjectionShifting (_target.GetComponent <Camera>()))
			{
				CustomGUILayout.EndVertical ();
				CustomGUILayout.Header ("Perspective offset");
				CustomGUILayout.BeginVertical ();
				_target.perspectiveOffset.x = CustomGUILayout.Slider ("Horizontal:", _target.perspectiveOffset.x, -0.05f, 0.05f, "", "The horizontal offset in perspective from the camera's centre");
				_target.perspectiveOffset.y = CustomGUILayout.Slider ("Vertical:", _target.perspectiveOffset.y, -0.05f, 0.05f, "", "The vertical offset in perspective from the camera's centre");
			}

			CustomGUILayout.EndVertical ();

			if (_target.isActiveEditor)
			{
				_target.UpdateCameraSnap ();
			}

			UnityVersionHandler.CustomSetDirty (_target);
		}

	}

}

#endif