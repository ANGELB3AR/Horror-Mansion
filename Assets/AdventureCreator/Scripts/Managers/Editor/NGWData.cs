#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using AC;

namespace AC
{

	public class NGWData : ScriptableObject
	{

		#region Variables

		[SerializeField] private CursorManager defaultCursorManager = null;
		[SerializeField] private MenuManager defaultMenuManager = null;

		[SerializeField] private Texture2D perspective2DPreview = null;
		[SerializeField] private Texture2D perspective25DPreview = null;
		[SerializeField] private Texture2D perspective3DPreview = null;

		[SerializeField] private Texture2D base2DPreview = null;
		[SerializeField] private Texture2D base25DPreview = null;
		[SerializeField] private Texture2D base3DPreview = null;

		[SerializeField] private Texture2D nonePreview = null;
		[SerializeField] private Texture2D pointAndClickPreview = null;
		[SerializeField] private Texture2D directPreview = null;
		[SerializeField] private Texture2D firstPersonPreview = null;

		[SerializeField] private Texture2D mouseAndKeyboardPreview = null;
		[SerializeField] private Texture2D keyboardOrControllerPreview = null;
		[SerializeField] private Texture2D touchScreenPreview = null;

		[SerializeField] private Texture2D contextSensitivePreview = null;
		[SerializeField] private Texture2D chooseHotspotThenInteractionPreview = null;
		[SerializeField] private Texture2D chooseInteractionThenHotspotPreview = null;

		private Option<CameraPerspective> perspectiveOption = new Option<CameraPerspective> ();
		private Option<MovementMethod> movementOption = new Option<MovementMethod> ();
		private Option<InputMethod> inputOption = new Option<InputMethod> ();
		private Option<AC_InteractionMethod> interactionOption = new Option<AC_InteractionMethod> ();

		private InterfaceOption interfaceOption = InterfaceOption.AdventureCreator;
		private enum InterfaceOption { None, AdventureCreator, UnityUI }

		private CameraPerspective cameraPerspective;
		private MovementMethod movementMethod;
		private InputMethod inputMethod;
		private AC_InteractionMethod interactionMethod;
		private HotspotDetection hotspotDetection;

		private Vector2 scrollPosition;

		#endregion


		#region PublicFunctions

		public Texture2D GetTemplateBackground (bool fromLiveManagers)
		{
			CameraPerspective perspective = cameraPerspective;
			if (fromLiveManagers && Resource.References.settingsManager)
			{
				perspective = Resource.References.settingsManager.cameraPerspective;
			}

			switch (perspective)
			{
				case CameraPerspective.TwoD:
					return base2DPreview;

				case CameraPerspective.TwoPointFiveD:
					return base25DPreview;

				default:
					return base3DPreview;
			}
		}


		public void RebuildPerspectiveOptions ()
		{
			perspectiveOption.RebuildOptionData
			(
				new OptionData<CameraPerspective>[]
				{
					new OptionData<CameraPerspective> () { label = "2D", description = "Sprite characters on sprite backgrounds, like the classic Lucasarts and Sierra games of old.", icon = perspective2DPreview, value = CameraPerspective.TwoD},
					new OptionData<CameraPerspective> () { label = "2.5D", description = "3D characters atop pre-rendered 2D backgrounds, as with games like The Longest Journey and Grim Fandango.", icon = perspective25DPreview, value = CameraPerspective.TwoPointFiveD},
					new OptionData<CameraPerspective> () { label = "3D", description = "Fully-3D environments and cameras, for a more modern style.", icon = perspective3DPreview, value = CameraPerspective.ThreeD},
				}
			);
		}
		

