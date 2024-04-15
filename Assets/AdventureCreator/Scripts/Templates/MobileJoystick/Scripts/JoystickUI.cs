using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC.Templates.MobileJoystick
{

	[DefaultExecutionOrder (-10)]
	public class JoystickUI : MonoBehaviour
	{

		#region Variables

		private Canvas canvas = null;
		[SerializeField] private Joystick playerJoystick = new Joystick ();
		[SerializeField] private bool autoSetRunState = true;
		private const string runAxis = "Run";
		[SerializeField] private Joystick cameraJoystick = new Joystick ();
		[SerializeField] private Button[] buttons = new Button[0];
		private GameCameraThirdPerson gameCameraThirdPerson;

		#endregion


		#region UnityStandards

		private void Start ()
		{
			canvas = GetComponent<Canvas> ();
		}


		private void OnEnable ()
		{
			if (KickStarter.playerInput)
			{
				KickStarter.playerInput.InputGetAxisDelegate = InputGetAxis;
				KickStarter.playerInput.InputGetButtonDelegate = InputGetButton;
				KickStarter.playerInput.InputGetFreeAimDelegate = InputGetFreeAim;

				KickStarter.playerInput.InputGetMouseButtonDownDelegate = InputGetMouseButtonDown;
				KickStarter.playerInput.InputGetTouchPhaseDelegate = InputGetTouchPhase;
			}

			EventManager.OnSwitchCamera += OnSwitchCamera;
		}


		private void OnDisable ()
		{
			if (KickStarter.playerInput)
			{
				KickStarter.playerInput.InputGetAxisDelegate = null;
				KickStarter.playerInput.InputGetButtonDelegate = null;
				KickStarter.playerInput.InputGetFreeAimDelegate = null;

				KickStarter.playerInput.InputGetMouseButtonDownDelegate = null;
				KickStarter.playerInput.InputGetTouchPhaseDelegate = null;
			}

			EventManager.OnSwitchCamera -= OnSwitchCamera;
		}


		private void Update ()
		{
			if (canvas)
			{
				playerJoystick.Update (this);
				cameraJoystick.Update (this);

				for (int i = 0; i < buttons.Length; i++)
				{
					buttons[i].Update (this);
				}
			}
		}


		private void LateUpdate ()
		{
			if (canvas)
			{
				playerJoystick.LateUpdate ();
				cameraJoystick.LateUpdate ();
			}
		}

		#endregion


		#region PublicFunctions

		public List<int> GetActiveFingerIDs ()
		{
			List<int> fingerIDs = new List<int> ();

			if (playerJoystick.IsUsed) fingerIDs.Add (playerJoystick.FingerID);
			if (cameraJoystick.IsUsed) fingerIDs.Add (cameraJoystick.FingerID);

			for (int i = 0; i < buttons.Length; i++)
			{
				if (buttons[i].IsUsed) fingerIDs.Add (buttons[i].FingerID);
			}

			return fingerIDs;
		}


#if UNITY_EDITOR

		public void ShowGUI ()
		{
			CustomGUILayout.BeginVertical ();
			playerJoystick.ShowGUI ("Player joystick");
			CustomGUILayout.EndVertical ();
			autoSetRunState = EditorGUILayout.Toggle ("Auto-set 'run' state?", autoSetRunState);
			EditorGUILayout.Space ();

			CustomGUILayout.BeginVertical ();
			cameraJoystick.ShowGUI ("Camera joystick");
			CustomGUILayout.EndVertical ();

			
			EditorGUILayout.Space ();
			CustomGUILayout.BeginVertical ();
			int numButtons = buttons.Length;
			numButtons = EditorGUILayout.DelayedIntField ("# of buttons:", numButtons);
			if (numButtons != buttons.Length) ResizeButtonArray (numButtons);
			EditorGUILayout.EndVertical ();

			for (int i = 0; i < buttons.Length; i++)
			{
				EditorGUILayout.Space ();
				CustomGUILayout.BeginVertical ();
				buttons[i].ShowGUI ("Button #" + i);
				CustomGUILayout.EndVertical ();
			}
		}

#endif

		#endregion


		#region CustomEvents

		private void OnSwitchCamera (_Camera fromCamera, _Camera toCamera, float transitionTime)
		{
			gameCameraThirdPerson = toCamera as GameCameraThirdPerson;
			if (gameCameraThirdPerson)
			{
				gameCameraThirdPerson.isDragControlled = false;
				gameCameraThirdPerson.spinAxis = cameraJoystick.horizontalAxis;
				gameCameraThirdPerson.pitchAxis = cameraJoystick.verticalAxis;
			}
		}

		#endregion


		#region PrivateFunctions

		private void ResizeButtonArray (int newLength)
		{
			List<Button> buttonList = new List<Button> ();
			for (int i = 0; i < Mathf.Min (newLength, buttons.Length); i++)
			{
				if (i < buttons.Length)
				{
					buttonList.Add (buttons[i]);
				}
				else
				{
					buttonList.Add (new Button ());
				}
			}

			while (buttonList.Count < newLength)
			{
				buttonList.Add (new Button ());
			}

			buttons = buttonList.ToArray ();
		}


		private bool InputGetButton (string axis)
		{
			if (axis == runAxis && autoSetRunState && !InvInstance.IsValid (KickStarter.runtimeInventory.SelectedInstance) && playerJoystick.IsUsed)
			{
				return (playerJoystick.GetDragVector ().magnitude / ACScreen.LongestDimension) > KickStarter.settingsManager.dragRunThreshold;
			}

			try { return Input.GetButton (axis); }
			catch { return false; }
		}


		private float InputGetAxis (string axis)
		{
			if (axis == cameraJoystick.horizontalAxis)
			{
				return cameraJoystick.GetDragVector ().x * cameraJoystick.EffectScaler;
			}
			else if (axis == cameraJoystick.verticalAxis)
			{
				return cameraJoystick.GetDragVector ().y * cameraJoystick.EffectScaler;
			}
			else if (axis == playerJoystick.horizontalAxis)
			{
				return playerJoystick.GetDragVector ().x * playerJoystick.EffectScaler;
			}
			else if (axis == playerJoystick.verticalAxis)
			{
				return playerJoystick.GetDragVector ().y * playerJoystick.EffectScaler;
			}

			try { return Input.GetAxis (axis); }
			catch { return 0f; }
		}


		private Vector2 InputGetFreeAim (bool cursorIsLocked)
		{
			return 0.01f * cameraJoystick.EffectScaler * cameraJoystick.GetDragVector ();
		}


		private bool InputGetMouseButtonDown (int button)
		{
			if (playerJoystick.IsUsed || cameraJoystick.IsUsed)
			{
				return false;
			}
			return Input.GetMouseButtonDown (button);
		}


		private TouchPhase InputGetTouchPhase (int index)
		{
			if (playerJoystick.IsUsed || cameraJoystick.IsUsed)
			{
				return TouchPhase.Canceled;
			}
			return Input.GetTouch (index).phase;
		}

		#endregion


		#region GetSet

		public Joystick PlayerJoystick { get { return playerJoystick; } }
		public Joystick CameraJoystick { get { return cameraJoystick; } }
		public Canvas Canvas { get { return canvas; }}

		#endregion

	}

}