using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if TextMeshProIsPresent
using TMPro;
#endif

namespace AC.Templates.GraphicOptions
{

	public class GraphicOptionsUI : MonoBehaviour
	{

		#region Variables

		[Header ("Settings")]
		[SerializeField] private string globalStringVariable = "GraphicOptionsData";
		[SerializeField] private int minimumResolutionHeight = 0;
		[SerializeField] private bool ignoreRefreshRates;
		[SerializeField] private List<float> supportedAspectRatios = new List<float> { 16/9f, 16/10f, 21/9f	};
		private List<Resolution> resolutions = new List<Resolution>();
		[SerializeField] private UnusedAdvancedOptions unusedAdvancedOptions = UnusedAdvancedOptions.Normal;
		private enum UnusedAdvancedOptions { Normal, Disabled, NonInteractable };

		[Header ("Components")]
		[SerializeField] private GameObject resolutionDropdown = null;
		[SerializeField] private CanvasGroup advancedOptionsCanvasGroup = null;
		[SerializeField] private Toggle fullScreenToggle = null;
		[SerializeField] private GameObject qualityPresetDropdown = null;
		[SerializeField] private GameObject antiAliasingDropdown = null;
		[SerializeField] private GameObject textureQualityDropdown = null;
		[SerializeField] private GameObject vSyncDropdown = null;
		private int nonCustomQualityLevel;

		#endregion


		#region UnityStandards

		private void OnEnable ()
		{
			UpdateUIValues ();
		}


		private void Start ()
		{
			// Add events
			AddDropdownEvents ();
		}

		#endregion


		#region PublicFunctions

		public void SaveAndApply ()
		{
			GVar gVar = GlobalVariables.GetVariable (globalStringVariable);
			if (gVar != null && gVar.type == VariableType.String)
			{
				GraphicOptionsData graphicOptionsData = new GraphicOptionsData (GetDropdownValue (resolutionDropdown), fullScreenToggle.isOn, IsCustomQuality ? nonCustomQualityLevel : GetDropdownValue (qualityPresetDropdown), IsCustomQuality, GetDropdownValue (antiAliasingDropdown), GetDropdownValue (textureQualityDropdown), GetDropdownValue (vSyncDropdown));
				gVar.TextValue = JsonUtility.ToJson (graphicOptionsData);
				Options.SavePrefs ();
			}
			else
			{
				ACDebug.LogWarning ("Could not apply Graphic Options data because no Global String variable was found", this);
			}

			Apply ();
		}


		public void Apply ()
		{
			GraphicOptionsData graphicOptionsData = GetSaveData ();
			if (graphicOptionsData != null)
			{
				graphicOptionsData.Apply (Resolutions);
			}
		}


		public void OnSetAdvancedOption ()
		{
			SetDropdownValue (qualityPresetDropdown, GetDropdownCount (qualityPresetDropdown) - 1);
		}


		public void OnSetQualityPreset ()
		{
			if (GetDropdownValue (qualityPresetDropdown) < GetDropdownCount (qualityPresetDropdown))
			{
				QualitySettings.SetQualityLevel (GetDropdownValue (qualityPresetDropdown), false);
				UpdateAdvancedUIValues ();

				nonCustomQualityLevel = GetDropdownValue (qualityPresetDropdown);
				//QualitySettings.SetQualityLevel (nonCustomQualityLevel, false);
			}
		}

		#endregion


		#region PrivateFunctions

		private int AAToIndex (int level)
		{
			switch (level)
			{
				case 0:
				default:
					return 0;

				case 2:
					return 1;

				case 4:
					return 2;

				case 8:
					return 3;
			}
		}