		public void RebuildMovementOptions ()
		{
			switch (perspectiveOption.Value)
			{
				case CameraPerspective.TwoD:
					movementOption.RebuildOptionData
					(
						new OptionData<MovementMethod>[]
						{
							new OptionData<MovementMethod> () { label = "None", description = "The Player is not controlled through gameplay.", icon = nonePreview, value = MovementMethod.None},
							new OptionData<MovementMethod> () { label = "Point and click", description = "Click the screen and the Player uses pathfinding to move there.", icon = pointAndClickPreview, value = MovementMethod.PointAndClick},
							new OptionData<MovementMethod> () { label = "Direct", description = "Use a keyboard, gamepad or on-screen joystick to move the Player freely in any direction.", icon = directPreview, value = MovementMethod.Direct},
						},
						1
					);
					break;

				case CameraPerspective.TwoPointFiveD:
					movementOption.RebuildOptionData
					(
						new OptionData<MovementMethod>[]
						{
							new OptionData<MovementMethod> () { label = "Point and click", description = "Click the screen and the Player uses pathfinding to move there.", icon = pointAndClickPreview, value = MovementMethod.PointAndClick},
							new OptionData<MovementMethod> () { label = "Direct", description = "Use a keyboard, gamepad or on-screen joystick to move the Player freely in any direction.", icon = directPreview, value = MovementMethod.Direct},
						},
						0
					);
					break;

				case CameraPerspective.ThreeD:
					movementOption.RebuildOptionData
					(
						new OptionData<MovementMethod>[]
						{
							new OptionData<MovementMethod> () { label = "None", description = "The Player is not controlled through gameplay.", icon = nonePreview, value = MovementMethod.None},
							new OptionData<MovementMethod> () { label = "Point and click", description = "Click the screen and the Player uses pathfinding to move there.", icon = pointAndClickPreview, value = MovementMethod.PointAndClick},
							new OptionData<MovementMethod> () { label = "Direct", description = "Use a keyboard, gamepad or on-screen joystick to move the Player freely in any direction.", icon = directPreview, value = MovementMethod.Direct},
							new OptionData<MovementMethod> () { label = "First person", description = "Use a keyboard, gamepad or on-screen joystick to move the Player freely in any direction.", icon = firstPersonPreview, value = MovementMethod.FirstPerson},
						},
						1
					);
					break;
			}
		}


		public void RebuildInputOptions ()
		{
			inputOption.RebuildOptionData
			(
				new OptionData<InputMethod>[]
				{
					new OptionData<InputMethod> () { label = "Mouse and keyboard", description = "The game is mainly mouse-driven, but the keyboard may be involved as well.", icon = mouseAndKeyboardPreview, value = InputMethod.MouseAndKeyboard},
					new OptionData<InputMethod> () { label = "Keyboard or controller", description = "The game is played with a keyboard or a gamepad, for a more arcade feel.", icon = keyboardOrControllerPreview, value = InputMethod.KeyboardOrController},
					new OptionData<InputMethod> () { label = "Touch-screen", description = "The game is played on a touch-screen device, such as a mobile or tablet.", icon = touchScreenPreview, value = InputMethod.TouchScreen},
				}
			);
		}


		public void RebuildInteractionOptions ()
		{
			interactionOption.RebuildOptionData
			(
				new OptionData<AC_InteractionMethod>[]
				{
					new OptionData<AC_InteractionMethod> () { label = "Context Sensitive", description = "A one-button interface, for maximum simplicity.  Click/tap a Hotspot to use it, with maybe a separate input to examine it.", icon = contextSensitivePreview, value = AC_InteractionMethod.ContextSensitive},
					new OptionData<AC_InteractionMethod> () { label = "Choose Hotspot Then Interaction", description = "Select a Hotspot, and a way of interacting with it.", icon = chooseHotspotThenInteractionPreview, value = AC_InteractionMethod.ChooseHotspotThenInteraction},
					new OptionData<AC_InteractionMethod> () { label = "Choose Interaction Then Hotspot", description = "First choose a way of interacting, then select a Hotspot to use in that way.", icon = chooseInteractionThenHotspotPreview, value = AC_InteractionMethod.ChooseInteractionThenHotspot},
				}
			);
		}


