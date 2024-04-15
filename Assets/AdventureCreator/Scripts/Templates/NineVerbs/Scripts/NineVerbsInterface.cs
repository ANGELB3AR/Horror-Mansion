using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AC.Templates.NineVerbs
{

	public class NineVerbsInterface : MonoBehaviour
	{

		#region Variables

		[SerializeField] private int useCursorID = 0;
		[SerializeField] private int giveCursorID = 4;
		[SerializeField] private float reachClearDelay = 0.5f;
		[SerializeField] private float verbLineFlashDuration = 0.2f;
		[SerializeField] private string verbLineElementName = "Verb line";
		public Color verbLineNormalColor = new Color (0.9f, 0.9f, 0.9f);
		public Color verbLineHighlightedColor = Color.white;

		private Menu menu;
		private float verbLineFlashTime = 0f;
		private string overrideText;
		private MenuLabel verbLine;
		private MenuInteraction[] verbElements;
		private ColorBlock normalColorBlock;
		private ColorBlock highlightedColorBlock;

		#endregion


		#region UnityStandards

		private void Start ()
		{
			menu = KickStarter.playerMenus.GetMenuWithCanvas (GetComponent<Canvas> ());
			if (verbElements == null) InitVerbButtons ();
			verbLine = menu.GetElementWithName (verbLineElementName) as MenuLabel;
		}


		private void OnEnable ()
		{
			EventManager.OnHotspotInteract += OnHotspotInteract;
			EventManager.OnHotspotReach += OnHotspotReach;
			EventManager.OnHotspotStopMovingTo += OnHotspotStopMovingTo;
			EventManager.OnEnterGameState += OnEnterGameState;
			EventManager.OnInventoryInteract_Alt += OnInventoryInteract;
			EventManager.OnCharacterSetPath += OnCharacterSetPath;
			EventManager.OnRequestMenuElementHotspotLabel += OnRequestMenuElementHotspotLabel;
			EventManager.OnMenuElementClick += OnMenuElementClick;
			EventManager.OnAfterChangeScene += OnAfterChangeScene;

			if (KickStarter.stateHandler && KickStarter.stateHandler.IsInGameplay ())
			{
				ClearOverrideText ();
			}
		}


		private void OnDisable ()
		{
			EventManager.OnHotspotInteract -= OnHotspotInteract;
			EventManager.OnHotspotReach -= OnHotspotReach;
			EventManager.OnHotspotStopMovingTo -= OnHotspotStopMovingTo;
			EventManager.OnEnterGameState -= OnEnterGameState;
			EventManager.OnInventoryInteract_Alt -= OnInventoryInteract;
			EventManager.OnCharacterSetPath -= OnCharacterSetPath;
			EventManager.OnRequestMenuElementHotspotLabel -= OnRequestMenuElementHotspotLabel;
			EventManager.OnMenuElementClick -= OnMenuElementClick;
			EventManager.OnAfterChangeScene -= OnAfterChangeScene;
		}


		private void Update ()
		{
			if (KickStarter.stateHandler.IsInGameplay ())
			{
				menu.SetHotspot (KickStarter.playerInteraction.GetActiveHotspot (), KickStarter.runtimeInventory.HoverInstance);
			}
			else
			{
				menu.SetHotspot (null, null);
			}
			
			verbLine.OverrideLabel (overrideText);
			
			foreach (MenuInteraction verbElement in verbElements)
			{
				verbElement.uiButton.colors = (verbElement.IsDefaultIcon) ? highlightedColorBlock : normalColorBlock;
			}

			if (verbLineFlashTime > 0f)
			{
				verbLineFlashTime -= Time.deltaTime;
			}

			verbLine.SetUITextColor ((!string.IsNullOrEmpty (overrideText) || verbLineFlashTime > 0f)
										? verbLineHighlightedColor
										: verbLineNormalColor);
			}

		#endregion


		#region EventHooks

		private void OnCharacterSetPath (Char character, Paths path)
		{
			if (KickStarter.stateHandler.IsInGameplay () &&
				character == KickStarter.player &&
				KickStarter.playerInteraction.GetHotspotMovingTo () == null)
			{
				verbLineFlashTime = verbLineFlashDuration;
			}
		}


		private void OnEnterGameState (GameState gamestate)
		{
			if (gamestate == GameState.Normal)
			{
				ClearOverrideText ();
			}
		}


		private void OnHotspotInteract (Hotspot hotspot, AC.Button button)
		{
			if (button == null)
			{
				return;
			}
			
			if (hotspot.interactionSource == InteractionSource.InScene)
			{
				if (button.interaction == null && button.playerAction == PlayerAction.DoNothing) return;
				if (button.interaction && button.interaction.actionListType == ActionListType.RunInBackground) return;
			}
			else if (hotspot.interactionSource == InteractionSource.AssetFile)
			{
				if (button.assetFile == null && button.playerAction == PlayerAction.DoNothing) return;
				if (button.assetFile && button.assetFile.actionListType == ActionListType.RunInBackground) return;
			}

			SetOverrideText (button.GetFullLabel (hotspot, null, Options.GetLanguage ()));
		}


		private void OnHotspotStopMovingTo (Hotspot hotspot)
		{
			ClearOverrideText ();
		}


		private void OnHotspotReach (Hotspot hotspot, AC.Button button)
		{
			switch (hotspot.interactionSource)
			{
				case InteractionSource.AssetFile:
					if (button.assetFile != null && button.assetFile.actionListType == ActionListType.PauseGameplay)
					{
						return;
					}
					break;

				case InteractionSource.InScene:
					if (button.interaction != null && button.interaction.actionListType == ActionListType.PauseGameplay)
					{
						return;
					}
					break;

				default:
					break;
			}

			Invoke ("ClearOverrideText", reachClearDelay);
		}


		private void OnInventoryInteract (InvInstance invInstance, int iconID)
		{
			bool foundInteraction = false;
			foreach (InvInteraction interaction in invInstance.Interactions)
			{
				if (interaction.icon.id == iconID)
				{
					foundInteraction = true;

					if (interaction.actionList == null || interaction.actionList.actionListType == ActionListType.RunInBackground)
					{
						ClearOverrideText ();
						return;
					}
				}
			}

			if (!foundInteraction)
			{
				// Run unhandled
				ActionListAsset unhandledAsset = KickStarter.cursorManager.GetUnhandledInteraction (iconID);
				if (unhandledAsset == null || unhandledAsset.actionListType == ActionListType.RunInBackground)
				{
					ClearOverrideText ();
					return;
				}
			}

			string prefix = KickStarter.cursorManager.GetLabelFromID (iconID, Options.GetLanguage ());
			string itemName = invInstance.ItemLabel;
			if (invInstance.InvItem.canBeLowerCase && !string.IsNullOrEmpty (prefix))
			{
				itemName = itemName.ToLower ();
			}

			SetOverrideText (AdvGame.CombineLanguageString (prefix, itemName, Options.GetLanguage ()));
		}


		private string OnRequestMenuElementHotspotLabel (Menu menu, MenuElement element, int slot, int language)
		{
			MenuInventoryBox inventoryBox = element as MenuInventoryBox;
			if (inventoryBox != null)
			{
				InvItem hoverItem = inventoryBox.GetItem (slot);
				if (hoverItem != null)
				{
					string hoverItemName = hoverItem.GetLabel (language);
					if (hoverItem.canBeLowerCase) hoverItemName = hoverItemName.ToLower ();

					if (KickStarter.runtimeInventory.SelectedItem == null && KickStarter.playerCursor.GetSelectedCursor () < 0)
					{
						string prefix = KickStarter.cursorManager.GetLabelFromID (useCursorID, language);
						return AdvGame.CombineLanguageString (prefix, hoverItemName, language);
					}
					else if (InvInstance.IsValid (KickStarter.runtimeInventory.SelectedInstance))
					{
						string selectedItemName = KickStarter.runtimeInventory.SelectedInstance.ItemLabel;

						if (KickStarter.runtimeInventory.SelectedItem == hoverItem)
						{
							if (KickStarter.runtimeInventory.SelectedInstance.SelectItemMode == SelectItemMode.Give)
							{
								string prefix = KickStarter.cursorManager.GetLabelFromID (giveCursorID, language);
								string combine1 = AdvGame.CombineLanguageString (prefix, selectedItemName, language);
								string prefix2 = KickStarter.runtimeLanguages.GetTranslation (KickStarter.cursorManager.hotspotPrefix4.label, KickStarter.cursorManager.hotspotPrefix4.lineID, language, KickStarter.cursorManager.hotspotPrefix4.GetTranslationType (0));
								return AdvGame.CombineLanguageString (combine1, prefix2, language);
							}
							else
							{
								string prefix = KickStarter.cursorManager.GetLabelFromID (useCursorID, language);
								string combine1 = AdvGame.CombineLanguageString (prefix, selectedItemName, language);
								string prefix2 = KickStarter.runtimeLanguages.GetTranslation (KickStarter.cursorManager.hotspotPrefix2.label, KickStarter.cursorManager.hotspotPrefix2.lineID, language, KickStarter.cursorManager.hotspotPrefix2.GetTranslationType (0));
								return AdvGame.CombineLanguageString (combine1, prefix2, language);
							}
						}
						else
						{
							// Combine two items, i.e. "Use worm on apple"
							string prefix = KickStarter.runtimeInventory.SelectedInstance.GetHotspotPrefixLabel (language, true);

							InvInstance hoverInstance = inventoryBox.GetInstance (slot);
							string slotItemLabel = hoverInstance.ItemLabel;
							if (hoverInstance.InvItem.canBeLowerCase && !string.IsNullOrEmpty (prefix))
							{
								slotItemLabel = slotItemLabel.ToLower ();
							}
							return AdvGame.CombineLanguageString (prefix, slotItemLabel, language);
						}
					}
				}
			}
			return string.Empty;
		}


		private void OnMenuElementClick (Menu menu, MenuElement element, int slot, int buttonPressed)
		{
			MenuInventoryBox inventoryBox = element as MenuInventoryBox;
			if (inventoryBox != null)
			{
				InvInstance hoverInstance = inventoryBox.GetInstance (slot);
				if (InvInstance.IsValid (hoverInstance))
				{
					if (buttonPressed == 1)
					{
						if (InvInstance.IsValid (KickStarter.runtimeInventory.SelectedInstance))
						{
							if (KickStarter.runtimeInventory.SelectedInstance == hoverInstance)
							{
								KickStarter.runtimeInventory.SetNull ();
							}
							else
							{
								KickStarter.runtimeInventory.SelectedInstance.Combine (hoverInstance);
							}
						}
						else
						{
							if (KickStarter.playerCursor.GetSelectedCursor () == -1)
							{
								hoverInstance.Use (useCursorID);
							}
							else
							{
								hoverInstance.Use (KickStarter.playerCursor.GetSelectedCursorID ());
							}
						}
					}
					else if (buttonPressed == 2)
					{
						if (InvInstance.IsValid (KickStarter.runtimeInventory.SelectedInstance))
						{
							hoverInstance.Combine (KickStarter.runtimeInventory.SelectedInstance);
						}
					}
					KickStarter.playerInput.ResetClick ();
				}
			}
		}


		private void OnAfterChangeScene (LoadingGame loadingGame)
		{
			overrideText = string.Empty;
		}

		#endregion


		#region PrivateFunctions

		private void ClearOverrideText ()
		{
			SetOverrideText (string.Empty);
		}


		private void SetOverrideText (string text)
		{
			overrideText = text;
		}


		private void InitVerbButtons ()
		{
			List<MenuInteraction> verbElementsList = new List<MenuInteraction> ();
			foreach (MenuElement element in menu.elements)
			{
				MenuInteraction menuInteraction = element as MenuInteraction;
				if (menuInteraction != null)
				{
					verbElementsList.Add (menuInteraction);
				}
			}
			verbElements = verbElementsList.ToArray ();

			if (verbElements.Length > 0)
			{
				UnityEngine.UI.Button button = verbElements[0].uiButton;
				if (button != null)
				{
					Color highlightedColor = button.colors.highlightedColor;
					normalColorBlock = highlightedColorBlock = button.colors;
					highlightedColorBlock.normalColor = highlightedColor;
				}
			}
		}

		#endregion

	}

}