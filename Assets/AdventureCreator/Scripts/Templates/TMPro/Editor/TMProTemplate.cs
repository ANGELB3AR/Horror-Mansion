// This script's code is adapted from davemeta's code available at: https://forum.unity.com/threads/replacing-text-with-textmesh-pro.515594/

#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Animations;

namespace AC.Templates.TMProIntegration
{

	public class TMProTemplate : Template
	{

		#region PublicFunctions

		public override bool CanInstall (ref string errorText)
		{
			#if TextMeshProIsPresent
			if (Resource.References.menuManager == null)
			{
				errorText = "No Menu Manager assigned";
				return false;
			}
			return true;
			#else
			return false;
			#endif
		}


		public override bool CanSuggest (NGWData data)
		{
			return true;
		}


		public override bool MeetsDependencyRequirements ()
		{
			#if TextMeshProIsPresent
			return true;
			#else
			return false;
			#endif
		}

		#endregion


		#region ProtectedFunctions

		protected override void MakeChanges (string installPath, bool canDeleteOldAssets, System.Action onComplete, System.Action<string> onFail)
		{
			#if TextMeshProIsPresent
			
			if (TMPro.TMP_Settings.defaultFontAsset == null)
			{
				onFail.Invoke ("Cannot convert Menus to TextMesh Pro because TMPro has no default Font asset assigned.");
				return;
			}
			
			foreach (Menu menu in Resource.References.menuManager.menus)
			{
				if (menu.PrefabCanvas == null) continue;

				bool wasUpdated = false;
				
				UnityEngine.UI.Button[] buttons = menu.PrefabCanvas.gameObject.GetComponentsInChildren<UnityEngine.UI.Button> ();
				ButtonSettings[] buttonSettings = new ButtonSettings[buttons.Length];
				for (int i = 0; i < buttons.Length; i++) buttonSettings[i] = new ButtonSettings (buttons[i]);

				Dropdown[] dropdowns = menu.PrefabCanvas.gameObject.GetComponentsInChildren<Dropdown> (true);
				foreach (var dropdown in dropdowns)
				{
					Convert (dropdown);
					wasUpdated = true;
				}

				Transform[] canvasChildren = menu.PrefabCanvas.gameObject.GetComponentsInChildren<Transform> (true);
				foreach (var child in canvasChildren)
				{
					if (Convert (child.gameObject))
					{
						wasUpdated = true;
					}
				}

				for (int i = 0; i < buttons.Length; i++) buttonSettings[i].Apply ();

				if (wasUpdated)
				{
					menu.useTextMeshProComponents = true;
					PrefabUtility.SavePrefabAsset (menu.PrefabCanvas.gameObject);
					EditorUtility.SetDirty (Resource.References.menuManager);
				}
			}
			AssetDatabase.SaveAssets ();

			onComplete?.Invoke ();

			#else

			onFail.Invoke ("TextMeshProIsPresent scripting define symbol not found - is the Text Mesh Pro package installed?");
			
			#endif
		}

		#endregion


		#region PrivateFunctions

#if TextMeshProIsPresent

		private void Convert (Dropdown dropdown)
		{
			if (dropdown == null) return;

			GameObject dropdownOb = dropdown.gameObject;
			DropdownSettings settings = new DropdownSettings (dropdown);

			Convert (settings.CaptionTextOb);
			Convert (settings.ItemTextOb);
			
			UnityEngine.Object.DestroyImmediate (dropdown, true);

			TMPro.TMP_Dropdown tmp = dropdownOb.AddComponent<TMPro.TMP_Dropdown> ();
			settings.Apply (tmp);
		}


		private void Convert (InputField inputField)
		{
			if (inputField == null) return;

			GameObject inputFieldOb = inputField.gameObject;
			InputFieldSettings settings = new InputFieldSettings (inputField);

			Convert (settings.TextComponentOb);
			Convert (settings.PlaceholderOb);
			
			UnityEngine.Object.DestroyImmediate (inputField, true);

			TMPro.TMP_InputField tmp = inputFieldOb.AddComponent<TMPro.TMP_InputField> ();
			settings.Apply (tmp);
		}


		private bool Convert (GameObject target)
		{
			if (target == null) return false;

			Text uiText = target.GetComponent<Text>();
			if (uiText == null) return false;
	
			var shadows = target.GetComponents<UnityEngine.UI.Shadow> ();
			foreach (var shadow in shadows)
			{
				UnityEngine.Object.DestroyImmediate (shadow, true);
			}

			var outlines = target.GetComponents<UnityEngine.UI.Outline> ();
			foreach (var outline in outlines)
			{
				UnityEngine.Object.DestroyImmediate (outline, true);
			}
	
			TextSettings settings = new TextSettings (uiText);
	
			UnityEngine.Object.DestroyImmediate (uiText, true);
	
			TMPro.TextMeshProUGUI tmp = target.AddComponent<TMPro.TextMeshProUGUI>();
			settings.Apply (tmp);
			
			return true;
		}

#endif

		#endregion


		#region GetSet