		public void ShowPerspectiveGUI (Rect position)
		{
			ShowOptionGUI<CameraPerspective> (perspectiveOption, position);
		}


		public void ShowMovementGUI (Rect position)
		{
			ShowOptionGUI<MovementMethod> (movementOption, position);
		}


		public void ShowInputGUI (Rect position)
		{
			ShowOptionGUI<InputMethod> (inputOption, position);
		}


		public void ShowInteractionGUI (Rect position)
		{
			ShowOptionGUI<AC_InteractionMethod> (interactionOption, position);
		}


		public void PrepareReview ()
		{
			cameraPerspective = perspectiveOption.Value;
			movementMethod = movementOption.Value;
			inputMethod = inputOption.Value;
			interactionMethod = interactionOption.Value;
			hotspotDetection = (movementMethod == MovementMethod.Direct && inputMethod == InputMethod.KeyboardOrController) ? HotspotDetection.PlayerVicinity : HotspotDetection.MouseOver;
			interfaceOption = InterfaceOption.AdventureCreator;
		}


		public void ShowReviewGUI (float width)
		{
			EditorGUILayout.BeginVertical ();
			EditorGUIUtility.labelWidth = 180;

			string[] cameraPerspective_list = { "2D", "2.5D", "3D" };
			int cameraPerspective_int = (int) cameraPerspective;
			cameraPerspective_int = EditorGUILayout.Popup ("Camera perspective:", cameraPerspective_int, cameraPerspective_list, GUILayout.Width (width));
			cameraPerspective = (CameraPerspective) cameraPerspective_int;

			movementMethod = (MovementMethod) EditorGUILayout.EnumPopup (new GUIContent ("Movement method:", "How the Player character is moved around the scene"), movementMethod, GUILayout.Width (width));
			inputMethod = (InputMethod) EditorGUILayout.EnumPopup (new GUIContent ("Input method:", "The main input device used to play the game"), inputMethod, GUILayout.Width (width));
			hotspotDetection = (HotspotDetection) EditorGUILayout.EnumPopup (new GUIContent ("Hotspot detection:", "The way in which Hotspots in the scene are selected"), hotspotDetection, GUILayout.Width (width));
			interactionMethod = (AC_InteractionMethod) EditorGUILayout.EnumPopup (new GUIContent ("Interaction method:", "The way in which Hotspot interactions are chosen"), interactionMethod, GUILayout.Width (width));
			interfaceOption = (InterfaceOption) EditorGUILayout.EnumPopup (new GUIContent ("Default interface:", "The method used to render the game's default Menus"), interfaceOption, GUILayout.Width (width));

			EditorGUIUtility.labelWidth = 0;
			EditorGUILayout.EndVertical ();
		}


		public void Apply (string installPath, SettingsManager settingsManager, CursorManager cursorManager, MenuManager menuManager, SpeechManager speechManager)
		{
			settingsManager.cameraPerspective = cameraPerspective;
			settingsManager.movementMethod = movementMethod;
			settingsManager.inputMethod = inputMethod;
			settingsManager.interactionMethod = interactionMethod;
			settingsManager.hotspotDetection = hotspotDetection;

			if (interfaceOption != InterfaceOption.None)
			{
				DefaultInterface.Apply (this, installPath, cursorManager, menuManager, speechManager);
			}
		}

		#endregion


		#region PrivateFunctions

