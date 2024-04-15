using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC.Templates.MobileJoystick
{

	[Serializable]
	public class Joystick : BaseUI
	{

		#region Variables

		[SerializeField] private RectTransform centre = null;
		[SerializeField] private bool requiresContinuousDragging;
		public float effectScaler = 1f;
		public string horizontalAxis = "Horizontal";
		public string verticalAxis = "Vertical";
		private Vector2 startPosition, lastFramePosition;
		private const float centreLerpSpeed = 12f;
		private Vector2 touchDrag;

		#endregion


		#region PublicFunctions

		public override void Update (JoystickUI joystickUI)
		{
			if (Boundary == null)
			{
				return;
			}

			Vector2 newCentrePosition = new Vector2 (0f, Boundary.sizeDelta.y * 0.5f);
			bool isInGameplay = KickStarter.stateHandler.IsInGameplay ();

			if (fingerID == -1)
			{
				if (isInGameplay)
				{
					int newFingerID = GetBeginningTouchIndex (joystickUI.Canvas);
					if (newFingerID >= 0 && !joystickUI.GetActiveFingerIDs ().Contains (newFingerID))
					{
						fingerID = newFingerID;
						lastFramePosition = startPosition = GetTouchPosition ();
						OnJoystickStartMoving?.Invoke (this);
					}
				}
			}
			else
			{
				if (isInGameplay && IsStillTouching ())
				{
					Vector2 centreOffset = requiresContinuousDragging
											? GetTouchDrag ()
											: GetTouchPosition () - startPosition;

					centreOffset *= 2f;

					if (centreOffset.magnitude > Boundary.sizeDelta.x)
					{
						centreOffset = centreOffset.normalized * Boundary.sizeDelta.x;
					}

					newCentrePosition = new Vector2 (centreOffset.x * 0.5f, (Boundary.sizeDelta.y + centreOffset.y) * 0.5f);
				}
				else
				{
					fingerID = -1;
					OnJoystickStopMoving?.Invoke (this);
				}
			}

			UpdateCentrePosition (newCentrePosition);
			base.Update (joystickUI);
		}


		public void LateUpdate ()
		{
			if (fingerID < 0 || Boundary == null)
			{
				return;
			}

			Vector2 touchPosition = GetTouchPosition ();
			touchDrag = GetTouchDrag ();
			lastFramePosition = touchPosition;
		}


		public Vector2 GetDragVector ()
		{
			if (fingerID < 0)
			{
				return Vector2.zero;
			}

			if (requiresContinuousDragging)
			{
				return new Vector2 (touchDrag.x / (Boundary.sizeDelta.x * 0.5f) * Time.deltaTime, touchDrag.y / (Boundary.sizeDelta.y * 0.5f) * Time.deltaTime) * 3000f;
			}

			Vector2 touchPosition = GetTouchPosition ();
			return new Vector2 ((touchPosition.x - startPosition.x) / (Boundary.sizeDelta.x * 0.5f), (touchPosition.y - startPosition.y) / (Boundary.sizeDelta.y * 0.5f));
		}


#if UNITY_EDITOR

		public override void ShowGUI (string label)
		{
			EditorGUILayout.LabelField (label, EditorStyles.largeLabel);

			EditorGUILayout.Space ();

			horizontalAxis = EditorGUILayout.TextField ("Horizontal axis:", horizontalAxis);
			verticalAxis = EditorGUILayout.TextField ("Vertical axis:", verticalAxis);
			centre = (RectTransform) CustomGUILayout.ObjectField<RectTransform> ("Centre (optional):", centre, true);
			requiresContinuousDragging = CustomGUILayout.Toggle ("Requires continuous dragging?", requiresContinuousDragging);
			effectScaler = CustomGUILayout.FloatField ("Effect scaler:", effectScaler);

			base.ShowGUI (label);
		}

#endif

		#endregion


		#region PrivateFunctions

		private Vector2 GetTouchPosition ()
		{
#if !UNITY_EDITOR
			for (int i = 0; i < Input.touchCount; i++)
			{
				Touch touch = Input.GetTouch (i);
				if (touch.fingerId == fingerID)
				{
					return touch.position;
				}
			}
			return startPosition;
#else
			return Input.mousePosition;
#endif
		}


		private Vector2 GetTouchDrag ()
		{
#if !UNITY_EDITOR
			for (int i = 0; i < Input.touchCount; i++)
			{
				Touch touch = Input.GetTouch (i);
				if (touch.fingerId == fingerID)
				{
					return touch.deltaPosition;
				}
			}
			return Vector2.zero;
#else
			return GetTouchPosition () - lastFramePosition;
#endif
		}


		private void UpdateCentrePosition (Vector2 newCentrePosition)
		{
			if (centre == null)
			{
				return;
			}

			if (requiresContinuousDragging || fingerID < 0)
			{
				centre.localPosition = Vector2.Lerp (centre.localPosition, newCentrePosition, Time.deltaTime * centreLerpSpeed);
			}
			else
			{
				centre.localPosition = newCentrePosition;
			}
		}

		#endregion


		#region GetSet

		public float EffectScaler => effectScaler;

		#endregion


		#region EventSystem

		public delegate void Delegate_Joystick (Joystick joystick);
		public static Delegate_Joystick OnJoystickStartMoving, OnJoystickStopMoving;

		#endregion

	}

}