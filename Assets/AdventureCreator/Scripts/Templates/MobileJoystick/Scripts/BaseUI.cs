using System;
using UnityEngine;

namespace AC.Templates.MobileJoystick
{

	[Serializable]
	public abstract class BaseUI
	{

		#region Variables

		[SerializeField] private RectTransform boundary = null;
		[SerializeField] private CanvasGroup canvasGroup = null;
		protected int fingerID = -1;

		private enum Visible { Always, DuringGameplay, OnlyWhenUsed };
		[SerializeField] private Visible visible = Visible.Always;
		[SerializeField] [Range (0f, 1f)] private float hiddenAlpha = 0.3f;

		#endregion


		#region PublicFunctions

		public virtual void Update (JoystickUI joystickUI)
		{
			bool isInGameplay = KickStarter.stateHandler.IsInGameplay ();
			UpdateVisibility (isInGameplay);
		}


#if UNITY_EDITOR

		public virtual void ShowGUI (string label)
		{
			boundary = (RectTransform) CustomGUILayout.ObjectField<RectTransform> ("Touch boundary:", boundary, true);
			visible = (Visible) CustomGUILayout.EnumPopup ("Visible:", visible);

			switch (visible)
			{
				case Visible.DuringGameplay:
				case Visible.OnlyWhenUsed:
					canvasGroup = (CanvasGroup) CustomGUILayout.ObjectField<CanvasGroup> ("Canvas Group:", canvasGroup, true);
					hiddenAlpha = CustomGUILayout.Slider ("Hidden alpha:", hiddenAlpha, 0f, 1f);
					break;

				default:
					break;
			}
		}

#endif

		#endregion


		#region ProtectedFunctions

		protected void UpdateVisibility (bool isInGameplay)
		{
			if (canvasGroup == null)
			{
				return;
			}

			bool isVisible = true;
			switch (visible)
			{
				case Visible.DuringGameplay:
					isVisible = isInGameplay;
					break;

				case Visible.OnlyWhenUsed:
					isVisible = fingerID >= 0;
					break;

				default:
					break;
			}

			canvasGroup.alpha = isVisible ? 1f : hiddenAlpha;
		}


		protected int GetBeginningTouchIndex (Canvas canvas)
		{
#if !UNITY_EDITOR
			for (int i = 0; i < Mathf.Min (2, Input.touchCount); i++)
			{
				Touch touch = Input.GetTouch (i);
				if (touch.phase == TouchPhase.Began && PointIsInBoundary (canvas, touch.position))
				{
					return touch.fingerId;
				}
			}
#else
			Vector2 mousePosition = Input.mousePosition;
			if (Input.GetMouseButtonDown (0) && PointIsInBoundary (canvas, mousePosition))
			{
				return 0;
			}
#endif
			return -1;
		}


		protected bool PointIsInBoundary (Canvas canvas, Vector2 point)
		{
			if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				return RectTransformUtility.RectangleContainsScreenPoint (boundary, point, null);
			}
			return RectTransformUtility.RectangleContainsScreenPoint (boundary, point, canvas.worldCamera);
		}


		protected bool IsStillTouching ()
		{
#if !UNITY_EDITOR
			for (int i = 0; i < Input.touchCount; i++)
			{
				Touch touch = Input.GetTouch (i);
				if (touch.fingerId == fingerID && touch.phase != TouchPhase.Ended)
				{
					return true;
				}
			}
			return false;
#else
			return Input.GetMouseButton (0);
#endif
		}

		#endregion


		#region GetSet

		public bool IsUsed { get { return fingerID >= 0; }}
		public int FingerID { get { return fingerID; }}
		protected RectTransform Boundary { get { return boundary; }}

		#endregion

	}

}