		private void ShowOptionGUI<T> (Option<T> option, Rect position) where T : System.Enum
		{
			int numOptions = option.Labels.Length;
			int totalScrollViewHeight = 30 * numOptions;

			GUI.Box (new Rect (NewGameWizardWindow.Padding, 160, 315, totalScrollViewHeight + 40), "", CustomStyles.Header);

			scrollPosition = GUI.BeginScrollView (new Rect (NewGameWizardWindow.Padding + 20, 180, 285, 280), scrollPosition, new Rect (0, 0, NewGameWizardWindow.ScrollBoxWidth - 20, totalScrollViewHeight));

			string[] optionLabels = new string[numOptions];
			for (int i = 0; i < optionLabels.Length; i++)
			{
				optionLabels[i] = option.Labels[i];
			}

			option.SelectedIndex = GUI.SelectionGrid (new Rect (0, 0, 275, totalScrollViewHeight), option.SelectedIndex, optionLabels, 1, NewGameWizardWindow.ButtonStyle);

			GUI.EndScrollView ();

			// Details box
			var optionData = option.optionDatas[option.SelectedIndex];
			GUI.Box (new Rect (position.width - NewGameWizardWindow.PreviewImageWidth - 35 - NewGameWizardWindow.Padding, 160, NewGameWizardWindow.PreviewImageWidth + 40, 320), "", CustomStyles.Header);

			GUI.DrawTexture (new Rect (position.width - NewGameWizardWindow.PreviewImageWidth - NewGameWizardWindow.Padding - 15, 180, NewGameWizardWindow.PreviewImageWidth, NewGameWizardWindow.PreviewImageHeight), optionData.icon, ScaleMode.StretchToFill);
			GUI.Label (new Rect (position.width - NewGameWizardWindow.PreviewImageWidth - NewGameWizardWindow.Padding - 15, 380, NewGameWizardWindow.PreviewImageWidth, 40), optionData.label, CustomStyles.managerHeader);
			bool wordWrapBackup = GUI.skin.label.wordWrap;
			GUI.skin.label.wordWrap = true;
			GUI.Label (new Rect (position.width - NewGameWizardWindow.PreviewImageWidth - NewGameWizardWindow.Padding - 15, 400, NewGameWizardWindow.PreviewImageWidth, 80), optionData.description, NewGameWizardWindow.LabelStyle);
			GUI.skin.label.wordWrap = wordWrapBackup;
		}

		#endregion


		#region GetSet

		public CursorManager DefaultCursorManager { get { return defaultCursorManager; }}
		public MenuManager DefaultMenuManager { get { return defaultMenuManager; }}

		public CameraPerspective CameraPerspective { get { return cameraPerspective; }}
		public InputMethod InputMethod { get { return inputMethod; }}
		public AC_InteractionMethod InteractionMethod { get { return interactionMethod; }}
		public MovementMethod MovementMethod { get { return movementMethod; }}

		public MenuSource MenuSource { get { return (interfaceOption == InterfaceOption.UnityUI) ? MenuSource.UnityUiPrefab : MenuSource.AdventureCreator; }}

		#endregion


		#region PrivateClasses

		private class Option<T> where T : System.Enum
		{

			public OptionData<T>[] optionDatas;
			private int selectedIndex;


			public void RebuildOptionData (OptionData<T>[] _optionDatas, int _selectedIndex = 0)
			{
				optionDatas = _optionDatas;
				selectedIndex = _selectedIndex;
			}

			
			public string[] Labels
			{
				get
				{
					string[] labels = new string[optionDatas.Length];
					for (int i = 0; i < labels.Length; i++)
					{
						labels[i] = optionDatas[i].label;
					}
					return labels;
				}
			}


			public int SelectedIndex
			{
				get
				{
					if (selectedIndex >= 0 && selectedIndex < optionDatas.Length)
					{
						return selectedIndex;
					}
					return 0;
				}
				set
				{
					selectedIndex = Mathf.Clamp (value, 0, optionDatas.Length);
				}
			}


			public int NumOptions { get { return optionDatas.Length; }}
			public T Value { get { return optionDatas[selectedIndex].value; }}

		}


		private class OptionData<T> where T : System.Enum
		{

			public string label;
			public string description;
			public Texture2D icon;
			public T value;

		}

		#endregion

	}

}

#endif