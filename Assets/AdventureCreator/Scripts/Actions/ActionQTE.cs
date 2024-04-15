/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionQTE.cs"
 * 
 *	This action checks if a specific key
 *	is being pressed within a set time limit
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
	public class ActionQTE : ActionCheck
	{

		public QTEType qteType = QTEType.SingleKeypress;
		public QTEHoldReleaseBehaviour qteHoldReleaseBehaviour = QTEHoldReleaseBehaviour.Reset;

		public int menuNameParameterID = -1;
		public string menuName;
		public bool animateUI = false;
		public bool wrongKeyFails = false;

		public float axisThreshold = 0.2f;

		public int inputNameParameterID = -1;
		public string inputName;
		
		public int durationParameterID = -1;
		public float duration;
		
		public float holdDuration;
		public float cooldownTime;
		public int targetPressesParameterID = -1;
		public int targetPresses;
		public bool doCooldown;
		public bool runIndefinitely;

		public string verticalInputName;
		public int verticalInputNameParameterID = -1;
		public bool rotationIsClockwise = true;
		public float targetRotations = 1f;
		public int targetRotationsParameterID = -1;

		
		public override ActionCategory Category { get { return ActionCategory.Input; }}
		public override string Title { get { return "QTE"; }}
		public override string Description { get { return "Initiates a Quick Time Event for a set duration. The QTE type can either be a single key- press, holding a button down, or button-mashing. The Input button must be defined in Unity's Input Manager."; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			menuName = AssignString (parameters, menuNameParameterID, menuName);
			inputName = AssignString (parameters, inputNameParameterID, inputName);
			duration = AssignFloat (parameters, durationParameterID, duration);
			targetPresses = AssignInteger (parameters, targetPressesParameterID, targetPresses);
		}
		
		
		public override float Run ()
		{
			if (string.IsNullOrEmpty (inputName) && (KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen || qteType == QTEType.SingleAxis))
			{
				isRunning = false;
				return 0f;
			}

			if (!runIndefinitely && duration <= 0f)
			{
				isRunning = false;
				return 0f;
			}

			float runtimeDuration = runIndefinitely ? -1f : duration;

			if (!isRunning)
			{
				isRunning = true;

				Animator animator = null;
				if (!string.IsNullOrEmpty (menuName))
				{
					AC.Menu menu = PlayerMenus.GetMenuWithName (menuName);
					if (menu != null)
					{
						menu.TurnOn (true);
						if (animateUI && menu.RuntimeCanvas)
						{
							animator = menu.RuntimeCanvas.GetComponent<Animator> ();
							if (animator == null)
							{
								animator = menu.RuntimeCanvas.GetComponentInChildren<Animator> ();
							}
						}
					}
				}

				switch (qteType)
				{
					case QTEType.SingleKeypress:
						KickStarter.playerQTE.StartSinglePressQTE (inputName, runtimeDuration, animator, wrongKeyFails);
						break;

					case QTEType.SingleAxis:
						KickStarter.playerQTE.StartSingleAxisQTE (inputName, runtimeDuration, axisThreshold, animator, wrongKeyFails);
						break;

					case QTEType.HoldKey:
						KickStarter.playerQTE.StartHoldKeyQTE (inputName, runtimeDuration, holdDuration, qteHoldReleaseBehaviour, animator, wrongKeyFails);
						break;

					case QTEType.ButtonMash:
						KickStarter.playerQTE.StartButtonMashQTE (inputName, runtimeDuration, targetPresses, doCooldown, cooldownTime, animator, wrongKeyFails);
						break;

					case QTEType.ThumbstickRotation:
						KickStarter.playerQTE.StartThumbstickRotationQTE (inputName, verticalInputName, runtimeDuration, targetRotations, rotationIsClockwise, animator, wrongKeyFails);
						break;
				}

				return defaultPauseTime;
			}
			else
			{
				if (KickStarter.playerQTE.GetState () == QTEState.Running)
				{
					return defaultPauseTime;
				}

				if (!string.IsNullOrEmpty (menuName))
				{
					AC.Menu menu = PlayerMenus.GetMenuWithName (menuName);
					if (menu != null)
					{
						menu.TurnOff (true);
					}
				}

				isRunning = false;
				return 0f;
			}
		}


		public override void Skip ()
		{
			KickStarter.playerQTE.SkipQTE ();
			if (!string.IsNullOrEmpty (menuName))
			{
				PlayerMenus.GetMenuWithName (menuName).TurnOff (true);
			}
		}
		

		public override bool CheckCondition ()
		{
			if (KickStarter.playerQTE.GetState () == QTEState.Win)
			{
				return true;
			}
			return false;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			qteType = (QTEType) EditorGUILayout.EnumPopup ("QTE type:" , qteType);
			string _label = string.Empty;

			if (qteType == QTEType.ThumbstickRotation)
			{
				TextField ("Horizontal input name:", ref inputName, parameters, ref inputNameParameterID);
				TextField ("Vertical input name:", ref verticalInputName, parameters, ref verticalInputNameParameterID);
				SliderField ("# of rotations:", ref targetRotations, 0.25f, 10f, parameters, ref targetRotationsParameterID);

				rotationIsClockwise = EditorGUILayout.Toggle ("Clockwise rotation?", rotationIsClockwise);
				_label = "Wrong direction fails?";
			}
			else
			{
				_label = (qteType == QTEType.SingleAxis) ? "axis" : "button";

				TextField ("Input " + _label + " name:", ref inputName, parameters, ref inputNameParameterID);

				if (qteType == QTEType.SingleAxis)
				{
					axisThreshold = EditorGUILayout.Slider ("Axis threshold:", axisThreshold, -1f, 1f);

					if (axisThreshold >= 0f)
					{
						_label = "Negative axis fails?";
					}
					else if (axisThreshold < 0f)
					{
						_label = "Positive axis fails?";
					}

					if (Mathf.Approximately (axisThreshold, 0f))
					{
						EditorGUILayout.HelpBox ("The 'Axis threshold' cannot be zero.", MessageType.Warning);
					}
					else if (axisThreshold > 0f)
					{
						EditorGUILayout.HelpBox ("The QTE will be succesful when the input value is greater than the 'Axis threshold'.", MessageType.Info);
					}
					else if (axisThreshold < 0f)
					{
						EditorGUILayout.HelpBox ("The QTE will be succesful when the input value is less than than the 'Axis threshold'.", MessageType.Info);
					}
				}
				else
				{
					_label = "Wrong button fails?";
			
					if (KickStarter.settingsManager != null && KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
					{
						EditorGUILayout.HelpBox ("If the input name field is left blank, then all screen taps will be valid.", MessageType.Info);
					}
				}
			}

			wrongKeyFails = EditorGUILayout.Toggle (_label, wrongKeyFails);

			runIndefinitely = EditorGUILayout.Toggle ("Run forever?", runIndefinitely);
			if (!runIndefinitely)
			{
				FloatField ("Duration (s):", ref duration, parameters, ref durationParameterID);
				if (durationParameterID < 0 && duration < 0f)
				{
					duration = 0f;
				}
			}
			
			if (qteType == QTEType.ButtonMash)
			{
				IntField ("Target # of presses:", ref targetPresses, parameters, ref targetPressesParameterID);

				doCooldown = EditorGUILayout.Toggle ("Cooldown effect?", doCooldown);
				if (doCooldown)
				{
					if (!runIndefinitely && durationParameterID < 0)
					{
						cooldownTime = EditorGUILayout.Slider ("Cooldown time (s):", cooldownTime, 0f, duration);
					}
					else
					{
						cooldownTime = EditorGUILayout.FloatField ("Cooldown time (s):", cooldownTime);
						if (cooldownTime < 0f) cooldownTime = 0f;
					}
				}
			}
			else if (qteType == QTEType.HoldKey)
			{
				if (!runIndefinitely && durationParameterID < 0)
				{
					holdDuration = EditorGUILayout.Slider ("Required duration (s):", holdDuration, 0f, duration);
				}
				else
				{
					holdDuration = EditorGUILayout.FloatField ("Required duration (s):", holdDuration);
				}
				qteHoldReleaseBehaviour = (QTEHoldReleaseBehaviour) EditorGUILayout.EnumPopup ("Release behaviour:", qteHoldReleaseBehaviour);
			}

			TextField ("Menu to display (optional):", ref menuName, parameters, ref menuNameParameterID);

			animateUI = EditorGUILayout.Toggle ("Animate UI?", animateUI);

			if (animateUI)
			{
				if (qteType == QTEType.SingleKeypress || qteType == QTEType.SingleAxis)
				{
					EditorGUILayout.HelpBox ("The Menu's UI must have an Animator with 2 States: Win, Lose.", MessageType.Info);
				}
				else if (qteType == QTEType.ButtonMash)
				{
					EditorGUILayout.HelpBox ("The Menu's UI must have an Animator with 3 States: Hit, Win, Lose.", MessageType.Info);
				}
				else if (qteType == QTEType.HoldKey)
				{
					EditorGUILayout.HelpBox ("The Menu's UI must have an Animator with 2 States: Win, Lose, and 1 Trigger: Held.", MessageType.Info);
				}
			}
		}
		
		
		public override string SetLabel ()
		{
			return qteType.ToString () + " - " + inputName;
		}
		
		#endif


		/**
		 * <summary>Creates a new instance of the 'Input: QTE' Action, set to detect a single keypress</summary>
		 * <param name = "inputButtonName">The name of the input button to detect</param>
		 * <param name = "duration">The duration of the QTE</param>
		 * <param name = "wrongButtonFails">If True, pressing the wrong button will fail the QTE</param>
		 * <param name = "menuToDisplay">The name of a menu to turn on during the QTE</param>
		 * <param name = "animateUI">If True, the Animator of a Unity UI-based menu will be controlled</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionQTE CreateNew_SingleKeypress (string inputButtonName, float duration, bool wrongButtonFails = false, string menuToDisplay = "", bool animateUI = false)
		{
			ActionQTE newAction = CreateNew<ActionQTE> ();
			newAction.qteType = QTEType.SingleKeypress;
			newAction.inputName = inputButtonName;
			newAction.duration = duration;
			newAction.wrongKeyFails = wrongButtonFails;
			newAction.menuName = menuToDisplay;
			newAction.animateUI = animateUI;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Input: QTE' Action, set to detect a single axis</summary>
		 * <param name = "inputAxisName">The name of the input axis to detect</param>
		 * <param name = "duration">The duration of the QTE</param>
		 * <param name = "axisThreshold">The minimum value that the axis should be to win</param>
		 * <param name = "wrongDirectionFails">If True, then moving the axis in the opposite direction will fail the QTE</param>
		 * <param name = "menuToDisplay">The name of a menu to turn on during the QTE</param>
		 * <param name = "animateUI">If True, the Animator of a Unity UI-based menu will be controlled</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionQTE CreateNew_SingleAxis (string inputAxisName, float duration, float axisThreshold = 0.2f, bool wrongDirectionFails = false, string menuToDisplay = "", bool animateUI = false)
		{
			ActionQTE newAction = CreateNew<ActionQTE> ();
			newAction.qteType = QTEType.SingleAxis;
			newAction.inputName = inputAxisName;
			newAction.duration = duration;
			newAction.axisThreshold = axisThreshold;
			newAction.wrongKeyFails = wrongDirectionFails;
			newAction.menuName = menuToDisplay;
			newAction.animateUI = animateUI;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Input: QTE' Action, set to detect a button held down</summary>
		 * <param name = "inputButtonName">The name of the input button to detect</param>
		 * <param name = "duration">The duration of the QTE</param>
		 * <param name = "requiredDuration">The duration that the button must be held down to win the QTE</param>
		 * <param name = "wrongButtonFails">If True, pressing the wrong button will fail the QTE</param>
		 * <param name = "menuToDisplay">The name of a menu to turn on during the QTE</param>
		 * <param name = "animateUI">If True, the Animator of a Unity UI-based menu will be controlled</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionQTE CreateNew_HoldKey (string inputButtonName, float duration, float requiredDuration, bool wrongButtonFails = false, string menuToDisplay = "", bool animateUI = false)
		{
			ActionQTE newAction = CreateNew<ActionQTE> ();
			newAction.qteType = QTEType.HoldKey;
			newAction.inputName = inputButtonName;
			newAction.duration = duration;
			newAction.holdDuration = requiredDuration;
			newAction.wrongKeyFails = wrongButtonFails;
			newAction.menuName = menuToDisplay;
			newAction.animateUI = animateUI;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Input: QTE' Action, set to detect button-mashing</summary>
		 * <param name = "inputButtonName">The name of the input button to mash</param>
		 * <param name = "duration">The duration of the QTE</param>
		 * <param name = "requiredPresses">The number of times to press the button</param>
		 * <param name = "cooldownTime">If positive, the time after which the registered number of button presses will decrease by one</param>
		 * <param name = "wrongButtonFails">If True, pressing the wrong button will fail the QTE</param>
		 * <param name = "menuToDisplay">The name of a menu to turn on during the QTE</param>
		 * <param name = "animateUI">If True, the Animator of a Unity UI-based menu will be controlled</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionQTE CreateNew_ButtonMash (string inputButtonName, float duration, int requiredPresses, float cooldownTime = -1f, bool wrongButtonFails = false, string menuToDisplay = "", bool animateUI = false)
		{
			ActionQTE newAction = CreateNew<ActionQTE> ();
			newAction.qteType = QTEType.ButtonMash;
			newAction.inputName = inputButtonName;
			newAction.duration = duration;
			newAction.targetPresses = requiredPresses;
			newAction.cooldownTime = cooldownTime;
			newAction.doCooldown = (cooldownTime >= 0f);
			newAction.wrongKeyFails = wrongButtonFails;
			newAction.menuName = menuToDisplay;
			newAction.animateUI = animateUI;
			return newAction;
		}
		
	}
	
}