		private void UpdateUIValues ()
		{
			// Advanced options
			GraphicOptionsData graphicOptionsData = GetSaveData ();
			bool usingAdvancedOptions = (graphicOptionsData != null) ? graphicOptionsData.UsingAdvancedOptions : false;

			// Resolution
			{
				List<string> resolutionLabels = new List<string> ();
				int resolutionIndex = -1;
				for (int i = 0; i < Resolutions.Count; i++)
				{
					if (ResolutionsAreEqual (Resolutions[i], Screen.currentResolution))
					{
						resolutionIndex = i;
					}

					resolutionLabels.Add (GetResolutionLabel (Resolutions[i]));
				}

				SetDropdownOptions (resolutionDropdown, resolutionLabels.ToArray ());
				if (resolutionIndex >= 0) SetDropdownValue (resolutionDropdown, resolutionIndex);
				else
				{
					SetDropdownCaption (resolutionDropdown, GetResolutionLabel (Screen.currentResolution));
				}
			}

			// Full-screen
			if (fullScreenToggle)
			{
				fullScreenToggle.isOn = Screen.fullScreen;
			}

			// Quality preset
			{
				List<string> qualityPresetLabels = new List<string> ();
				foreach (string qualityName in QualitySettings.names)
				{
					qualityPresetLabels.Add (qualityName);
				}
				qualityPresetLabels.Add ("Custom");
				SetDropdownOptions (qualityPresetDropdown, qualityPresetLabels.ToArray ());
				if (usingAdvancedOptions)
				{
					SetDropdownValue (qualityPresetDropdown, GetDropdownCount (qualityPresetDropdown));
				}
				else
				{
					SetDropdownValue (qualityPresetDropdown, QualitySettings.GetQualityLevel ());
				}
				nonCustomQualityLevel = QualitySettings.GetQualityLevel ();
			}

			UpdateAdvancedUIValues ();
		}
		

		private void UpdateAdvancedUIValues ()
		{
			// Anti-aliasing
			int antiAliasingValue = AAToIndex (QualitySettings.antiAliasing);
			SetDropdownValue (antiAliasingDropdown, antiAliasingValue);

			// Texture quality
#if UNITY_2022_3_OR_NEWER
			SetDropdownValue (textureQualityDropdown, QualitySettings.globalTextureMipmapLimit);
#else
			SetDropdownValue (textureQualityDropdown, QualitySettings.masterTextureLimit);
#endif

			// Vsync
			SetDropdownValue (vSyncDropdown, QualitySettings.vSyncCount);

			if (advancedOptionsCanvasGroup)
			{
				switch (unusedAdvancedOptions)
				{
					case UnusedAdvancedOptions.Disabled:
						advancedOptionsCanvasGroup.gameObject.SetActive (IsCustomQuality);
						break;

					case UnusedAdvancedOptions.NonInteractable:
						advancedOptionsCanvasGroup.interactable = IsCustomQuality;
						break;

					default:
						break;
				}
			}
		}


		private GraphicOptionsData GetSaveData ()
		{
			GVar gVar = GlobalVariables.GetVariable (globalStringVariable);
			if (gVar != null && gVar.type == VariableType.String)
			{
				string optionsDataString = gVar.TextValue;
				if (!string.IsNullOrEmpty (optionsDataString))
				{
					GraphicOptionsData graphicOptionsData = JsonUtility.FromJson<GraphicOptionsData> (optionsDataString);
					return graphicOptionsData;
				}
				return null;
			}
			else
			{
				ACDebug.LogWarning ("Could not apply Graphic Options data because no Global String variable was found", this);
				return null;
			}
		}


		private void SetDropdownValue (GameObject dropdownObject, int value)
		{
			if (dropdownObject == null) return;

			#if TextMeshProIsPresent
			TMP_Dropdown dropdownTMP = dropdownObject.GetComponent<TMP_Dropdown> ();
			if (dropdownTMP)
			{
				dropdownTMP.SetValueWithoutNotify (value);
				dropdownTMP.RefreshShownValue ();
				return;
			}
			#endif
			Dropdown dropdown = dropdownObject.GetComponent<Dropdown> ();
			if (dropdown)
			{
				dropdown.SetValueWithoutNotify (value);
				dropdown.RefreshShownValue ();
			}
		}


		private void SetDropdownCaption (GameObject dropdownObject, string text)
		{
			if (dropdownObject == null) return;

			#if TextMeshProIsPresent
			TMP_Dropdown dropdownTMP = dropdownObject.GetComponent<TMP_Dropdown> ();
			if (dropdownTMP && dropdownTMP.captionText)
			{
				dropdownTMP.captionText.text = text;
				return;
			}
			#endif
			Dropdown dropdown = dropdownObject.GetComponent<Dropdown> ();
			if (dropdown && dropdown.captionText)
			{
				dropdown.captionText.text = text;
			}
		}