		public override string Label { get { return "TextMesh Pro menus"; }}
		public override string PreviewText { get { return "Updates all Unity UI Prefab menus to use TextMesh Pro components. Note: A default font asset must be assigned. (Requires TextMesh Pro)"; }}
		public override Type[] AffectedManagerTypes { get { return new Type[] { typeof (MenuManager) }; }}
		public override bool SelectedByDefault { get { return false; }}
		public override TemplateCategory Category { get { return TemplateCategory.Misc; }}
		public override int OrderInCategory { get { return 100; }}
		public override bool IsExclusiveToCategory { get { return false; }}
		
		#endregion


		#region PrivateClasses

#if TextMeshProIsPresent

		private class TextSettings
		{

			private readonly bool Enabled;
			private readonly TMPro.FontStyles FontStyle;
			private readonly float FontSize;
			private readonly float FontSizeMin;
			private readonly float FontSizeMax;
			private readonly float LineSpacing;
			private readonly bool EnableRichText;
			private readonly bool EnableAutoSizing;
			private readonly TMPro.TextAlignmentOptions TextAlignmentOptions;
			private readonly bool WrappingEnabled;
			private readonly TMPro.TextOverflowModes TextOverflowModes;
			private readonly string Text;
			private readonly Color Color;
			private readonly bool RayCastTarget;


			public TextSettings (Text uiText)
			{
				Enabled = uiText.enabled;
				FontStyle = FontStyleToFontStyles (uiText.fontStyle);
				FontSize = uiText.fontSize;
				FontSizeMin = uiText.resizeTextMinSize;
				FontSizeMax = uiText.resizeTextMaxSize;
				LineSpacing = uiText.lineSpacing;
				EnableRichText = uiText.supportRichText;
				EnableAutoSizing = uiText.resizeTextForBestFit;
				TextAlignmentOptions = TextAnchorToTextAlignmentOptions (uiText.alignment);
				WrappingEnabled = HorizontalWrapModeToBool (uiText.horizontalOverflow);
				TextOverflowModes = VerticalWrapModeToTextOverflowModes (uiText.verticalOverflow);
				Text = uiText.text;
				Color = uiText.color;
				RayCastTarget = uiText.raycastTarget;
			}


			public void Apply (TMPro.TextMeshProUGUI uiText)
			{
				uiText.enabled = Enabled;
				uiText.fontStyle = FontStyle;
				uiText.fontSize = FontSize;
				uiText.fontSizeMin = FontSizeMin;
				uiText.fontSizeMax = FontSizeMax;
				uiText.lineSpacing = LineSpacing;
				uiText.richText = EnableRichText;
				uiText.enableAutoSizing = EnableAutoSizing;
				uiText.alignment = TextAlignmentOptions;
				uiText.enableWordWrapping = WrappingEnabled;
				uiText.overflowMode = TextOverflowModes;
				uiText.text = Text;
				uiText.color = Color;
				uiText.raycastTarget = RayCastTarget;
			}

			
			private bool HorizontalWrapModeToBool (HorizontalWrapMode overflow)
			{
				return overflow == HorizontalWrapMode.Wrap;
			}
		
			
			private TMPro.TextOverflowModes VerticalWrapModeToTextOverflowModes (VerticalWrapMode verticalOverflow)
			{
				return verticalOverflow == VerticalWrapMode.Truncate ? TMPro.TextOverflowModes.Truncate : TMPro.TextOverflowModes.Overflow;
			}
		
			
			private TMPro.FontStyles FontStyleToFontStyles (FontStyle fontStyle)
			{
				switch (fontStyle)
				{
					case UnityEngine.FontStyle.Normal:
					default:
						return TMPro.FontStyles.Normal;
		
					case UnityEngine.FontStyle.Bold:
						return TMPro.FontStyles.Bold;
		
					case UnityEngine.FontStyle.Italic:
						return TMPro.FontStyles.Italic;
		
					case UnityEngine.FontStyle.BoldAndItalic:
						return TMPro.FontStyles.Bold | TMPro.FontStyles.Italic;
				}
			}
		
		
			private TMPro.TextAlignmentOptions TextAnchorToTextAlignmentOptions (TextAnchor textAnchor)
			{
				switch (textAnchor)
				{
					case TextAnchor.UpperLeft:
					default:
						return TMPro.TextAlignmentOptions.TopLeft;
		
					case TextAnchor.UpperCenter:
						return TMPro.TextAlignmentOptions.Top;
		
					case TextAnchor.UpperRight:
						return TMPro.TextAlignmentOptions.TopRight;
		
					case TextAnchor.MiddleLeft:
						return TMPro.TextAlignmentOptions.Left;
		
					case TextAnchor.MiddleCenter:
						return TMPro.TextAlignmentOptions.Center;
		
					case TextAnchor.MiddleRight:
						return TMPro.TextAlignmentOptions.Right;
		
					case TextAnchor.LowerLeft:
						return TMPro.TextAlignmentOptions.BottomLeft;
		
					case TextAnchor.LowerCenter:
						return TMPro.TextAlignmentOptions.Bottom;
		
					case TextAnchor.LowerRight:
						return TMPro.TextAlignmentOptions.BottomRight;
				}
			}


		}


