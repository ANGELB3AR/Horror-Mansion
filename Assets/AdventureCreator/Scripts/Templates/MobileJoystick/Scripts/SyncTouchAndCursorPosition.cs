using UnityEngine;

namespace AC.Templates.MobileJoystick
{

	public class SyncTouchAndCursorPosition : MonoBehaviour
	{

		#region Variables

		private int cursorFingerID = -1;
		private Vector2 touchCursorPosition;

		#endregion


		#region UnityStandards

		private void Start ()
		{
			KickStarter.playerInput.InputMousePositionDelegate = InputMousePosition;
		}


		private void Update ()
		{
			UpdateCursor ();
		}

		#endregion


		#region PrivateFunctions

		private void UpdateCursor ()
		{
			if (cursorFingerID < 0)
			{
				for (int i = 0; i < Input.touchCount; i++)
				{
					Touch touch = Input.GetTouch (i);
					if (touch.phase == TouchPhase.Began)
					{
						touchCursorPosition = touch.position;
						cursorFingerID = touch.fingerId;
						return;
					}
				}
				return;
			}

			for (int i = 0; i < Input.touchCount; i++)
			{
				if (Input.GetTouch (i).fingerId == cursorFingerID && Input.GetTouch (i).phase != TouchPhase.Ended)
				{
					return;
				}
			}
			cursorFingerID = -1;
		}


		private Vector2 InputMousePosition (bool isLocked)
		{
			if (isLocked)
			{
				return KickStarter.playerInput.LockedCursorPosition;
			}

			if (cursorFingerID >= 0)
			{
				for (int i = 0; i < Input.touchCount; i++)
				{
					if (Input.GetTouch (i).fingerId == cursorFingerID)
					{
						touchCursorPosition = Input.GetTouch (i).position;
					}
				}
				return touchCursorPosition;
			}
			return Input.mousePosition;
		}

		#endregion

	}

}