		private void SetDropdownOptions (GameObject dropdownObject, string[] labels)
		{
			if (dropdownObject == null) return;

			#if TextMeshProIsPresent
			TMP_Dropdown dropdownTMP = dropdownObject.GetComponent<TMP_Dropdown> ();
			if (dropdownTMP)
			{
				dropdownTMP.options.Clear ();
				foreach (string label in labels)
				{
					dropdownTMP.options.Add (new TMP_Dropdown.OptionData (label));
				}
				return;
			}
			#endif
			Dropdown dropdown = dropdownObject.GetComponent<Dropdown> ();
			if (dropdown)
			{
				dropdown.options.Clear ();
				foreach (string label in labels)
				{
					dropdown.options.Add (new Dropdown.OptionData (label));
				}
			}
		}

		
		private int GetDropdownValue (GameObject dropdownObject)
		{
			if (dropdownObject == null) return 0;

			#if TextMeshProIsPresent
			TMP_Dropdown dropdownTMP = dropdownObject.GetComponent<TMP_Dropdown> ();
			if (dropdownTMP) return dropdownTMP.value;
			#endif
			Dropdown dropdown = dropdownObject.GetComponent<Dropdown> ();
			if (dropdown) return dropdown.value;
			return 0;
		}


		private int GetDropdownCount (GameObject dropdownObject)
		{
			if (dropdownObject == null) return 0;

			#if TextMeshProIsPresent
			TMP_Dropdown dropdownTMP = dropdownObject.GetComponent<TMP_Dropdown> ();
			if (dropdownTMP) return dropdownTMP.options.Count;
			#endif
			Dropdown dropdown = dropdownObject.GetComponent<Dropdown> ();
			if (dropdown) return dropdown.options.Count;
			return 0;
		}


		private void AddDropdownEvents ()
		{
			if (qualityPresetDropdown)
			{
				#if TextMeshProIsPresent
				TMP_Dropdown dropdownTMP = qualityPresetDropdown.GetComponent<TMP_Dropdown> ();
				if (dropdownTMP) dropdownTMP.onValueChanged.AddListener (delegate { OnSetQualityPreset (); });
				#endif
				Dropdown dropdown = qualityPresetDropdown.GetComponent<Dropdown> ();
				if (dropdown) dropdown.onValueChanged.AddListener (delegate { OnSetQualityPreset (); });
			}

			if (antiAliasingDropdown)
			{
				#if TextMeshProIsPresent
				TMP_Dropdown dropdownTMP = antiAliasingDropdown.GetComponent<TMP_Dropdown> ();
				if (dropdownTMP) dropdownTMP.onValueChanged.AddListener (delegate { OnSetAdvancedOption (); });
				#endif
				Dropdown dropdown = antiAliasingDropdown.GetComponent<Dropdown> ();
				if (dropdown) dropdown.onValueChanged.AddListener (delegate { OnSetAdvancedOption (); });
			}

			if (textureQualityDropdown)
			{
				#if TextMeshProIsPresent
				TMP_Dropdown dropdownTMP = textureQualityDropdown.GetComponent<TMP_Dropdown> ();
				if (dropdownTMP) dropdownTMP.onValueChanged.AddListener (delegate { OnSetAdvancedOption (); });
				#endif
				Dropdown dropdown = textureQualityDropdown.GetComponent<Dropdown> ();
				if (dropdown) dropdown.onValueChanged.AddListener (delegate { OnSetAdvancedOption (); });
			}

			if (vSyncDropdown)
			{
				#if TextMeshProIsPresent
				TMP_Dropdown dropdownTMP = vSyncDropdown.GetComponent<TMP_Dropdown> ();
				if (dropdownTMP) dropdownTMP.onValueChanged.AddListener (delegate { OnSetAdvancedOption (); });
				#endif
				Dropdown dropdown = vSyncDropdown.GetComponent<Dropdown> ();
				if (dropdown) dropdown.onValueChanged.AddListener (delegate { OnSetAdvancedOption (); });
			}
		}