		private class DropdownSettings
		{
			
			private readonly bool Enabled;
			private readonly Selectable.Transition Transition;
			private readonly UnityEngine.UI.Navigation Navigation;
			private readonly RectTransform Template;
			public readonly GameObject CaptionTextOb;
			private readonly Image CaptionImage;
			public readonly GameObject ItemTextOb;
			private readonly Image ItemImage;
			private readonly int Value;
			private readonly float AlphaFadeSpeed;
			private readonly Dropdown.OptionData[] Options;


			public DropdownSettings (Dropdown dropdown)
			{
				Enabled = dropdown.enabled;
				Transition = dropdown.transition;
				Navigation = dropdown.navigation;
				Template = dropdown.template;
				CaptionTextOb = dropdown.captionText ? dropdown.captionText.gameObject : null;
				CaptionImage = dropdown.captionImage;
				ItemTextOb = dropdown.itemText ? dropdown.itemText.gameObject : null;
				ItemImage = dropdown.itemImage;
				Value = dropdown.value;
				AlphaFadeSpeed = dropdown.alphaFadeSpeed;
				Options = dropdown.options.ToArray ();

				if (dropdown.onValueChanged != null && dropdown.onValueChanged.GetPersistentEventCount () > 0)
				{
					ACDebug.LogWarning ("'OnValueChanged' event for Dropdown " + dropdown.gameObject + " cannot be transferred to TMP_Dropdown - it must be added manually.", dropdown.gameObject);
				}
			}


			public void Apply (TMPro.TMP_Dropdown dropdown)
			{
				dropdown.enabled = Enabled;
				dropdown.transition = Transition;
				dropdown.navigation = Navigation;
				dropdown.template = Template;
				dropdown.captionText = CaptionTextOb ? CaptionTextOb.GetComponent<TMPro.TextMeshProUGUI> () : null;
				dropdown.captionImage = CaptionImage;
				dropdown.itemText = ItemTextOb ? ItemTextOb.GetComponent<TMPro.TextMeshProUGUI> () : null;
				dropdown.value = Value;
				dropdown.alphaFadeSpeed = AlphaFadeSpeed;
				foreach (var option in Options)
				{
					var optionData = new TMPro.TMP_Dropdown.OptionData
					{
						text = option.text,
						image = option.image
					};
					dropdown.options.Add (optionData);
				}
			}

		}


		private class InputFieldSettings
		{
			
			private readonly bool Enabled;
			private readonly Selectable.Transition Transition;
			private readonly UnityEngine.UI.Navigation Navigation;
			public readonly GameObject TextComponentOb;
			private readonly string Text;
			public readonly GameObject PlaceholderOb;
			public float CaretBlinkRate;
			public int CaretWidth;
			public bool CustomCaretColor;
			public Color SelectionColor;


			public InputFieldSettings (InputField inputField)
			{
				Enabled = inputField.enabled;
				Transition = inputField.transition;
				Navigation = inputField.navigation;
				TextComponentOb = inputField.textComponent ? inputField.textComponent.gameObject : null;
				Text = inputField.text;
				PlaceholderOb = inputField.placeholder ? inputField.placeholder.gameObject : null;
				CaretBlinkRate = inputField.caretBlinkRate;
				CaretWidth = inputField.caretWidth;
				CustomCaretColor = inputField.customCaretColor;
				SelectionColor = inputField.selectionColor;

				if (inputField.onValueChanged != null && inputField.onValueChanged.GetPersistentEventCount () > 0)
				{
					ACDebug.LogWarning ("'OnValueChanged' event for InputField " + inputField.gameObject + " cannot be transferred to TMP_InputField - it must be added manually.", inputField.gameObject);
				}
			}


			public void Apply (TMPro.TMP_InputField inputField)
			{
				inputField.enabled = Enabled;
				inputField.transition = Transition;
				inputField.navigation = Navigation;
				inputField.textComponent = TextComponentOb ? TextComponentOb.GetComponent<TMPro.TextMeshProUGUI> () : null;
				inputField.text = Text;
				inputField.placeholder = PlaceholderOb ? PlaceholderOb.GetComponent<TMPro.TextMeshProUGUI> () : null;
				inputField.caretBlinkRate = CaretBlinkRate;
				inputField.caretWidth = CaretWidth;
				inputField.customCaretColor = CustomCaretColor;
				inputField.selectionColor = SelectionColor;
			}

		}


		private class ButtonSettings
		{

			private readonly UnityEngine.UI.Button Button;
			private readonly GameObject TargetGraphicObject;


			public ButtonSettings (UnityEngine.UI.Button button)
			{
				Button = button;
				TargetGraphicObject = button.targetGraphic && button.targetGraphic.GetComponent<Text> () ? button.targetGraphic.gameObject : null;
			}


			public void Apply ()
			{
				if (TargetGraphicObject)
				{
					Button.targetGraphic = TargetGraphicObject.GetComponent<UnityEngine.UI.Graphic> ();
				}
			}

		}

#endif

		#endregion

	}


	[CustomEditor (typeof (TMProTemplate))]
	public class TMProTemplateEditor : TemplateEditor
	{}

}

#endif