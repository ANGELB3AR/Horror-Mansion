using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC.Templates.MobileJoystick
{

	[Serializable]
	public class Button : BaseUI
	{

		#region Variables

		[SerializeField] private Sound soundOnPress = null;
		[SerializeField] private string inputName;
		[SerializeField] private bool isContinuous = false;

		#endregion


		#region Constructors

		public Button ()
		{}

		#endregion


		#region PublicFunctions

		public override void Update (JoystickUI joystickUI)
		{
			if (Boundary == null || !Boundary.gameObject.activeSelf)
			{
				fingerID = -1;
				return;
			}

			if (!string.IsNullOrEmpty (inputName))
			{
				bool isInGameplay = KickStarter.stateHandler.IsInGameplay ();

				if (fingerID == -1)
				{
					if (isInGameplay)
					{
						fingerID = GetBeginningTouchIndex (joystickUI.Canvas);
						if (fingerID >= 0)
						{
							// Start
							if (soundOnPress) soundOnPress.Play ();
							KickStarter.playerInput.SimulateInputButton (inputName);
						}
					}
				}
				else
				{
					if (isInGameplay && IsStillTouching ())
					{
						// Update
						if (isContinuous)
						{
							KickStarter.playerInput.SimulateInputButton (inputName);
						}
					}
					else
					{
						fingerID = -1;
						// End
					}
				}
			}

			base.Update (joystickUI);
		}

#if UNITY_EDITOR

		public override void ShowGUI (string label)
		{
			EditorGUILayout.LabelField (label, EditorStyles.largeLabel);
			EditorGUILayout.Space ();

			inputName = CustomGUILayout.TextField ("Input name:", inputName);
			isContinuous = CustomGUILayout.Toggle ("Is continuous?", isContinuous);
			soundOnPress = (Sound) CustomGUILayout.ObjectField<Sound> ("Sound (optional):", soundOnPress, true);

			base.ShowGUI (label);
		}

#endif

		#endregion

	}

}