		private bool ResolutionsAreEqual (Resolution resolutionA, Resolution resolutionB)
		{
			if (resolutionA.width == resolutionB.width &&
				resolutionA.height == resolutionB.height &&
				(ignoreRefreshRates ||
#if UNITY_2022_3_OR_NEWER
				resolutionA.refreshRateRatio.value == resolutionB.refreshRateRatio.value))
#else
				resolutionA.refreshRate == resolutionB.refreshRate))
#endif
			{
				return true;
			}
			return false;
		}


		private string GetResolutionLabel (Resolution resolution)
		{
#if UNITY_2022_3_OR_NEWER
			return resolution.width.ToString () + " x " + resolution.height.ToString () + (ignoreRefreshRates ? string.Empty : (" " + resolution.refreshRateRatio.value.ToString () + " hz"));
#else
			return resolution.width.ToString () + " x " + resolution.height.ToString () + (ignoreRefreshRates ? string.Empty : (" " + resolution.refreshRate.ToString () + " hz"));
#endif
		}

		#endregion


		#region GetSet

		public bool IsCustomQuality
		{
			get
			{
				return GetDropdownValue (qualityPresetDropdown) == GetDropdownCount (qualityPresetDropdown) - 1;
			}
		}


		private List<Resolution> Resolutions
		{
			get
			{
				#if !UNITY_EDITOR
				if (resolutions.Count == 0)
				#endif
				{
					resolutions.Clear ();
					for (int i = 0; i < Screen.resolutions.Length; i++)
					{
						if (supportedAspectRatios.Count == 0 || supportedAspectRatios.Contains ((float) Screen.resolutions[i].width / (float) Screen.resolutions[i].height))
						{
							if (Screen.resolutions[i].height >= minimumResolutionHeight)
							{
								bool canAdd = true;
								if (ignoreRefreshRates)
								{
									foreach (Resolution resolution in resolutions)
									{
										if (ResolutionsAreEqual (resolution, Screen.resolutions[i]))
										{
											canAdd = false;
											break;
										}
									}
								}

								if (canAdd)
								{
									resolutions.Add (Screen.resolutions[i]);
								}
							}
						}
					}
				}
				return resolutions;
			}
		}

		#endregion

	}


	[Serializable]
	public class GraphicOptionsData
	{

		#region Variables

		[SerializeField] private int screenResolutionIndex;
		[SerializeField] private bool isFullScreen;
		[SerializeField] private int qualityPresetIndex;
		[SerializeField] private bool usingAdvancedOptions;
		[SerializeField] private int antiAliasingLevel;
		[SerializeField] private int textureQualityLevel;
		[SerializeField] private int vSyncCount;

		#endregion


		#region Constructors

		public GraphicOptionsData (int _screenResolutionIndex, bool _isFullScreen, int _qualityPresetIndex, bool _usingAdvancedOptions, int _antiAliasingLevel, int _textureQualityLevel, int _vSyncCount)
		{
			screenResolutionIndex = _screenResolutionIndex;
			isFullScreen = _isFullScreen;
			qualityPresetIndex = _qualityPresetIndex;
			usingAdvancedOptions = _usingAdvancedOptions;
			antiAliasingLevel = _antiAliasingLevel;
			textureQualityLevel = _textureQualityLevel;
			vSyncCount = _vSyncCount;
		}

		#endregion


		#region PublicFunctions

		public void Apply (List<Resolution> resolutions)
		{
			if (screenResolutionIndex < resolutions.Count)
			{
				Resolution chosenResolution = resolutions[screenResolutionIndex];
				Screen.SetResolution (chosenResolution.width, chosenResolution.height, isFullScreen);
			}
			else
			{
				#if UNITY_EDITOR
				ACDebug.LogWarning ("Invalid resolution index. This may be due to an attempt to set the resolution in the Editor.");
				#else
				ACDebug.LogWarning ("Invalid resolution index.");
				#endif
			}

			QualitySettings.SetQualityLevel (qualityPresetIndex, true);
			if (usingAdvancedOptions)
			{
				QualitySettings.antiAliasing = IndexToAA (antiAliasingLevel);
#if UNITY_2022_3_OR_NEWER
				QualitySettings.globalTextureMipmapLimit = textureQualityLevel;
#else
				QualitySettings.masterTextureLimit = textureQualityLevel;
#endif
				QualitySettings.vSyncCount = vSyncCount;
			}

			KickStarter.playerMenus.RecalculateAll ();
		}

		#endregion


		#region PrivateFunctions

		private int IndexToAA (int index)
		{
			return (int) Mathf.Pow (2, index);
		}

		#endregion
		

		#region GetSet

		public bool UsingAdvancedOptions { get { return usingAdvancedOptions; } }

		#endregion

	}

}